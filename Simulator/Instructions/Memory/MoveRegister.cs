namespace Simulator.Instructions.Memory;

public class MoveRegister : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var arg = cpu.ReadRomAtPc(1);
        var dstIx = (byte)(arg >> 4);
        var srcIx = (byte)(arg & 0x0f);
        var val = cpu.Registers[srcIx];
        cpu.Registers[dstIx] = val;
        cpu.Registers.ZeroFlag = val == 0;
        cpu.Registers.SignFlag = (val & 0x80) != 0;
        return new ExecutionResult(
            dstIx, true, false, false, 0, 0
        );
    }
}