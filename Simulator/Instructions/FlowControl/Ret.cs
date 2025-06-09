namespace Simulator.Instructions.FlowControl;

public abstract class Ret : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var addrH = cpu.ReadRamLocation(cpu.Registers.StackPointer++, out var hMap);
        var addrL = cpu.ReadRamLocation(cpu.Registers.StackPointer++, out var lMap);
        var addr = (ushort)((addrH << 8) | addrL);
        var spa = (ushort)(cpu.Registers.StackPointer - 1);
        cpu.Registers.ProgramCounter = addr;
        return new ExecutionResult(
            0, false, true,
            hMap | lMap, (byte)(spa >> 8), (byte)(spa & 0xff)
        );
    }
}