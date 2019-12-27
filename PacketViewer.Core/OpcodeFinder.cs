using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tera;
using Tera.Game.Messages;
using OpcodeId = System.UInt16;
using Grade = System.UInt32;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Reflection;

namespace PacketViewer
{
    public class OpcodeFinder
    {
        public static OpcodeFinder Instance => _instance ?? (_instance = new OpcodeFinder());
        private static OpcodeFinder _instance;
        private static bool _viewOnly = false;

        private OpcodeFinder() {
            NetworkController.Instance.UiUpdateKnownOpcode = new Dictionary<OpcodeId, OpcodeEnum>
            {
                { 19900, OpcodeEnum.C_CHECK_VERSION },
                { 19901, OpcodeEnum.S_CHECK_VERSION }
            };
            NetworkController.Instance.UiUpdateData = new List<ParsedMessage>();
            var mainMethod = "Process";
            var classes = AppDomain.CurrentDomain.GetAssemblies()
            .Select(x => x.GetTypes())
            .SelectMany(x => x)
            .Where(x => x.Namespace == typeof(Heuristic.C_CHECK_VERSION).Namespace)
            .Where(x => x.GetMethod(mainMethod) != null);

            foreach (Type cl in classes)
            {

                var method = cl.GetMethod(mainMethod);
                var obj = Activator.CreateInstance(method.DeclaringType);
                if (cl.Name.StartsWith("C_"))
                {
                    ClientOpcode.Add(method, obj);
                }
                else if (cl.Name.StartsWith("S_"))
                {
                    ServerOpcode.Add(method, obj);
                }
                else
                {
                    throw new Exception("invalid class name");
                }
            }
        }

        public enum KnowledgeDatabaseItem
        {
            LoggedCharacter = 0,
            PlayerLocation = 1,
            Characters = 2,
            SpawnedUsers = 3,
            SpawnedNpcs = 4,
            LoggedCharacterAbnormalities = 5,
            CharacterSpawnedSuccesfully = 6,
            PartyMemberList = 7,
        }

        public bool OpcodePartialMatch()
        {
            var opcodeFile = NetworkController.Instance.LoadOpcodeCheck;
            NetworkController.Instance.LoadOpcodeCheck = null;
            var file = new System.IO.StreamReader(opcodeFile);
            string line;
            bool matched = true;
            while ((line = file.ReadLine()) != null)
            {
                line = line.Replace("=", "");
                var match = Regex.Match(line, @"(?i)^\s*([a-z_0-9]+)\s+(\d+)\s*$");
                Enum.TryParse(match.Groups[1].Value, out OpcodeEnum opcodeName);
                OpcodeId opcodeId = OpcodeId.Parse(match.Groups[2].Value);

                if(KnownOpcode.ContainsKey(opcodeId) && KnownOpcode[opcodeId] != opcodeName)
                {
                    Console.WriteLine("Incorrect match type 1 for " + KnownOpcode[opcodeId] + " : " + opcodeId);
                    matched = false;
                }else if(ReverseKnownOpcode.ContainsKey(opcodeName) && ReverseKnownOpcode[opcodeName] != opcodeId)
                {
                    Console.WriteLine("Incorrect match type 2 for " + opcodeName + " : " + opcodeId);
                    matched = false;
                }else if(!ReverseKnownOpcode.ContainsKey(opcodeName) && !KnownOpcode.ContainsKey(opcodeId))
                {

                    // Stay silent if the parser didn't found every opcode. 
                    // TODO: add option for strict match: aka -> this case generate error 
                    if (NetworkController.Instance.StrictCheck)
                    {
                        Console.WriteLine("Missing match for " + opcodeName + " : " + opcodeId);
                        matched = false;
                    }
                }
                else
                {
                    Console.WriteLine("Correct match for " + KnownOpcode[opcodeId] + " : " + opcodeId);
                }
            }

            file.Close();
            return matched;
        }



        public void OpcodeLoadKnown()
        {
            var opcodeFile = NetworkController.Instance.LoadOpcodeForViewing;
            NetworkController.Instance.LoadOpcodeForViewing = null;
            KnownOpcode.Clear();
            ReverseKnownOpcode.Clear();
            NetworkController.Instance.UiUpdateKnownOpcode.Clear();
            var file = new System.IO.StreamReader(opcodeFile);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                line = line.Replace("=", "");
                var match = Regex.Match(line, @"(?i)^\s*([a-z_0-9]+)\s+(\d+)\s*$");
                Enum.TryParse(match.Groups[1].Value, out OpcodeEnum opcodeName);
                OpcodeId opcodeId = OpcodeId.Parse(match.Groups[2].Value);
                SetOpcode(opcodeId, opcodeName);
            }
            file.Close();
            _viewOnly = true;
        }

        // Use that to set value like CID etc ...
        public ConcurrentDictionary<KnowledgeDatabaseItem,  object> KnowledgeDatabase = new ConcurrentDictionary<KnowledgeDatabaseItem, object>();
        private Dictionary<OpcodeId, OpcodeEnum> KnownOpcode = new Dictionary<OpcodeId, OpcodeEnum>()
        {
            { 19900, OpcodeEnum.C_CHECK_VERSION },
            { 19901, OpcodeEnum.S_CHECK_VERSION },
        };
        private Dictionary<OpcodeEnum, OpcodeId> ReverseKnownOpcode = new Dictionary<OpcodeEnum, OpcodeId>()
        {
            { OpcodeEnum.C_CHECK_VERSION, 19900 },
            { OpcodeEnum.S_CHECK_VERSION, 19901 },
        };

        // Once your module found a new opcode
        public void SetOpcode(OpcodeId opcode, OpcodeEnum opcodeName)
        {
            if (KnownOpcode.ContainsKey(opcode))
            {
                KnownOpcode.TryGetValue(opcode, out var value);
                throw new Exception("opcode: " + opcode + " is already know = " + value + " . You try to add instead = " + Enum.GetName(typeof(OpcodeEnum), opcodeName));
            }
            if (KnownOpcode.Values.Contains(opcodeName))
            {
                throw new Exception("opcodename: " + Enum.GetName(typeof(OpcodeEnum), opcodeName) + " is already know = " + opcode);
            }
            Console.WriteLine(opcode +" => "+opcodeName);
            ReverseKnownOpcode.Add(opcodeName, opcode);
            KnownOpcode.Add(opcode, opcodeName);
            NetworkController.Instance.UiUpdateKnownOpcode.Add(opcode, opcodeName);
        }

        public bool IsKnown(OpcodeId opcode) => KnownOpcode.ContainsKey(opcode);

        public bool IsKnown(OpcodeEnum opcode) => ReverseKnownOpcode.ContainsKey(opcode);

        public ushort? GetOpcode(OpcodeEnum opcode)
        {
            if (!ReverseKnownOpcode.ContainsKey(opcode)) { return null; }
            return ReverseKnownOpcode[opcode];
        }

        //For the kind of heuristic "only appear in the first X packet"
        public long PacketCount = 0;

        public void Find(ParsedMessage message)
        {
            PacketCount++;
            AllPackets.Add(PacketCount, message);
            NetworkController.Instance.UiUpdateData.Add(message);
            if (_viewOnly) { return; }
            if (message.Direction == MessageDirection.ClientToServer) {         
                Parallel.ForEach(ClientOpcode, x => x.Key.Invoke(x.Value, new object[] { message }));
            }
            else
            {                
                Parallel.ForEach(ServerOpcode, x => x.Key.Invoke(x.Value, new object[] { message }));
            }
        }

        // For the kind of heuristic "this opcode only appear less than 1 second after this other opcode"
        public Dictionary<long, ParsedMessage> AllPackets = new Dictionary<long, ParsedMessage>();
        public KeyValuePair<long, ParsedMessage>? LastOccurrence(OpcodeEnum opcode) {
            if (!IsKnown(opcode))
            {
                return null;
            }

            return AllPackets.Where(x => x.Value.OpCode == ReverseKnownOpcode[opcode]).Last();

        }

        // NOT TESTED
        public bool PacketSeenInTheLastXms(OpcodeEnum opcode, DateTime now, long ms)
        {
            if (!ReverseKnownOpcode.ContainsKey(opcode)) { return false; }
            for(var i = PacketCount; i > 0; --i)
            {
                var packet = AllPackets[i];
                if(packet.Time.Ticks < now.Ticks - TimeSpan.TicksPerMillisecond * ms)
                {
                    return false;
                }
                if(packet.OpCode == ReverseKnownOpcode[opcode]) { return true; }
            }
            return false;
        }

        public long TotalOccurrenceOpcode(OpcodeId opcode) => AllPackets.Where(x => x.Value.OpCode == opcode).Count();

        public ParsedMessage GetMessage(long messageNumber) => AllPackets[messageNumber];
        private readonly Dictionary<MethodInfo, object> ClientOpcode = new Dictionary<MethodInfo, object>();
        private readonly Dictionary<MethodInfo, object> ServerOpcode = new Dictionary<MethodInfo, object>();

        public void Reset()
        {
            _instance = null;
        }
    }
}