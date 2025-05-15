namespace Simulator.Instructions;

public interface IInstruction
{
    public static abstract ExecutionResult Execute(HekateInstance cpu);
}

