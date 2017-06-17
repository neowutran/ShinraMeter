using System;

namespace Data
{
    public class DpsServerData
    {

        public Uri UploadUrl { get; set; }
        public Uri AllowedAreaUrl { get; set; }
        public Uri GlyphUrl { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public bool Enabled { get; set; }
        public string HostName => UploadUrl?.Host;

        public DpsServerData(Uri uploadUrl, Uri allowedAreaUrl, Uri glyphUrl, string username, string token, bool enabled)
        {
            UploadUrl = uploadUrl;
            AllowedAreaUrl = allowedAreaUrl;
            GlyphUrl = glyphUrl;
            Username = username;
            Token = token;
            Enabled = enabled;
        }

        public DpsServerData(DpsServerData data)
        {
            UploadUrl = data.UploadUrl;
            AllowedAreaUrl = data.AllowedAreaUrl;
            GlyphUrl = data.GlyphUrl;
            Username = data.Username;
            Token = data.Token;
            Enabled = data.Enabled;
        }

        public static DpsServerData Neowutran = new DpsServerData(new Uri("https://neowutran.ovh/storage/store.php"), null, null, null, null, true);

        public static DpsServerData Moongourd = new DpsServerData(
            new Uri("https://moongourd.com/dpsmeter_data.php"),
            new Uri("https://moongourd.com/api/shinra/whitelist"),
            new Uri("https://moongourd.com/shared/glyph_data.php"),
            null, null, false );

        public static DpsServerData TeraLogs = new DpsServerData(
            new Uri("http://teralogs.com/api/logs"),
            new Uri("http://teralogs.com/api/logs/a/allow"),
            null, null, null, false );
    }
}
