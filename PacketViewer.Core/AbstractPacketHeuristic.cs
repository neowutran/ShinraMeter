using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;
using OpcodeId = System.UInt16;
using Grade = System.UInt32;

namespace PacketViewer
{
    public abstract class AbstractPacketHeuristic
    {
        protected OpcodeEnum OPCODE => (OpcodeEnum)Enum.Parse(typeof(OpcodeEnum), GetType().Name);
        public bool IsKnown => OpcodeFinder.Instance.IsKnown(OPCODE);
        public OpcodeId? KnownOpcode => OpcodeFinder.Instance.GetOpcode(OPCODE);
        protected TeraMessageReader Reader;
        public void Process(ParsedMessage message){
            // Allow to read the packet structure for check & data extracting
            Reader = new TeraMessageReader(message);
        }

        public bool IsSamePacket(OpcodeId opcode) {
            return IsKnown && KnownOpcode == opcode;
        }
        // Used for probability: for difficult packet, can grade the probility of each opcode to be the good one, then a the end, 
        // compute what opcode have the best grade
        public List<Tuple<OpcodeId, Grade>> OpcodeProbablity = new List<Tuple<OpcodeId, Grade>>();

        public Dictionary<OpcodeId, Grade> GetGrades()
        {

            var sumGrade = new Dictionary<OpcodeId, Tuple<int, long>>();
            foreach(var packetGrade in OpcodeProbablity)
            {
                if (!sumGrade.ContainsKey(packetGrade.Item1))
                {
                    sumGrade.Add(packetGrade.Item1, new Tuple<int, long>(0, 0));
                }
                sumGrade[packetGrade.Item1] = new Tuple<int, long>(sumGrade[packetGrade.Item1].Item1 + 1, sumGrade[packetGrade.Item1].Item2 + packetGrade.Item2);
            }

            var result = new Dictionary<OpcodeId, Grade>();
            foreach (var grade in sumGrade)
            {
                result.Add(grade.Key, (Grade)(grade.Value.Item2 / grade.Value.Item1));
            }
            return result;
        }

        public OpcodeId? GetOpcodeWithBestGrade()
        {
            var grades = GetGrades();
            if(grades.Count == 0) { return null; }
            return grades.FirstOrDefault(x => x.Value == grades.Values.Max()).Key;
        }

    }
}
