namespace Simulator.Instructions.FlowControl;

public abstract class JumpNotZero : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu, byte[] args)
    {
        var jmp = (ushort)(args[0] | (args[1] << 8));
        var npc = (ushort)(cpu.Registers.ProgramCounter + 3);
        cpu.Registers.ProgramCounter = (cpu.Registers.ZeroFlag) ? npc : jmp;
        return new ExecutionResult(
            0,
            false,
            false,
            false,
            0,
            0
        );
    }
}