using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tera.Protocol.Game
{
    public struct RaceGenderClass
    {
        public Race Race { get; private set; }
        public Gender Gender { get; private set; }
        public PlayerClass Class { get; private set; }

        public int Raw
        {
            get
            {
                if ((byte)Race >= 50 || (byte)Gender >= 2 || (byte)Class >= 100)
                    throw new InvalidOperationException();
                return 10000 + 200 * (int)Race + 100 * (int)Gender + (int)Class;
            }
        }

        private static T ParseEnum<T>(string s)
        {
            return (T)Enum.Parse(typeof(T), s);
        }

        public RaceGenderClass(string race, string gender, string @class)
            : this()
        {
            Race = ParseEnum<Race>(race);
            Gender = ParseEnum<Gender>(gender);
            Class = ParseEnum<PlayerClass>(@class);
        }

        public RaceGenderClass(Race race, Gender gender, PlayerClass @class)
            : this()
        {
            Race = race;
            Gender = gender;
            Class = @class;
        }

        public RaceGenderClass(int raw)
            : this()
        {
            Race = (Race)(raw / 200 % 50);
            Gender = (Gender)(raw / 100 % 2);
            Class = (PlayerClass)(raw % 100);
            Debug.Assert(Raw == raw);
        }

        public IEnumerable<RaceGenderClass> Fallbacks()
        {
            yield return this;
            yield return new RaceGenderClass(Race.Common, Gender.Common, Class);
            yield return new RaceGenderClass(Race.Common, Gender.Common, PlayerClass.Common);
        }

        public override bool Equals(object obj)
        {

            if (!(obj is RaceGenderClass))
                return false;
            var other = (RaceGenderClass)obj;
            return (Race == other.Race) && (Gender == other.Gender) && (Class == other.Class);
        }

        public override int GetHashCode()
        {
            return (int)Race << 16 | (int)Gender << 8 | (int)Class;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Race, Gender, Class);
        }
    }
}
