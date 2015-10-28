using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tera.Game
{
    // Tera often uses a this tuple of Race, Gender and Class. For example for looking up skills
    public struct RaceGenderClass
    {
        public Race Race { get; private set; }
        public Gender Gender { get; private set; }
        public PlayerClass Class { get; private set; }

        public int Raw
        {
            get
            {
                if ((byte) Race >= 50 || (byte) Gender >= 2 || (byte) Class >= 100)
                    throw new InvalidOperationException();
                return 10000 + 200*(int) Race + 100*(int) Gender + (int) Class;
            }
            private set
            {
                if (value/10000 != 1)
                    throw new ArgumentException(string.Format("Unexpected raw value for RaceGenderClass {0}", value));
                Race = (Race) (value/200%50);
                Gender = (Gender) (value/100%2);
                Class = (PlayerClass) (value%100);
                Debug.Assert(Raw == value);
            }
        }

        private static T ParseEnum<T>(string s)
        {
            return (T) Enum.Parse(typeof (T), s);
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
            Raw = raw;
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
            var other = (RaceGenderClass) obj;
            return (Race == other.Race) && (Gender == other.Gender) && (Class == other.Class);
        }

        public override int GetHashCode()
        {
            return (int) Race << 16 | (int) Gender << 8 | (int) Class;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Race, Gender, Class);
        }
    }
}