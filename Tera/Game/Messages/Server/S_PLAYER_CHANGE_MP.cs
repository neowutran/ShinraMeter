using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System;
using System.CodeDom;

namespace Tera.Game.Messages
{


    public class S_PLAYER_CHANGE_MP : ParsedMessage
    {
        internal S_PLAYER_CHANGE_MP(TeraMessageReader reader) : base(reader)
        {
            MpRemaining = reader.ReadInt32();
            TotalMp = reader.ReadInt32();
            MpChange = reader.ReadInt16();
            Type = reader.ReadInt32();
            Unknow3 = reader.ReadInt16();
            TargetId = reader.ReadEntityId();
            SourceId = reader.ReadEntityId();
            Critical = reader.ReadInt16();


            Console.WriteLine("target = " + TargetId + ";Source:" + SourceId + ";Critical:" + Critical + ";Mp left:" + MpRemaining + ";Max MP:" + TotalMp + ";MpLost/Gain:" + MpChange + ";Type:" + Type + ";Unknow3:" + Unknow3);

        }

        public int Unknow3 { get; }
        public int MpChange { get; }

        public int Type { get; }


        public int MpRemaining { get; }

        public int TotalMp { get; }

        public int Critical { get; }



        public EntityId TargetId { get; }
        public EntityId SourceId { get; }

    }
}
