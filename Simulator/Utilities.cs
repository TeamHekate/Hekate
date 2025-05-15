namespace Simulator;

public abstract class Utilities
{
    public static ushort SignExtend(byte value)
    {
        if ((value & 0x80) != 0) return (ushort)(0xff00 | value);
        return (ushort)(0x0000 | value);
    }
}