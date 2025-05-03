namespace Simulator;

public class RegisterFile
{
    public class GeneralPurposeRegisters
    {
        private byte[] _registers = new byte[15];
        public byte this[int index] {
            get => index == 0 ? (byte)0 : _registers[index - 1];
            set
            {
                if(index > 0) _registers[index - 1] = value;
            }
        }
    }

    public bool ZeroFlag = false;
    public bool CarryFlag = false;
    public bool SignFlag = false;
    public bool OverflowFlag = false;
    public bool HaltFlag = false;
    
    public GeneralPurposeRegisters Registers = new GeneralPurposeRegisters();
    public ushort ProgramCounter { get; set; }
    public ushort StackPointer { get; set; }
    public byte InstructionRegister { get; set; }
}