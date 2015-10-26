// Unknown Author and License

using System;
using System.Security.Cryptography;

namespace Tera.Sniffing.Crypt
{
    internal class Cryptor
    {
        private int _changeData;
        private int _changeLen;

        //No fucking idea where that come from, will search later
        //TODO
        private readonly CryptorKey[] _key = {
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
            var shaAlgorithm = SHA1.Create();
            for (int i = 0; i < 680; i += 20)
            {
                byte[] sha = shaAlgorithm.ComputeHash(buf);
                for (int j = 0; j < 5; j++)
                    Buffer.BlockCopy(sha, 0, buf, i + j * 4, 4);
            }

            for (int i = 0; i < 220; i += 4)
                _key[0].Buffer[i / 4] = BitConverter.ToUInt32(buf, i);

            for (int i = 0; i < 228; i += 4)
                _key[1].Buffer[i / 4] = BitConverter.ToUInt32(buf, 220 + i);

            for (int i = 0; i < 232; i += 4)
                _key[2].Buffer[i / 4] = BitConverter.ToUInt32(buf, 448 + i);
        }

        public void ApplyCryptor(byte[] buf, int size)
        {
            int pre = (size < _changeLen) ? size : _changeLen;
            if (pre != 0)
            {
                for (int j = 0; j < pre; j++)
                    buf[j] ^= (byte)(_changeData >> (8 * (4 - _changeLen + j)));

                _changeLen -= pre;
                size -= pre;
            }

            for (int i = pre; i < buf.Length - 3; i += 4)
            {
                int result = _key[0].Key & _key[1].Key | _key[2].Key & (_key[0].Key | _key[1].Key);

                for (int j = 0; j < 3; j++)
                {
                    CryptorKey k = _key[j];
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
                int result = _key[0].Key & _key[1].Key | _key[2].Key & (_key[0].Key | _key[1].Key);
                _changeData = 0;
                for (int j = 0; j < 3; j++)
                {
                    CryptorKey k = _key[j];
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
                    _changeData ^= (int)k.Sum;
                }

                for (int j = 0; j < remain; j++)
                    buf[size + pre - remain + j] ^= (byte)(_changeData >> (j * 8));

                _changeLen = 4 - remain;
            }
        }
    }
}