namespace Simulator.Instructions.FlowControl;

public abstract class SetFlagOverflow : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        cpu.Registers.OverflowFlag = true;
        cpu.Registers.ProgramCounter++;
        return new ExecutionResult(
            0, true, false, false, 0, 0);
    }
}