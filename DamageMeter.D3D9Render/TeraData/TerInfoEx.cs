using System;
using System.Collections.Generic;
using System.Linq;

namespace DamageMeter.D3D9Render.TeraData
{
    public struct ClassInfo
    {
        public string PName;
        public string PDsp;
        public string PDmg;
        public string PCrit;
        public uint PId;
    }

    public static class Ex
    {

        private static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            return string.IsNullOrEmpty(value) ?
                defaultValue : Enum.TryParse(value, true, out T result) ? result : defaultValue;
        }
    }
}