namespace Simulator.Instructions.Memory;

public class LoadRom : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var reg = cpu.ReadRomAtPc(1);
        var offs = cpu.ReadRomAtPc(2);
        var page = cpu.ReadRomAtPc(3);

        var dstIx = (byte)(reg >> 4);
        var adrIx = (byte)(reg & 0xf);
        var address = (ushort)((offs | (page << 8)) + cpu.Registers[adrIx]);
        var val = cpu.ReadRomLocation(address);
        
        cpu.Registers[dstIx] = val;
        cpu.Registers.ProgramCounter += 3;
        if (val == 0) cpu.Registers.ZeroFlag = true;
        if ((val & 0x80) != 0) cpu.Registers.SignFlag = true;
        
        return new ExecutionResult(dstIx, true, false, false, 0, 0);
    }
}