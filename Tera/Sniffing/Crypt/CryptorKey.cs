// Unknown Author and License

namespace Tera.Sniffing.Crypt
{
    internal class CryptorKey
    {
        public uint[] Buffer;
        public int Key;
        public int MaxPos;
        public int Pos1;
        public int Pos2;
        public int Size;
        public uint Sum;

        public CryptorKey(int size, int maxPos)
        {
            Size = size;
            Pos2 = MaxPos = maxPos;

            Buffer = new uint[Size*4];
        }
    }
}