using System.Globalization;

namespace Assembler;

public static class Generator
{
    private enum InstructionMode
    {
        None,
        RegSrc,
        RegDst,
        RegReg,
        RegImm,
        RegMem,
        MemReg,
        Jump
    }

    private static byte[] _image;
    private static ushort _index = 0;

    private static readonly Dictionary<byte, InstructionMode> Modes = new()
    {
        { 0x00, InstructionMode.None }, // nop
        { 0x01, InstructionMode.None }, // hlt
        { 0x02, InstructionMode.None }, // cfz
        { 0x03, InstructionMode.None }, // sfz
        { 0x04, InstructionMode.None }, // cfc
        { 0x05, InstructionMode.None }, // sfc
        { 0x06, InstructionMode.None }, // cfs
        { 0x07, InstructionMode.None }, // sfs
        { 0x08, InstructionMode.None }, // cfv
        { 0x09, InstructionMode.None }, // sfv
        { 0x0B, InstructionMode.None }, // ret

        { 0x10, InstructionMode.RegReg }, // add
        { 0x11, InstructionMode.RegReg }, // adc
        { 0x12, InstructionMode.RegReg }, // sub
        { 0x13, InstructionMode.RegReg }, // sbc
        { 0x14, InstructionMode.RegReg }, // shl
        { 0x15, InstructionMode.RegReg }, // shr
        { 0x16, InstructionMode.RegReg }, // rol
        { 0x17, InstructionMode.RegReg }, // ror
        { 0x18, InstructionMode.RegReg }, // rcl
        { 0x19, InstructionMode.RegReg }, // rcr
        { 0x1A, InstructionMode.RegReg }, // asr
        { 0x1B, InstructionMode.RegReg }, // and
        { 0x1C, InstructionMode.RegReg }, // ior
        { 0x1D, InstructionMode.RegReg }, // xor
        { 0x1E, InstructionMode.RegReg }, // cmp
        { 0x50, InstructionMode.RegReg }, // addf
        { 0x51, InstructionMode.RegReg }, // subf

        { 0x26, InstructionMode.RegSrc }, // push
        { 0x25, InstructionMode.RegDst }, // pop
        { 0x30, InstructionMode.RegDst }, // inc
        { 0x31, InstructionMode.RegDst }, // dec
        { 0x32, InstructionMode.RegDst }, // inv
        { 0x33, InstructionMode.RegDst }, // neg
        { 0x34, InstructionMode.RegDst }, // mir

        { 0x20, InstructionMode.RegImm }, // ldi
        { 0x21, InstructionMode.RegMem }, // ldr
        { 0x22, InstructionMode.MemReg }, // mov (store ram)
        { 0x23, InstructionMode.RegMem }, // mov (load ram)
        { 0x24, InstructionMode.RegReg }, // mov (reg-reg)

        { 0x0A, InstructionMode.Jump }, // call
        { 0x40, InstructionMode.Jump }, // jmp
        { 0x42, InstructionMode.Jump }, // jz
        { 0x43, InstructionMode.Jump }, // jnz
        { 0x44, InstructionMode.Jump }, // jc
        { 0x45, InstructionMode.Jump }, // jnc
        { 0x46, InstructionMode.Jump }, // js
        { 0x47, InstructionMode.Jump }, // jns
        { 0x48, InstructionMode.Jump }, // jv
        { 0x49, InstructionMode.Jump }, // jnv
        { 0x4A, InstructionMode.Jump }, // jg
        { 0x4B, InstructionMode.Jump }, // jle
        { 0x4C, InstructionMode.Jump }, // jl
        { 0x4D, InstructionMode.Jump }, // jge
    };

    private static void ApplyOrigin(OriginNode node)
    {
        _index = node.Address;
    }

    private static byte MiniFloatToByte(string val)
    {
        var value = float.Parse(val);
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

    private static byte ConvertImmediateToByte(string value)
    {
        var neg = false;
        var val = value.ToUpperInvariant();
        if (val.StartsWith('-'))
        {
            val = val.TrimStart('-');
            neg = true;
        }

        byte ret;
        if (val.StartsWith("0B"))
            ret = byte.Parse(val.Replace("0B", ""), NumberStyles.BinaryNumber);
        else if (val.StartsWith("0X"))
            ret = byte.Parse(val.Replace("0X", ""), NumberStyles.HexNumber);
        else if (val.EndsWith('F'))
            return MiniFloatToByte((neg ? "-" : "") + val.Replace("F", ""));
        else
            ret = byte.Parse(val, NumberStyles.Integer);
        if (neg)
        {
            ret = (byte)((byte)~ret + 1);
        }

        return ret;
    }

    private static void GenerateInstruction(InstructionNode node)
    {
        var mode = Modes[node.Opcode];
        _image[_index++] = node.Opcode;
        switch (mode)
        {
            case InstructionMode.None:
                break;
            case InstructionMode.RegReg:
            {
                var dstIx = ((RegisterArg)node.Arguments[0]).RegIx;
                var srcIx = ((RegisterArg)node.Arguments[1]).RegIx;
                _image[_index++] = (byte)((dstIx << 4) | srcIx);
                break;
            }
            case InstructionMode.RegImm:
            {
                var dstIx = ((RegisterArg)node.Arguments[0]).RegIx;
                var imm = ConvertImmediateToByte(((ImmediateArg)node.Arguments[1]).Value);
                _image[_index++] = (byte)((dstIx << 4));
                _image[_index++] = imm;
                break;
            }
            case InstructionMode.RegMem:
            {
                var dstIx = ((RegisterArg)node.Arguments[0]).RegIx;
                var addrArg = (AddressArg)node.Arguments[1];
                var addr = addrArg.Address;
                var addrH = (byte)(addr >> 8);
                var addrL = (byte)(addr & 0xff);
                var ofsIx = addrArg.RegIx;
                _image[_index++] = (byte)(dstIx << 4 | ofsIx);
                _image[_index++] = addrL;
                _image[_index++] = addrH;
                break;
            }
            case InstructionMode.MemReg:
            {
                var addrArg = (AddressArg)node.Arguments[0];
                var addr = addrArg.Address;
                var addrH = (byte)(addr >> 8);
                var addrL = (byte)(addr & 0xff);
                var ofsIx = addrArg.RegIx;
                var srcIx = ((RegisterArg)node.Arguments[1]).RegIx;
                _image[_index++] = (byte)(srcIx << 4 | ofsIx);
                _image[_index++] = addrL;
                _image[_index++] = addrH;
                break;
            }
            case InstructionMode.Jump:
            {
                var jumpTarget = (JumpTargetArg)node.Arguments[0];
                var addrH = (byte)(jumpTarget.Address >> 8);
                var addrL = (byte)(jumpTarget.Address & 0xff);
                _image[_index++] = addrL;
                _image[_index++] = addrH;
                break;
            }
            case InstructionMode.RegDst:
            {
                var target = ((RegisterArg)node.Arguments[0]).RegIx;
                _image[_index++] = (byte)((target & 0xf) << 4);
                break;
            }
            case InstructionMode.RegSrc:
            {
                var target = ((RegisterArg)node.Arguments[0]).RegIx;
                _image[_index++] = (byte)((target & 0xf));
                break;
            }
            default:
                throw new ArgumentException($"Addressing mode for opcode ${node.Opcode} was not defined. ");
        }
    }

    public static byte[] Generate(Queue<ParseNode> parseNodes)
    {
        _image = new byte[0x10000];
        for (var i = 0; i < _image.Length; i++) _image[i] = 0x00;
        _index = 0;

        while (parseNodes.Count > 0)
        {
            var node = parseNodes.Dequeue();
            switch (node)
            {
                case InstructionNode instructionNode:
                    GenerateInstruction(instructionNode);
                    break;
                case OriginNode originNode:
                    ApplyOrigin(originNode);
                    break;
            }
        }

        return _image;
    }
}