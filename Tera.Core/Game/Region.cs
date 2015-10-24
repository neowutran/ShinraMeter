using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Game
{
    public class Region
    {
        public string Key { get; private set; }
        public string Version { get; private set; }

        public Region(string key, string version)
        {
            Key = key;
            Version = version;
        }
    }
}
