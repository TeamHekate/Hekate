namespace Simulator.Instructions.FlowControl;

public class Jump
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        
        var arg0 =  cpu.ReadRomAtPc(1);
        var arg1 = cpu.ReadRomAtPc(2);
        var jmp = (ushort)(arg0 | (arg1 << 8));
        cpu.Registers.ProgramCounter = jmp;
        return new ExecutionResult(
            0,
            false,
            false,
            false,
            0,
            0
        );
    }
}