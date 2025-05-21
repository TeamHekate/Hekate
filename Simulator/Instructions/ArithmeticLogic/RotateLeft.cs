namespace Simulator.Instructions.ArithmeticLogic;

public abstract class RotateLeft : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var arg0 = cpu.ReadRomAtPc(1);
        
        var dstIx = (byte)((arg0 >> 4) & 0x0f);
        var rotateAmount = (byte)(arg0 & 0x0f);
        
        // Guard against 0 and >7 rotates
        if (rotateAmount is 0 or >7)
        {
            // Or call nop twice
            cpu.Registers.ProgramCounter += 2;
            return new ExecutionResult(0, false, false, false, 0, 0);
        }
        
        var dstVal = cpu.Registers[dstIx];

        var acc = (byte)((dstVal << rotateAmount) | (dstVal >> (8 - rotateAmount)));
        
        cpu.Registers.CarryFlag = false;
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