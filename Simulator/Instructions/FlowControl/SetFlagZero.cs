namespace Simulator.Instructions.FlowControl;

public abstract class SetFlagZero : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        cpu.Registers.ZeroFlag = true;
        cpu.Registers.ProgramCounter++;
        return new ExecutionResult(
            0, true, false, false, 0, 0);
    }
}