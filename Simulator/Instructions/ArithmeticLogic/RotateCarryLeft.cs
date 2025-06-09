namespace Simulator.Instructions.ArithmeticLogic;

public abstract class RotateCarryLeft : IInstruction
{
    public static ExecutionResult Execute(HekateInstance cpu)
    {
        var arg0 = cpu.ReadRomAtPc(1);
        
        var dstIx = (byte)((arg0 >> 4) & 0x0f);
        var srcIx = (byte)(arg0 & 0x0f);
        var rotateAmount = (byte)(cpu.Registers[srcIx] & 0x7);
        
        var acc = cpu.Registers[dstIx];
        var carry = cpu.Registers.CarryFlag;
        
        for (var i = 0; i < rotateAmount; i++)
        {
            var newCarry = (acc & 0x80) != 0;
            acc = (byte)((acc << 1) | (carry ? 0x01 : 0x00));
            carry = newCarry;
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