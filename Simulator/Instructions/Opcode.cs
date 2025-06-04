namespace Simulator.Instructions;

public enum Opcode
{
    Noop = 0x00,        // NOP
    ClearZero = 0x01,   // CFZ
    
    Add = 0x10,         // ADD r, r
    AddC = 0x11,        // ADC r, r
    
    LoadImm = 0x20,     // LDI r, i
    LoadRom = 0x21,     // LDR r, [[ah:al]+r]
    StoreRam = 0x22,    // MOV [[ah:al]+r], r
    LoadRam = 0x23,     // MOV r, [[ah:al]+r]
    MoveReg = 0x24,     // MOV r, r
    Pop = 0x25,
    Push = 0x26,

    Jump = 0x40,        // JMP [ah:al]
    JumpZero = 0x42,    // JZ [ah:al]
    JumpNotZero = 0x43, // JNZ [ah:al]

    Halt = 0xFF         // HLT
}