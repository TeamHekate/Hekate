namespace Simulator.Instructions.ArithmeticLogic;

public abstract class LoadImmediate : IInstruction
{
    // LDI RX, YZ => 20 X0 YZ 
    public static ExecutionResult Execute(HekateInstance cpu, byte[] args)
    {
        var dstIx = (byte)((args[0] >> 4) & 0x0f);
        var imm = args[1];
        cpu.Registers.ZeroFlag = imm == 0;
        cpu.Registers.SignFlag = (imm & 0x80) != 0;

        cpu.Registers[dstIx] = imm;
        cpu.Registers.ProgramCounter += 3;

        return new ExecutionResult(dstIx, true, false, false, 0, 0);
    }
}