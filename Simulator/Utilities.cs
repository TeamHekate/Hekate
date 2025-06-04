namespace Simulator;

public abstract class Utilities
{
    public static ushort SignExtend(byte value)
    {
        if ((value & 0x80) != 0) return (ushort)(0xff00 | value);
        return (ushort)(0x0000 | value);
    }

    // 1.3.4 Representation. Bias is 3 
    public static float ByteToFloat(byte value)
    {
        var sign = (uint)(value >> 7);
        var exponent = (uint)((value >> 4) & 0x7);
        var mantissa = (uint)(value & 0xf);

        if (exponent == 0 && mantissa == 0) return 0;
        if (exponent == 0x7 && mantissa == 0) return (sign == 1) ? float.NegativeInfinity : float.PositiveInfinity;
        if (exponent == 0x7 && mantissa != 0) return (sign == 1) ? -float.NaN : float.NaN;

        var v = mantissa / 16.0f;
        if (exponent != 0) v += 1;
        var E = exponent != 0 ? ((int)exponent - 3) : -2;
        v *= float.Exp2(E);

        return (sign == 1) ? -v : v;
    }

    // 1.3.4 Representation. Bias is 3 
    public static byte FloatToByte(float value)
    {
        if (value == 0) return 0x00;
        if (float.IsPositiveInfinity(value)) return 0x70;
        if (float.IsNegativeInfinity(value)) return 0xf0;

        var sign = (byte)(value > 0 ? 0 : 1);
        var abs = Math.Abs(value);
        var exponent = float.Floor(float.Log2(abs));

        var subNormal = false;
        if (exponent <= -3)
        {
            subNormal = true;
            exponent = -2;
        }

        var norm = abs * float.Pow(2, -exponent);
        var frac = norm - float.Truncate(norm);
        byte mantissa = 0;
        for (var i = 0; i < 4; i++)
        {
            mantissa <<= 1;
            frac *= 2;
            if (!(frac >= 1.0f)) continue;
            mantissa |= 1;
            frac -= 1.0f;
        }

        if (subNormal) exponent = -3;
        return (byte)((sign << 7) | ((byte)(exponent + 3) & 0x7) << 4 | (mantissa & 0xf));
    }
}