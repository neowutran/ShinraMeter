using Data;
using Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Tera.Game;

namespace DamageMeter.TeraDpsApi
{
    public class DpsServer
    {

        public static readonly List<AreaAllowed> DefaultAreaAllowed = new List<AreaAllowed> {
            new AreaAllowed(770),
            new AreaAllowed(769),
            new AreaAllowed(916),
            new AreaAllowed(969),
            new AreaAllowed(970),
            new AreaAllowed(710),
            new AreaAllowed(780),
            new AreaAllowed(980),
            new AreaAllowed(781),
            new AreaAllowed(981),
            new AreaAllowed(950, new List<int>{ 1000, 2000, 3000, 4000})
        };

        public static DpsServer NeowutranAnonymousServer => new DpsServer(DpsServerData.Neowutran, true);

        public DpsServer(DpsServerData data, bool anonymousUpload)
        {
            AnonymousUpload = anonymousUpload;
            Guid = Guid.NewGuid();
        }


       public bool CheckInputAndSend(EncounterBase teradpsData, NpcEntity entity)
        {
            if (!Data.Enabled && !AnonymousUpload) { return false; }
            var areaId = int.Parse(teradpsData.areaId);

            try
            {
                if (_allowedAreaId.Count == 0) { try { FetchAllowedAreaId(); } catch { return false; } }
                if (!_allowedAreaId.Any(x => x.AreaId == areaId && ( x.BossIds.Count == 0 || x.BossIds.Contains((int)entity.Info.TemplateId)))) { return false; }

                long timediff;
                try { timediff = FetchServerTime(entity); } catch { return false; }

                teradpsData.encounterUnixEpoch += timediff;
                var json = JsonConvert.SerializeObject(teradpsData,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.None });
                teradpsData.encounterUnixEpoch -= timediff;
                Send(entity, json, 3);
            }
            catch
            {
                NetworkController.Instance.BossLink.TryAdd(
                       "!" + Guid + " " + LP.TeraDpsIoApiError + " " + entity.Info.Name + " " + DateTime.UtcNow.Ticks, entity);
                return false;
            }
            return true;
         }

        private void Send(NpcEntity npc, string json, int retry = 3)
        {
    
          try
          {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Auth-Token", Data.Token);
                client.DefaultRequestHeaders.Add("X-Auth-Username", Data.Username);
                client.DefaultRequestHeaders.Add("X-Local-Time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                client.Timeout = TimeSpan.FromSeconds(40);
                var response = client.PostAsync(Data.UploadUrl, new StringContent(json, Encoding.UTF8, "application/json"));
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
                Send(npc,json, --retry);
          }
        }

        public long FetchServerTime(NpcEntity entity)
        {
            var serverTimeUrl = String.IsNullOrWhiteSpace(Data.ServerTimeUrl.ToString()) ? HomeUrl : Data.ServerTimeUrl;
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
                    var response = client.GetAsync(Data.AllowedAreaUrl);
                    allowedAreaIdByServer = JsonConvert.DeserializeObject<List<AreaAllowed>>(response.Result.Content.ReadAsStringAsync().Result);
                }
                catch
                {
                    //TODO logs
                    allowedAreaIdByServer = new List<AreaAllowed> (DefaultAreaAllowed);
                }
                ComputeAllowedAreaId(allowedAreaIdByServer);
            }
        }

        private void ComputeAllowedAreaId(List<AreaAllowed> allowedAreaIdByServer)
        {
            _allowedAreaId = allowedAreaIdByServer;
            _allowedAreaId.RemoveAll(x => !BasicTeraData.Instance.WindowData.WhiteListAreaId.Contains(x.AreaId));
        }

        public Uri HomeUrl => new Uri(Data.UploadUrl?.GetLeftPart(UriPartial.Authority));
        private List<AreaAllowed> _allowedAreaId = new List<AreaAllowed>();
        public Guid Guid { get; private set; }
        public bool AnonymousUpload { get; private set; }

        public DpsServerData Data { get; set; }

    }
}
