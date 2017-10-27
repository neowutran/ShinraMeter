using Data;
using Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.TeraDpsApi
{
    public class DpsServer
    {
        public static readonly List<AreaAllowed> DefaultAreaAllowed = JsonConvert.DeserializeObject<List<AreaAllowed>>(
            "[{\"AreaId\": 735,\"BossIds\": []},{\"AreaId\": 935,\"BossIds\": []},{\"AreaId\": 950,\"BossIds\": [1000, 2000, 3000, 4000]},{\"AreaId\": 794,\"BossIds\": []},{\"AreaId\": 994,\"BossIds\": []},{\"AreaId\": 916,\"BossIds\": [1000, 91606]}]"
            );
        public static DpsServer NeowutranAnonymousServer => new DpsServer(DpsServerData.Neowutran, true);

        public bool Enabled => Data.Enabled;
        public Uri UploadUrl => Data.UploadUrl;
        public Uri AllowedAreaUrl => Data.AllowedAreaUrl;
        public Uri GlyphUrl => Data.GlyphUrl;
        public string Token => Data.Token;
        public string Username => Data.Username;

        public DpsServerData Data;
        public DpsServer(DpsServerData data, bool anonymousUpload)
        {  
            AnonymousUpload = anonymousUpload;
            Guid = Guid.NewGuid();
            Data = data;
            Debug.WriteLine("dps url:"+data.UploadUrl + ";enabled:" + Enabled + ";anonymous:" + AnonymousUpload);
        }


       public bool CheckAndSendFightData(EncounterBase teradpsData, NpcEntity entity)
        {
            if (!Enabled && !AnonymousUpload || String.IsNullOrWhiteSpace(UploadUrl?.ToString())) { return false; }
            var areaId = int.Parse(teradpsData.areaId);

            try
            {
                if (_allowedAreaId.Count == 0) { FetchAllowedAreaId(); }
                if (!_allowedAreaId.Any(x => x.AreaId == areaId && ( x.BossIds.Count == 0 || x.BossIds.Contains((int)entity.Info.TemplateId)))) { return false; }

                long timediff;
                try { timediff = FetchServerTime(entity); } catch { return false; }

                teradpsData.encounterUnixEpoch += timediff;
                var json = JsonConvert.SerializeObject(teradpsData,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.None });
                teradpsData.encounterUnixEpoch -= timediff;
                SendFightData(entity, json, 3);
            }
            catch
            {
                NetworkController.Instance.BossLink.TryAdd(
                       "!" + Guid + " " + LP.TeraDpsIoApiError + " " + entity.Info.Name + " " + DateTime.UtcNow.Ticks, entity);
                return false;
            }
            return true;
         }

        public bool SendGlyphData()
        {
            if (!Enabled||String.IsNullOrWhiteSpace(GlyphUrl?.ToString())) { return false ; }

            var json = JsonConvert.SerializeObject(NetworkController.Instance.Glyphs,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.None });
            Task.Run(() =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("X-Auth-Token", Token);
                        client.DefaultRequestHeaders.Add("X-Auth-Username", Username);

                        client.Timeout = TimeSpan.FromSeconds(40);
                        var response = client.PostAsync(GlyphUrl, new StringContent(json, Encoding.UTF8, "application/json"));

                        var responseString = response.Result.Content.ReadAsStringAsync();
                        Debug.WriteLine(responseString.Result);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            });

            return true;
        }

        private void SendFightData(NpcEntity npc, string json, int retry = 3)
        {
    
          try
          {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Auth-Token", Token);
                client.DefaultRequestHeaders.Add("X-Auth-Username", Username);
                client.DefaultRequestHeaders.Add("X-Local-Time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                client.Timeout = TimeSpan.FromSeconds(40);
                var response = client.PostAsync(UploadUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result.Content.ReadAsStringAsync().Result);
                if (responseObject.ContainsKey("id") && ((string)responseObject["id"]).StartsWith("http"))
                {
                    NetworkController.Instance.BossLink.TryAdd((string)responseObject["id"], npc);
                }
                else
                {
                    NetworkController.Instance.BossLink.TryAdd(
                        "!" + Guid + " " + (string)responseObject["message"] + " " + npc.Info.Name + " " + DateTime.UtcNow.Ticks, npc);
                }
            }
          }
          catch
          {
                //Network issue or server respond with shitty value
                //TODO logs
                if(retry <= 1) { throw;}
                Thread.Sleep(10000);
                SendFightData(npc,json, --retry);
          }
        }

        public long FetchServerTime(NpcEntity entity)
        {
            var serverTimeUrl = new Uri(HomeUrl+"/api/shinra/servertime");
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(40);
                    var response = client.GetAsync(serverTimeUrl);
                    return (response.Result.Headers.Date.Value.UtcDateTime.Ticks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerSecond;
                }
            }
            catch
            {
                // network issue
                //TODO logs
                throw;
            }
        }


        public void FetchAllowedAreaId()
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(40);
                List<AreaAllowed> allowedAreaIdByServer;
                try
                {
                    var response = client.GetAsync(AllowedAreaUrl);
                    var allwedAreaIdByServerString = response.Result.Content.ReadAsStringAsync().Result;
                    allowedAreaIdByServer = JsonConvert.DeserializeObject<List<AreaAllowed>>(allwedAreaIdByServerString);
                    Debug.WriteLine("Allowed Area Id successfully retrieved for "+AllowedAreaUrl+" : "+ allwedAreaIdByServerString);
                }
                catch
                {
                    allowedAreaIdByServer = new List<AreaAllowed> (DefaultAreaAllowed);
                    Debug.WriteLine("Allowed Area Id retrieve failed for " + AllowedAreaUrl + " , using default values");
                    // TODO, display to error to a UI ?
                }
                ComputeAllowedAreaId(allowedAreaIdByServer);
            }
        }

        private void ComputeAllowedAreaId(List<AreaAllowed> allowedAreaIdByServer)
        {
            _allowedAreaId = allowedAreaIdByServer;
            _allowedAreaId.RemoveAll(x => BasicTeraData.Instance.WindowData.BlackListAreaId.Contains(x.AreaId));
        }

        public Uri HomeUrl => UploadUrl==null ? null : new Uri(UploadUrl.GetLeftPart(UriPartial.Authority));
        private List<AreaAllowed> _allowedAreaId = new List<AreaAllowed>();
        public Guid Guid { get; private set; }
        public bool AnonymousUpload { get; private set; }
    }
}
