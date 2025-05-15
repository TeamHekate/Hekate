using System.Globalization;

namespace Assembler;

public static class Generator
{
    private enum InstructionMode
    {
        None,
        RegReg,
        RegImm,
        RegMem,
        MemReg,
        Jump
    }

    private static byte[] _image;
    private static ushort index = 0;

    private static Dictionary<byte, InstructionMode> _modes = new()
    {
        { 0x00, InstructionMode.None },

        { 0x10, InstructionMode.RegReg },
        { 0x11, InstructionMode.RegReg },

        { 0x20, InstructionMode.RegImm },
        { 0x21, InstructionMode.RegMem },
        { 0x22, InstructionMode.MemReg },
        { 0x23, InstructionMode.RegMem },
        { 0x24, InstructionMode.RegReg },

        { 0x40, InstructionMode.Jump },
        { 0x42, InstructionMode.Jump },
        { 0x43, InstructionMode.Jump },

        { 0xFF, InstructionMode.None },
    };

    private static void ApplyOrigin(OriginNode node)
    {
        index = node.Address;
    }

    private static byte MiniFloatToByte(string value)
    {
        return 0xFF;
        // NOT SUPPORTED YET.
    }

    private static byte ConvertImmediateToByte(string value)
    {
        var val = value.ToUpperInvariant();
        if (val.StartsWith("0B")) return byte.Parse(val.Replace("0B", ""), NumberStyles.BinaryNumber);
        if (val.StartsWith("0X")) return byte.Parse(val.Replace("0X", ""), NumberStyles.HexNumber);
        if (val.EndsWith('F')) return MiniFloatToByte(val.Replace("F", ""));
        return byte.Parse(val, NumberStyles.Integer);
    }

    private static void GenerateInstruction(InstructionNode node)
    {
        var mode = _modes[node.Opcode];
        _image[index++] = node.Opcode;
        switch (mode)
        {
            case InstructionMode.None:
                break;
            case InstructionMode.RegReg:
            {
                var dstIx = ((RegisterArg)node.Arguments[0]).RegIx;
                var srcIx = ((RegisterArg)node.Arguments[1]).RegIx;
                _image[index++] = (byte)((dstIx << 4) | srcIx);
                break;
            }
            case InstructionMode.RegImm:
            {
                var dstIx = ((RegisterArg)node.Arguments[0]).RegIx;
                var imm = ConvertImmediateToByte(((ImmediateArg)node.Arguments[1]).Value);
                _image[index++] = (byte)((dstIx << 4) | imm);
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
                _image[index++] = (byte)(dstIx << 4 | ofsIx);
                _image[index++] = addrL;
                _image[index++] = addrH;
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
                _image[index++] = (byte)(srcIx << 4 | ofsIx);
                _image[index++] = addrL;
                _image[index++] = addrH;
                break;
            }
            case InstructionMode.Jump:
            {
                var jumpTarget = (JumpTargetArg) node.Arguments[0];
                var addrH = (byte)(jumpTarget.Address >> 8);
                var addrL = (byte)(jumpTarget.Address & 0xff);
                _image[index++] = addrL;
                _image[index++] = addrH;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static byte[] Generate(Queue<ParseNode> parseNodes)
    {
        _image = new byte[0x10000];
        for (var i = 0; i < _image.Length; i++) _image[i] = 0x00;
        index = 0;

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