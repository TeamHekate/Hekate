namespace Simulator.Instructions;

public enum Opcode
{
    Noop = 0x00,
    
    Add = 0x10,
    AddC = 0x11,
    
    LoadImm = 0x20,

    JumpZero = 0x40,
    JumpNotZero = 0x41,

    Halt = 0xFF
}