using System;

namespace ShinraMeter.D3D9Render.TeraData
{
    public struct ClassInfo
    {
        /// <summary>
        /// Player Nickname
        /// </summary>
        public string PName;
        /// <summary>
        /// Player DPS
        /// </summary>
        public string PDsp;
        /// <summary>
        /// Player Summary dps for percent
        /// </summary>
        public string PDmg;
        /// <summary>
        /// Player Crit Rate
        /// </summary>
        public string PCrit;
        /// <summary>
        /// Player ID
        /// Needed for caching
        /// </summary>
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