namespace Simulator.Instructions.Memory;

public abstract class LoadRom : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var reg = cpu.ReadRomAtPc(1);
        var offs = cpu.ReadRomAtPc(2);
        var page = cpu.ReadRomAtPc(3);

        var dstIx = (byte)(reg >> 4);
        var adrIx = (byte)(reg & 0xf);
        // Extend by sign so that the 16-bit addition works with an 8-bit offset
        var regOffset = Utilities.SignExtend(cpu.Registers[adrIx]);
        var address = (ushort)((offs | (page << 8)) + regOffset);
        var val = cpu.ReadRomLocation(address);
        
        cpu.Registers[dstIx] = val;
        cpu.Registers.ProgramCounter += 4;
        if (val == 0) cpu.Registers.ZeroFlag = true;
        if ((val & 0x80) != 0) cpu.Registers.SignFlag = true;
        
        return new ExecutionResult(dstIx, true, false, false, 0, 0);
    }
}