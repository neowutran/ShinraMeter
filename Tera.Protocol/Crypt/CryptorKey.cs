namespace Crypt
{
    internal class CryptorKey
    {
        public int Size;
        public int Pos1;
        public int Pos2;
        public int MaxPos;
        public int Key;
        public uint[] Buffer;
        public uint Sum;

        public CryptorKey(int size, int maxPos)
        {
            Size = size;
            Pos2 = MaxPos = maxPos;

            Buffer = new uint[Size*4];
        }
    }
}