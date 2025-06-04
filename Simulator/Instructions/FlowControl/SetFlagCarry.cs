namespace Simulator.Instructions.FlowControl;

public abstract class SetFlagCarry : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        cpu.Registers.CarryFlag = true;
        cpu.Registers.ProgramCounter++;
        return new ExecutionResult(
            0, true, false, false, 0, 0);
    }
}