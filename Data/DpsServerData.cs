using System;

namespace Data
{
    public class DpsServerData
    {

        public Uri UploadUrl { get; set; }
        public Uri AllowedAreaUrl { get; set; }
        public Uri ServerTimeUrl { get; set; }
        public Uri GlyphUrl { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public bool Enabled { get; set; }



        public static DpsServerData Neowutran => new DpsServerData()
        {
            UploadUrl = new Uri("https://neowutran.ovh/storage/store.php"),
            ServerTimeUrl = new Uri("https://neowutran.ovh/updates/"),
            Enabled = true
        };

        public static DpsServerData Moongourd => new DpsServerData()
        {
            UploadUrl = new Uri("https://moongourd.com/dpsmeter_data.php"),
            ServerTimeUrl = new Uri("https://moongourd.com/api/shinra/servertime"),
            AllowedAreaUrl = new Uri("https://moongourd.com/api/shinra/whitelist"),
            GlyphUrl = new Uri("https://moongourd.com/shared/glyph_data.php"),
            Enabled = false
        };

        public static DpsServerData TeraLogs => new DpsServerData()
        {
            UploadUrl = new Uri("http://teralogs.com/api/logs"),
            ServerTimeUrl = new Uri("https://teralogs.com/"),
            AllowedAreaUrl = new Uri("http://teralogs.com/api/logs/a/allow"), 
            Enabled = false
        };

    }
}
