namespace Simulator.Instructions.ArithmeticLogic;

public abstract class DecimalAdjust : IInstruction
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
        var acc = cpu.Registers[regIx];
        
        // To do: add AuxFlag and change the following line.
        // var adjustLow = (acc & 0x0F) > 9 || cpu.Registers.AuxFlag
        var adjustLow = (acc & 0x0F) > 9;
        var adjustHigh = (acc >> 4) > 9 || cpu.Registers.CarryFlag;

        if (adjustLow)
        {
            acc += 0x06;
        }

        if (adjustHigh)
        {
            acc += 0x60;
            cpu.Registers.CarryFlag = true;
        }
        
        cpu.Registers.OverflowFlag = false;
        cpu.Registers.SignFlag = (acc & 0x80) != 0;
        cpu.Registers.ZeroFlag = (acc & 0xff) == 0;
        
        cpu.Registers[regIx] = (byte)(acc & 0xff);
        cpu.Registers.ProgramCounter += 2;
        
        return new ExecutionResult(
            regIx, true, false, false, 0, 0
        );
    }
}