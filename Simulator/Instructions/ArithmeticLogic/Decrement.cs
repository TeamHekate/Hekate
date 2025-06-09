namespace Simulator.Instructions.ArithmeticLogic;

public abstract class Decrement : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var arg0 = cpu.ReadRomAtPc(1);
        
        // If the low nibble is not zero, throw exception
        if ((arg0 & 0x0f) != 0)
        {
            throw new NotImplementedException("[SIMULATOR] Unknown opcode: " + cpu.ReadRomAtPc(0) + cpu.ReadRomAtPc(1));
        }
        
        var regIx = (byte)((arg0 >> 4) & 0x0f);
        var regVal = cpu.Registers[regIx];

        var acc = (ushort)(regVal-1);
        
        cpu.Registers.CarryFlag = (regVal & 0xff) == 0;
        cpu.Registers.OverflowFlag = (regVal == 0x80); 
        cpu.Registers.SignFlag = (acc & 0x80) != 0;
        cpu.Registers.ZeroFlag = (acc & 0xff) == 0;
        
        cpu.Registers[regIx] = (byte)(acc & 0xff);
        cpu.Registers.ProgramCounter += 2;
        
        return new ExecutionResult(
            regIx, true, false, false, 0, 0
        );
    }
}