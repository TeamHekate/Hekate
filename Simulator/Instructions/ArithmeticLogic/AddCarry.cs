namespace Simulator.Instructions.ArithmeticLogic;

public abstract class AddCarry : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        
        var arg0 = cpu.ReadRomAtPc(1);
        
        var dstIx = (byte)((arg0 >> 4) & 0x0f);
        var srcIx = (byte)(arg0 & 0x0f);
        var dstVal = cpu.Registers[dstIx];
        var srcVal = cpu.Registers[srcIx];

        var acc = (ushort)(srcVal + dstVal + (cpu.Registers.CarryFlag ? 1 : 0));

        var dstSign = (dstVal & 0x80) != 0;
        var srcSign = (srcVal & 0x80) != 0;
        var accSign = (acc & 0x80) != 0;

        cpu.Registers.OverflowFlag = (dstSign == srcSign && accSign != dstSign);
        cpu.Registers.SignFlag = accSign;
        cpu.Registers.ZeroFlag = (acc & 0xff) == 0;
        cpu.Registers.CarryFlag = (acc & 0x100) != 0;

        cpu.Registers[dstIx] = (byte)(acc & 0xff);
        cpu.Registers.ProgramCounter += 2;

        return new ExecutionResult(
            dstIx, true, false, false, 0, 0
        );
    }
}