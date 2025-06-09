namespace Simulator.Instructions.ArithmeticLogic;

public abstract class ArithmeticShiftRight : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var arg0 = cpu.ReadRomAtPc(1);
        
        var dstIx = (byte)((arg0 >> 4) & 0x0f);
        var srcIx = (byte)(arg0 & 0x0f);
        var shiftAmount = (byte)(cpu.Registers[srcIx]);
        
        var acc = (byte) cpu.Registers[dstIx];
        
        var carry = false;
        var accSign = (acc & 0x80) != 0;

        for (var i = 0; i < shiftAmount; i++)
        {
            carry = (acc & 0x01) != 0;
            acc >>= 1;
            if (accSign) acc |= 0x80;
        }
        
        cpu.Registers.CarryFlag = carry;
        cpu.Registers.OverflowFlag = false;
        cpu.Registers.SignFlag = (acc & 0x80) != 0;
        cpu.Registers.ZeroFlag = (acc & 0xff) == 0;
        
        cpu.Registers[dstIx] = (byte)(acc & 0xff);
        cpu.Registers.ProgramCounter += 2;
        
        return new ExecutionResult(
            dstIx, true, false, false, 0, 0
        );
    }
}