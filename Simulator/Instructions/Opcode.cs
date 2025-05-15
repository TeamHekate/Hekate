namespace Simulator.Instructions;

public enum Opcode
{
    Noop = 0x00,
    
    Add = 0x10,
    AddC = 0x11,
    
    LoadImm = 0x20,
    LoadRom = 0x21,
    StoreRam = 0x22,
    LoadRam = 0x23,

    Jump = 0x40,
    JumpZero = 0x42,
    JumpNotZero = 0x43,

    Halt = 0xFF
}