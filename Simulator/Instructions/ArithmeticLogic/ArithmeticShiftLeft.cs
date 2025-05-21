namespace Simulator.Instructions.ArithmeticLogic;

public abstract class ArithmeticShiftLeft : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        return ShiftLeft.Execute(cpu);
    }
}