namespace Simulator.Instructions.Memory;

public class StoreRam : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var reg = cpu.ReadRomAtPc(1);
        var offs = cpu.ReadRomAtPc(2);
        var page = cpu.ReadRomAtPc(3);

        var srcIx = (byte)(reg >> 4);
        var adrIx = (byte)(reg & 0xf);
        var address = (ushort)((offs | (page << 8)) + cpu.Registers[adrIx]);

        var val = cpu.Registers[srcIx];
        var wasMapped = false;
        cpu.WriteRamLocation(address, val, ref wasMapped);
        cpu.Registers.ProgramCounter += 4;
        
        return new ExecutionResult(
            0, false, false, true, page, offs);
    }
}