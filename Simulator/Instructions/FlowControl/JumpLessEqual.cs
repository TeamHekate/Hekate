namespace Simulator.Instructions.FlowControl;

public abstract class JumpLessEqual : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        
        var arg0 = cpu.ReadRomAtPc(1);
        var arg1 = cpu.ReadRomAtPc(2);
        
        var jmp = (ushort)(arg0 | (arg1 << 8));
        var npc = (ushort)(cpu.Registers.ProgramCounter + 3);
        
        var lessEq = !cpu.Registers.ZeroFlag && (cpu.Registers.OverflowFlag != cpu.Registers.SignFlag);
        
        cpu.Registers.ProgramCounter = lessEq ? jmp : npc;
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