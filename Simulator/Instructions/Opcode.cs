namespace Simulator.Instructions;

public enum Opcode
{
    // Implied Addressing / Special
    Noop = 0x00,        // NOP
    Halt = 0x01,        // HLT
    ClearZero = 0x02,   // CFZ
    SetZero = 0x03,     // SFZ
    ClearCarry = 0x04,  // CFC
    SetCarry = 0x05,    // SFC
    ClearSign = 0x06,   // CFS
    SetSign = 0x07,     // SFS
    ClearOverflow = 0x08,   // CFV
    SetOverflow = 0x09,     // SFV
    Call = 0x0A,            // CALL i16
    Ret = 0x0B,             // RET
    
    // Arithmetic-Logic
    Add = 0x10,                     // ADD r, r
    AddC = 0x11,                    // ADC r, r
    Sub = 0x12,                     // SUB r, r
    SubB = 0x13,                    // SBB r, r
    ShiftLeft = 0x14,               // SHL r, r
    ShiftRight = 0x15,              // SHR r, r
    RotateLeft = 0x16,              // ROL r, r
    RotateRight = 0x17,             // ROR r, r
    RotateCarryLeft = 0x18,         // RCL r, r
    RotateCarryRight = 0x19,        // RCR r, r
    ArithmeticShiftRight = 0x1A,    // ASR r, r
    And = 0x1B,                     // AND r, r
    Ior = 0x1C,                     // IOR r, r
    Xor = 0x1D,                     // XOR r, r
    Compare = 0x1E,                 // CMP r, r
    AddF = 0x50,                    // ADF r, r
    SubF = 0x51,                    // SBF r, r
    
    Increment = 0x30,               // INC r
    Decrement = 0x31,               // DEC r
    Invert = 0x32,                  // INV r
    Negate = 0x33,                  // NEG r
    Mirror = 0x34,                  // MIR r
    
    // Memory
    LoadImm = 0x20,     // LDI r, i
    LoadRom = 0x21,     // LDR r, [[ah:al]+r]
    StoreRam = 0x22,    // MOV [[ah:al]+r], r
    LoadRam = 0x23,     // MOV r, [[ah:al]+r]
    MoveReg = 0x24,     // MOV r, r
    Pop = 0x25,         // POP r
    Push = 0x26,        // PUSH r

    
    // Control Flow
    Jump = 0x40,        // JMP [ah:al]
    JumpZero = 0x42,    // JZ [ah:al]
    JumpNotZero = 0x43, // JNZ [ah:al]
    JumpCarry = 0x44,
    JumpNotCarry = 0x45,
    JumpSign = 0x46,
    JumpNotSign = 0x47,
    JumpOverflow = 0x48,
    JumpNotOverflow = 0x49,
}