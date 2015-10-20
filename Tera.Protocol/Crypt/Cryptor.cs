using System;

namespace Crypt
{
    internal class Cryptor
    {
        private int ChangeData;
        private int ChangeLen;

        private CryptorKey[] Key = {
                                         new CryptorKey(55, 31),
                                         new CryptorKey(57, 50),
                                         new CryptorKey(58, 39)
                                     };

        public Cryptor(byte[] key)
        {
            GenerateKey(key);
        }

        private byte[] FillKey(byte[] src)
        {
            byte[] result = new byte[680];

            for (int i = 0; i < 680; i++)
                result[i] = src[i % 128];

            result[0] = 128;

            return result;
        }

        private void GenerateKey(byte[] src)
        {
            byte[] buf = FillKey(src);
            for (int i = 0; i < 680; i += 20)
            {
                uint[] sha = Sha.Digest(buf);
                for (int j = 0; j < 5; j++)
                    Buffer.BlockCopy(BitConverter.GetBytes(sha[j]), 0, buf, i + j * 4, 4);
            }

            for (int i = 0; i < 220; i += 4)
                Key[0].Buffer[i / 4] = BitConverter.ToUInt32(buf, i);

            for (int i = 0; i < 228; i += 4)
                Key[1].Buffer[i / 4] = BitConverter.ToUInt32(buf, 220 + i);

            for (int i = 0; i < 232; i += 4)
                Key[2].Buffer[i / 4] = BitConverter.ToUInt32(buf, 448 + i);
        }

        public void ApplyCryptor(byte[] buf, int size)
        {
            int pre = (size < ChangeLen) ? size : ChangeLen;
            if (pre != 0)
            {
                for (int j = 0; j < pre; j++)
                    buf[j] ^= (byte)(ChangeData >> (8 * (4 - ChangeLen + j)));

                ChangeLen -= pre;
                size -= pre;
            }

            for (int i = pre; i < buf.Length - 3; i += 4)
            {
                int result = Key[0].Key & Key[1].Key | Key[2].Key & (Key[0].Key | Key[1].Key);

                for (int j = 0; j < 3; j++)
                {
                    CryptorKey k = Key[j];
                    if (result == k.Key)
                    {
                        uint t1 = k.Buffer[k.Pos1];
                        uint t2 = k.Buffer[k.Pos2];
                        uint t3 = (t1 <= t2) ? t1 : t2;
                        k.Sum = t1 + t2;
                        k.Key = (t3 > k.Sum) ? 1 : 0;
                        k.Pos1 = (k.Pos1 + 1) % k.Size;
                        k.Pos2 = (k.Pos2 + 1) % k.Size;
                    }
                    buf[i] ^= (byte)k.Sum;
                    buf[i + 1] ^= (byte)(k.Sum >> 8);
                    buf[i + 2] ^= (byte)(k.Sum >> 16);
                    buf[i + 3] ^= (byte)(k.Sum >> 24);
                }
            }

            int remain = size & 3;
            if (remain != 0)
            {
                int result = Key[0].Key & Key[1].Key | Key[2].Key & (Key[0].Key | Key[1].Key);
                ChangeData = 0;
                for (int j = 0; j < 3; j++)
                {
                    CryptorKey k = Key[j];
                    if (result == k.Key)
                    {
                        uint t1 = k.Buffer[k.Pos1];
                        uint t2 = k.Buffer[k.Pos2];
                        uint t3 = (t1 <= t2) ? t1 : t2;
                        k.Sum = t1 + t2;
                        k.Key = (t3 > k.Sum) ? 1 : 0;
                        k.Pos1 = (k.Pos1 + 1) % k.Size;
                        k.Pos2 = (k.Pos2 + 1) % k.Size;
                    }
                    ChangeData ^= (int)k.Sum;
                }

                for (int j = 0; j < remain; j++)
                    buf[size + pre - remain + j] ^= (byte)(ChangeData >> (j * 8));

                ChangeLen = 4 - remain;
            }
        }
    }
}