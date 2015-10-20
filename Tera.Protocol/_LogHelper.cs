using System;

namespace Tera.Protocol
{
    class LogHelper
    {
        private static readonly DateTime TimeOrigin = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static byte[] DateTimeToBytes(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException();

            var value = (long)Math.Round((dateTime - TimeOrigin).TotalMilliseconds);

            int byteCount = 0;
            while ((value >> byteCount * 8) != 0)
            {
                byteCount++;
            }

            var result = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                result[i] = unchecked((byte)(value >> i * 8));
            }
            return result;
        }

        public static DateTime BytesToTimeSpan(byte[] data)
        {
            ulong value = 0;
            for (int i = 0; i < data.Length; i++)
            {
                value |= (ulong)data[i] << i * 8;
            }
            return TimeOrigin + TimeSpan.FromMilliseconds(value);
        }
    }
}
