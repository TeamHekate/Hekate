namespace Simulator.Instructions.Memory;

public abstract class StoreRam : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var reg = cpu.ReadRomAtPc(1);
        var offs = cpu.ReadRomAtPc(2);
        var page = cpu.ReadRomAtPc(3);

        var srcIx = (byte)(reg >> 4);
        var adrIx = (byte)(reg & 0xf);
        // Extend by sign so that the 16-bit addition works with an 8-bit offset
        var regOffset = Utilities.SignExtend(cpu.Registers[adrIx]);
        var address = (ushort)((offs | (page << 8)) + regOffset);

        var val = cpu.Registers[srcIx];
        cpu.WriteRamLocation(address, val);
        cpu.Registers.ProgramCounter += 4;
        
        return new ExecutionResult(
            0, false, false, true, (byte)(address >> 8), (byte)(address & 0xff));
    }
}