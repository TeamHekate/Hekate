namespace Simulator.Instructions;

public enum Opcode
{
    Noop = 0x00,        // NOP
    
    // Arithmetic
    Add = 0x10,         // ADD r, r
    AddC = 0x11,        // ADC r, r
    Increment = 0xA6,      // INC r
    Decrement = 0xA7,      // DEC r
    DecimalAdjust = 0xA8,  // DAA
    ArithmeticShiftLeft = 0xA9, // ASL r, n
    ArithmeticShiftRight = 0xAA, // ASR r, n
    
    // Memory
    LoadImm = 0x20,     // LDI r, i
    LoadRom = 0x21,     // LDR r, [[ah:al]+r]
    StoreRam = 0x22,    // MOV [[ah:al]+r], r
    LoadRam = 0x23,     // MOV r, [[ah:al]+r]
    MoveReg = 0x24,     // MOV r, r
    
    // Control Flow
    Jump = 0x40,        // JMP [ah:al]
    JumpZero = 0x42,    // JZ [ah:al]
    JumpNotZero = 0x43, // JNZ [ah:al]
    
    // Logical
    And = 0xC0,         // AND r, r
    Ior = 0xC1,         // IOR r, r
    Xor = 0xC2,         // XOR r, r
    Invert = 0xC3,      // INV r
    Negate = 0xC4,       // NEG r
    Mirror = 0xC5,       // MIR r
    ShiftLeft = 0xC9,    // SHL r, n
    ShiftRight = 0xCA,    // SHR r, n
    RotateLeft = 0xCB,    // ROL r, n
    RotateRight = 0xCC,    // ROR r, n
    RotateCarryLeft = 0xCD, // RCL r, n
    RotateCarryRight = 0xCE, // RCR r, n
    

    Halt = 0xFF         // HLT
}
