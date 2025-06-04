namespace Simulator.Instructions.FlowControl;

public abstract class ClearFlagZero : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        cpu.Registers.ZeroFlag = false;
        cpu.Registers.ProgramCounter++;
        return new ExecutionResult(
            0, true, false, false, 0, 0);
    }
}