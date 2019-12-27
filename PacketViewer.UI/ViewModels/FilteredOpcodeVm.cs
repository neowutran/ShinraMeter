namespace PacketViewer.UI
{
    public class FilteredOpcodeVm 
    {
        public ushort Opcode { get; }
        public FilterMode Mode { get; }

        public FilteredOpcodeVm(ushort opc, FilterMode f)
        {
            Opcode = opc;
            Mode = f;
        }
    }
}