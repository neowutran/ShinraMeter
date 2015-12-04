using System;
using System.Collections.Generic;

namespace Tera.DamageMeter.Skills.Skill
{
    public class Skill : IEquatable<object>
    {
        public List<int> SkillId;

        public Skill(string skill, List<int> skillId)
        {
            SkillId = skillId;
            SkillName = skill;
        }

        public string SkillName { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Skill) obj);
        }

        public bool Equals(Skill other)
        {
            return SkillName.Equals(other.SkillName);
        }

        public static bool operator ==(Skill a, Skill b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Skill a, Skill b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return SkillName.GetHashCode();
        }
    }
}