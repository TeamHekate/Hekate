namespace Simulator.Instructions.ArithmeticLogic;

public abstract class ShiftLeft : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var arg0 = cpu.ReadRomAtPc(1);
        
        var dstIx = (byte)((arg0 >> 4) & 0x0f);
        var shiftAmount = (byte)(arg0 & 0x0f);
        
        // Guard against 0 shifts, (shiftAmount - 1) would make an error.
        if (shiftAmount is 0)
        {
            cpu.Registers.ProgramCounter += 2;
            return new ExecutionResult(0, false, false, false, 0, 0);
        }
        
        var dstVal = cpu.Registers[dstIx];

        var acc = (byte) dstVal << shiftAmount;
        
        cpu.Registers.CarryFlag = ((dstVal << (shiftAmount - 1)) & 0x80) != 0;
        cpu.Registers.OverflowFlag = false;
        cpu.Registers.SignFlag = (acc & 0x80) != 0;
        cpu.Registers.ZeroFlag = (acc & 0xff) == 0;
        
        cpu.Registers[dstIx] = (byte)(acc & 0xff);
        cpu.Registers.ProgramCounter += 2;
        
        return new ExecutionResult(
            dstIx, true, false, false, 0, 0
        );
    }
}