using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;

namespace Tera.Data
{
    public class Monster
    {

        public string Id { get; private set; }
        public string Name { get; private set; }

        public bool Boss { get; private set; }
        public Monster(string id, string name, bool boss)
        {
            Id = id;
            Name = name;
            Boss = boss;

        }

    }
}
