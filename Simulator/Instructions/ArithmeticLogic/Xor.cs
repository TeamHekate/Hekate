namespace Simulator.Instructions.ArithmeticLogic;

public abstract class Xor : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var arg0 = cpu.ReadRomAtPc(1);
        
        var dstIx = (byte)((arg0 >> 4) & 0x0f);
        var srcIx = (byte)(arg0 & 0x0f);
        var dstVal = cpu.Registers[dstIx];
        var srcVal = cpu.Registers[srcIx];

        var acc = (byte)(srcVal ^ dstVal);
        var accSign = (acc & 0x80) != 0;
        
        cpu.Registers.CarryFlag = false;
        cpu.Registers.OverflowFlag = false; 
        cpu.Registers.SignFlag = accSign;
        cpu.Registers.ZeroFlag = (acc & 0xff) == 0;
        
        cpu.Registers[dstIx] = (byte)(acc & 0xff);
        cpu.Registers.ProgramCounter += 2;
        
        return new ExecutionResult(
            dstIx, true, false, false, 0, 0
        );
    }
}