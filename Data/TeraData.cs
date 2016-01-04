using System.IO;
using Tera.Game;

namespace Data
{
    public class TeraData
    {
        internal TeraData(BasicTeraData basicData, string region)
        {
            OpCodeNamer =
                new OpCodeNamer(Path.Combine(basicData.ResourceDirectory,
                    $"data/opcodes/opcodes-{region}.txt"));
        }

        public OpCodeNamer OpCodeNamer { get; private set; }
    }
}