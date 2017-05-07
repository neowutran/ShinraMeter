using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Randomizer
{
    internal class Program
    {
        private static readonly byte[] RandomizeString =
        {
            0x53, 0x00, 0x75, 0x00, 0x70, 0x00, 0x65, 0x00, 0x72, 0x00, 0x55, 0x00, 0x6E, 0x00, 0x69, 0x00, 0x71, 0x00,
            0x75, 0x00, 0x65, 0x00,
            0x53, 0x00, 0x74, 0x00, 0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x45, 0x00, 0x61, 0x00, 0x73, 0x00,
            0x69, 0x00, 0x6C, 0x00,
            0x79, 0x00, 0x44, 0x00, 0x65, 0x00, 0x74, 0x00, 0x65, 0x00, 0x63, 0x00, 0x74, 0x00, 0x61, 0x00, 0x62, 0x00,
            0x6C, 0x00, 0x65, 0x00,
            0x54, 0x00, 0x6F, 0x00, 0x42, 0x00, 0x65, 0x00, 0x41, 0x00, 0x62, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x54, 0x00,
            0x6F, 0x00, 0x52, 0x00,
            0x61, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x7A, 0x00, 0x65, 0x00, 0x54, 0x00,
            0x68, 0x00, 0x65, 0x00,
            0x50, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x67, 0x00, 0x72, 0x00, 0x61, 0x00, 0x6D, 0x00, 0x41, 0x00, 0x6E, 0x00,
            0x64, 0x00, 0x42, 0x00,
            0x79, 0x00, 0x70, 0x00, 0x61, 0x00, 0x73, 0x00, 0x73, 0x00, 0x53, 0x00, 0x69, 0x00, 0x67, 0x00, 0x6E, 0x00,
            0x61, 0x00, 0x74, 0x00,
            0x75, 0x00, 0x72, 0x00, 0x65, 0x00, 0x42, 0x00, 0x61, 0x00, 0x73, 0x00, 0x65, 0x00, 0x64, 0x00, 0x42, 0x00,
            0x6C, 0x00, 0x6F, 0x00,
            0x63, 0x00, 0x6B, 0x00
        };

        private static void Main()
        {
            DetectReplace();
        }

        private static char RandomChar()
        {
            var chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&".ToCharArray();
            var r = new Random();
            return chars[r.Next(chars.Length)];
        }

        private static long RandomLong(long min, long max)
        {
            var rand = new Random();
            var buf = new byte[8];
            rand.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (max - min)) + min;
        }

        public static void DetectReplace()
        {
            using (
                var stream =
                    new FileStream(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ShinraMeter.exe",
                        FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var detected = Detect(stream);
                if (detected == null)
                {
                    Console.WriteLine("The file as already been randomized or randomization impossible. Exiting");
                    Console.ReadKey();
                    return;
                }
                Randomize(stream, detected.Value);
                Console.WriteLine("The file as been successfully randomized. Exiting");
                Console.ReadKey();
            }
        }

        public static void Randomize(FileStream stream, KeyValuePair<long, long> positions)
        {
            var beginRandomize = RandomLong(positions.Key, positions.Value - 2);
            var sizeRandomize = RandomLong(2, positions.Key - beginRandomize);

            stream.Position = beginRandomize;
            if (stream.ReadByte() != 0)
                stream.Position--;
            while (stream.Position < beginRandomize + sizeRandomize)
            {
                stream.WriteByte(Convert.ToByte(RandomChar()));
                stream.Position = stream.Position + 2;
            }
        }

        public static KeyValuePair<long, long>? Detect(FileStream stream)
        {
            var byteCheckPosition = 0;
            long beginPosition = 0;
            while (stream.Position < stream.Length)
            {
                if (byteCheckPosition == 0)
                    beginPosition = stream.Position;

                if (stream.ReadByte() == RandomizeString[byteCheckPosition])
                    byteCheckPosition++;
                else
                    byteCheckPosition = 0;
                if (byteCheckPosition == RandomizeString.Length)
                    return new KeyValuePair<long, long>(beginPosition, stream.Position - 1);
            }
            return null;
        }
    }
}