using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.DamageMeter
{
    public class Skill : IEquatable<Object>
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Skill) obj);
        }

        private readonly string _skill;
        public List<int> SkillId;

        public Skill(string skill, List<int> skillId)
        {
            SkillId = skillId;
            _skill = skill;
        }

        public string SkillName => _skill;

        public bool Equals(Skill other)
        {
            return _skill.Equals(other._skill);
        }

        public static bool operator ==(Skill a, Skill b)
        {
            if (System.Object.ReferenceEquals(a, b))
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
            return _skill.GetHashCode();
        }
    }
}