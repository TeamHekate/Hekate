namespace Assembler;

public class Instruction
{
    public Dictionary<InstructionType, int> InstructionLengths = new()
    {
        { InstructionType.NOP , 1},
        { InstructionType.HALT , 1}
    };
}

public enum InstructionType
{
    NOP,
    HALT
}