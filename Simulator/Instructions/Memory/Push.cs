namespace Simulator.Instructions.Memory;

public class Push : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var srcIx = cpu.ReadRomAtPc(1) & 0x0f;
        var srcVal = cpu.Registers[srcIx];
        var adr = --cpu.Registers.StackPointer;
        cpu.WriteRamLocation(adr, srcVal);
        cpu.Registers.ProgramCounter += 2;
        return new ExecutionResult(
            0, false, true,
            true, (byte)(adr >> 8), (byte)(adr & 0xff)
        );
    }
}