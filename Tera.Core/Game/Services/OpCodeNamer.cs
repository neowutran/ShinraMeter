// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tera.Game
{
    // Maps between numeric OpCodes and OpCode names
    // Since this mapping is version dependent, we can't use a sing global instance of this
    public class OpCodeNamer
    {
        private readonly Dictionary<ushort, string> _opCodeNames;
        private readonly Dictionary<string, ushort> _opCodeCodes;

        public string GetName(ushort opCode)
        {
            string name;
            if (_opCodeNames.TryGetValue(opCode, out name))
                return name;
            else
                return opCode.ToString("X4");
        }

        public OpCodeNamer(IEnumerable<KeyValuePair<ushort, string>> names)
        {
            var namesArray = names.ToArray();
            _opCodeNames = namesArray.ToDictionary(parts => parts.Key, parts => parts.Value);
            _opCodeCodes = namesArray.ToDictionary(parts => parts.Value, parts => parts.Key);
        }

        private static IEnumerable<KeyValuePair<ushort, string>> ReadOpCodeFile(string filename)
        {
            var names = File.ReadLines(filename)
                      .Select(s => s.Split('=').Select(part => part.Trim()).ToArray())
                      .Select(parts => new KeyValuePair<ushort, string>(ushort.Parse(parts[1]), parts[0]));
            return names;
        }

        public OpCodeNamer(string filename)
            : this(ReadOpCodeFile(filename))
        {
        }

        public ushort GetCode(string name)
        {
            ushort code;
            if (_opCodeCodes.TryGetValue(name, out code))
                return code;
            throw new ArgumentException(string.Format("Unknown name '{0}'", name));
        }
    }
}
