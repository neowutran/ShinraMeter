using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game;
using Tera.Game.Messages;
using Tera.PacketLog;

namespace PacketViewer
{
    public static class LogReader
    {
        public static List<Message> LoadLogFromFile(string filename)
        {
            var plf = new PacketLogFile(filename);
            var messageList = plf.Messages.ToList();
            if(messageList.ElementAt(0).OpCode != 19900)
            {
                throw new Exception("Analysing a log without starting by the beginning is meaningless");
            }
            if(messageList.ElementAt(0).Payload.Count == 30) // Old log, need to remove the last 2 bytes
            {
                Console.WriteLine("Old log detected, removing the last 2 bytes");
                Parallel.ForEach(messageList, message =>
                {
                    message.Data = new ArraySegment<byte>(message.Data.Array, 0, message.Data.Count - 2);
                });
            }
            return messageList;
        }
    }
}
