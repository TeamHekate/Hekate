namespace Simulator;

public class RegisterFile
{
    public bool ZeroFlag = false;
    public bool CarryFlag = false;
    public bool SignFlag = false;
    public bool OverflowFlag = false;
    public bool HaltFlag = false;

    private readonly byte[] _registers;

    public ushort ProgramCounter = 0;
    public ushort StackPointer = 0;
    public byte InstructionRegister = 0;
    
    
    
    public byte this[int index]
    {
        get => _registers[index];
        set
        {
            if (index != 0) _registers[index] = value;
            else Console.WriteLine("Cannot write to hardwired register 0.");
        }
    }

    public RegisterFile(ushort pc, ushort sp)
    {
        ZeroFlag = false;
        CarryFlag = false;
        SignFlag = false;
        OverflowFlag = false;
        HaltFlag = false;
        _registers = new byte[16];

        InstructionRegister = 0;
        ProgramCounter = pc;
        StackPointer = sp;
    }

    public RegisterFile() : this(0, 0) {}


}