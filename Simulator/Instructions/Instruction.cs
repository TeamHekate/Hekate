namespace Simulator.Instructions;

public abstract class Instruction
{
    public abstract ExecutionResult Execute(HekateInstance cpu, byte[] args);
}

