namespace Simulator;

public class RegisterFile
{
    public class GeneralPurposeRegisters
    {
        private ushort[] _registers = new ushort[15];
        public ushort this[int index] {
            get => index == 0 ? (ushort)0 : _registers[index - 1];
            set
            {
                if(index > 0) _registers[index - 1] = value;
            }
        }
    }

    public bool ZeroFlag;
    public bool CarryFlag;
    public bool SignFlag;
    public bool OverflowFlag;
    public bool HaltFlag;
    
    public GeneralPurposeRegisters Registers = new GeneralPurposeRegisters();
    public ushort ProgramCounter { get; set; }
    public ushort StackPointer { get; set; }
    public byte InstructionRegister { get; set; }
}