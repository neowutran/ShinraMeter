using System.Collections.Generic;

namespace Data
{
    public class World
    {
        public Dictionary<uint, Guard> Guards { get; set; }
        public uint Id { get; }
        public uint NameId { get; }

        public World(uint wId, uint name)
        {
            Guards = new Dictionary<uint, Guard>();
            Id = wId;
            NameId = name;
        }

    }
}
