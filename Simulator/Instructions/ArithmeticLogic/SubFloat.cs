namespace Simulator.Instructions.ArithmeticLogic;

public abstract class SubFloat : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var dstIx = (byte)(cpu.ReadRomAtPc(1) >> 4);
        var srcIx = (byte)(cpu.ReadRomAtPc(1) & 0xf);
        var dstVal = cpu.Registers[dstIx];
        var srcVal = cpu.Registers[srcIx];
        var dstF = Utilities.ByteToFloat(dstVal);
        var srcF = Utilities.ByteToFloat(srcVal);
        var accF = dstF - srcF;
        var acc = Utilities.FloatToByte(accF);

        var dstInf = float.IsInfinity(dstF);
        var srcInf = float.IsInfinity(srcF);
        var accInf = float.IsInfinity(accF);

        cpu.Registers.SignFlag = (acc & 0x80) != 0;
        cpu.Registers.OverflowFlag = !dstInf && !srcInf && accInf;
        cpu.Registers.ZeroFlag = acc == 0;
        
        cpu.Registers[dstIx] = acc;
        cpu.Registers.ProgramCounter += 2;

        return new ExecutionResult(dstVal, true, false, false, 0, 0);
    }
}