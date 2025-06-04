namespace Simulator.Instructions.FlowControl;

public class Call : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var callAddrL = cpu.ReadRomAtPc(1);
        var callAddrH = cpu.ReadRomAtPc(2);
        var callAddr = (ushort)((callAddrH << 8) | callAddrL);
        var retAddress = (ushort)(cpu.Registers.ProgramCounter + 3);
        var retAddressL = (byte)(retAddress & 0xff);
        var retAddressH = (byte)(retAddress >> 8);
        
        cpu.WriteRamLocation(--cpu.Registers.StackPointer, retAddressL);
        cpu.WriteRamLocation(--cpu.Registers.StackPointer, retAddressH);
        cpu.Registers.ProgramCounter = callAddr;

        var spa = (byte)(cpu.Registers.StackPointer + 1);
        
        return new ExecutionResult(
            0, false, true, true, (byte)(spa >> 8), (byte)(spa & 0xff));
    }
}