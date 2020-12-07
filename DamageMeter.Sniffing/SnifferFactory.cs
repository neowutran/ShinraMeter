using System;
using System.Linq;
using Tera.Sniffing;

namespace DamageMeter.Sniffing
{
    public static class SnifferFactory
    {
        public static ITeraSniffer Create()
        {
            if(Environment.GetCommandLineArgs().Contains("--toolbox")) return new ToolboxSniffer();
            return new TeraSniffer();
        }
    }
}