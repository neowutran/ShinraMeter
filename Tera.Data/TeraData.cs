// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Tera.Game;

namespace Tera.Data
{
    public class TeraData
    {
        public Region Region { get; private set; }
        public OpCodeNamer OpCodeNamer { get; private set; }
        public SkillDatabase SkillDatabase { get; private set; }

        internal TeraData(BasicTeraData basicData, string region)
        {
            Region = basicData.Regions.Single(x => x.Key == region);
            SkillDatabase = new SkillDatabase(Path.Combine(basicData.ResourceDirectory, "user_skills.txt"));
            OpCodeNamer = new OpCodeNamer(Path.Combine(basicData.ResourceDirectory, string.Format("opcodes-{0}.txt", Region.Version)));
        }
    }
}
