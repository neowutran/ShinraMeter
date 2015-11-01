using System;
using Tera.Game.Messages;

namespace Tera.Game
{
    // NPCs and Mosters - Tera doesn't distinguish these
    public class NpcEntity : Entity
    {
        public NpcEntity(SpawnNpcServerMessage message)
            : base(message.Id)
        {
            Console.WriteLine("identifiant:" + message.Id);
            Console.WriteLine("### Data ###");
            var data = message.Data.Array;
            foreach (var partdata in data)
            {
                Console.Write(partdata + "-");
            }
            Console.WriteLine("### Payload ###");
            data = message.Payload.Array;
            foreach (var partdata in data)
            {
                Console.Write(partdata + "-");
            }
            Console.WriteLine("########");
            Console.WriteLine(message.OpCode);
            Console.WriteLine(message.OpCodeName);
        }
    }
}