namespace Simulator.Instructions.FlowControl;

public abstract class ClearFlagCarry : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        cpu.Registers.CarryFlag = false;
        cpu.Registers.ProgramCounter++;
        return new ExecutionResult(
            0, true, false, false, 0, 0);
    }
}