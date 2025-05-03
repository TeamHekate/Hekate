namespace Simulator.Instructions.FlowControl;

public abstract class Halt : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu, byte[] args)
    {
        cpu.Registers.HaltFlag = true;
        cpu.Registers.ProgramCounter++;
        return new ExecutionResult(
            0, true, false, false, 0, 0
        );
    }
}