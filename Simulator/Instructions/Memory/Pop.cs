namespace Simulator.Instructions.Memory;

public class Pop : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var dstIx = (byte)(cpu.ReadRomAtPc(1) >> 4);
        var adr = cpu.Registers.StackPointer;
        var val = cpu.ReadRamLocation(adr, out var wasMapped);
        cpu.Registers[dstIx] = val;
        cpu.Registers.ProgramCounter += 2;
        cpu.Registers.StackPointer++;

        cpu.Registers.SignFlag = (val & 0x80) != 0;
        cpu.Registers.CarryFlag = val == 0;

        return new ExecutionResult(
            dstIx, true, true,
            wasMapped, (byte)(adr >> 8), (byte)(adr & 0xff)
        );
    }
}