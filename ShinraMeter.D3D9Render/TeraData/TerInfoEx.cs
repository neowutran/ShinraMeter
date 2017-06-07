using System;
using System.Collections.Generic;
using System.Linq;

namespace ShinraMeter.D3D9Render.TeraData
{
    public struct ClassInfo
    {
        public string PName;
        public string PDsp;
        public string PDmg;
        public string PCrit;
    }

    public static class Ex
    {

        private static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return Enum.TryParse(value, true, out T result) ? result : defaultValue;
        }
    }
}