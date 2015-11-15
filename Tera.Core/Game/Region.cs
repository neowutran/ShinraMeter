namespace Tera.Game
{
    public class Region
    {
        public Region(string key, string version)
        {
            Key = key;
            Version = version;
        }

        public string Key { get; private set; }
        public string Version { get; private set; }
    }
}