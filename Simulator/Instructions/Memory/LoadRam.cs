namespace Simulator.Instructions.Memory;

public abstract class LoadRam : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var reg = cpu.ReadRomAtPc(1);
        var offs = cpu.ReadRomAtPc(2);
        var page = cpu.ReadRomAtPc(3);

        var dstIx = (byte)(reg >> 4);
        var adrIx = (byte)(reg & 0xf);
        var regOffset = Utilities.SignExtend(cpu.Registers[adrIx]);
        var address = (ushort)((offs | (page << 8)) + regOffset);

        var val = cpu.ReadRamLocation(address, out var wasMapped);
        cpu.Registers[dstIx] = val;
        if (val == 0) cpu.Registers.ZeroFlag = true;
        if ((val & 0x80) != 0) cpu.Registers.SignFlag = true;

        cpu.Registers.ProgramCounter += 4;
        return new ExecutionResult(
            dstIx, true, false, wasMapped, (byte)(address >> 8), (byte)(address & 0xff));
    }
}