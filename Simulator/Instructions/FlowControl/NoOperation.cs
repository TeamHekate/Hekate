namespace Simulator.Instructions.FlowControl;

public class NoOperation : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu, byte[] args)
    {
        cpu.Registers.ProgramCounter++;
        return new ExecutionResult(0, false, false, false, 0, 0);
    }
}