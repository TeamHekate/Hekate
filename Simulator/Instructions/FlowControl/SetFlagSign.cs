namespace Simulator.Instructions.FlowControl;

public abstract class SetFlagSign : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        cpu.Registers.SignFlag = true;
        cpu.Registers.ProgramCounter++;
        return new ExecutionResult(
            0, true, false, false, 0, 0);
    }
}