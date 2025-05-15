namespace Simulator.Instructions.Memory;

public abstract class LoadImmediate : IInstruction
{
    // LDI RX, YZ => 20 X0 YZ 
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        
        var arg0 = cpu.ReadRomAtPc(1);
        var arg1 = cpu.ReadRomAtPc(2);
        
        var dstIx = (byte)((arg0 >> 4) & 0x0f);
        cpu.Registers.ZeroFlag = arg1 == 0;
        cpu.Registers.SignFlag = (arg1 & 0x80) != 0;

        cpu.Registers[dstIx] = arg1;
        cpu.Registers.ProgramCounter += 3;

        return new ExecutionResult(dstIx, true, false, false, 0, 0);
    }
}