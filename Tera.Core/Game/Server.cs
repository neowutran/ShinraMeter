namespace Tera.Game
{
    public class Server
    {
        public Server(string name, string region, string ip)
        {
            Ip = ip;
            Name = name;
            Region = region;
        }

        public string Ip { get; private set; }
        public string Name { get; private set; }
        public string Region { get; private set; }
    }
}