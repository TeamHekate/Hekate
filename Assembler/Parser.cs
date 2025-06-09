using System.Globalization;

namespace Assembler;

public record ParseNode();

public record OriginNode(ushort Address) : ParseNode;

public record InstructionNode(byte Opcode, params Argument[] Arguments) : ParseNode
{
    public override string ToString()
    {
        return $"{Opcode:X2}\t {string.Join<Argument>(", ", Arguments)}";
    }
}

public record Argument;

public record ImmediateArg(string Value) : Argument
{
    public override string ToString()
    {
        return Value;
    }
}

public record LabeledArg(ushort Address, string? Label) : Argument
{
    public ushort Address { get; set; } = Address;

    public override string ToString()
    {
        return Address.ToString("X4");
    }
}

public record JumpTargetArg(ushort Address, string? Label) : LabeledArg(Address, Label)
{
    public override string ToString()
    {
        return Address.ToString("X4");
    }
}

public record AddressArg(ushort Address, string? Label, byte RegIx) : LabeledArg(Address, Label)
{
    public override string ToString()
    {
        return $"[{Address:X4}+R{RegIx:X2}]";
    }
}

public record RegisterArg(byte RegIx) : Argument
{
    public override string ToString()
    {
        return $"R{RegIx:X1}";
    }
}

public static class Parser
{
    private static Queue<Token> _tokens = [];
    private static Queue<LabeledArg> _unresolvedLabeledArgs = [];
    private static Dictionary<string, ushort> _labels = [];
    private static ushort _currentAddress = 0;
    private static int _line = 0;

    private static Token Consume(params TokenType[] accepts)
    {
        var tok = _tokens.Dequeue();
        if (accepts.Length == 0 || accepts.Contains(tok.Type)) return tok;
        throw new Exception($"[Line {_line}] Unexpected token of type " + tok.Type);
    }

    private static Token Peek()
    {
        return _tokens.Peek();
    }

    private static AddressArg ParseAddressArg()
    {
        Consume(TokenType.LBRACKET); // Consume [

        var aTok = Consume(TokenType.HEX_IMM, TokenType.LABEL); // Consume hex number or label
        byte regIx = 0; // Offset register
        if (Peek().Type == TokenType.PLUS) // If there is a plus
        {
            Consume(TokenType.PLUS); // Consume the plus and
            var regTok = Consume(TokenType.REGISTER); // Consume the new offset register
            regIx = byte.Parse(regTok.Value.Replace("R", ""), NumberStyles.HexNumber); // Extract index
        }

        Consume(TokenType.RBRACKET); // Consume ]

        if (aTok.Type == TokenType.LABEL)
        {
            var res = new AddressArg(0, aTok.Value, regIx);
            _unresolvedLabeledArgs.Enqueue(res);
            return res;
        } // If using label, add in unresolved queue.

        // Extract address as ushort, return value
        var adr = ushort.Parse(aTok.Value.ToUpperInvariant().Replace("0X", ""), NumberStyles.HexNumber);
        return new AddressArg(adr, null, regIx);
    }

    private static ImmediateArg ParseImmediateArg()
    {
        var iTok = Consume(
            TokenType.BIN_IMM,
            TokenType.DEC_IMM,
            TokenType.HEX_IMM,
            TokenType.FLOAT_IMM);
        return new ImmediateArg(iTok.Value);
    }

    private static JumpTargetArg ParseJumpTargetArg()
    {
        var tTok = Consume(TokenType.HEX_IMM, TokenType.LABEL);
        var res = (tTok.Type == TokenType.HEX_IMM)
            ? new JumpTargetArg(
                ushort.Parse(tTok.Value.ToUpperInvariant().Replace("0X", ""), NumberStyles.HexNumber), null)
            : new JumpTargetArg(0, tTok.Value);
        if (res.Label != null) _unresolvedLabeledArgs.Enqueue(res);
        _currentAddress += 3;
        return res;
    }

    private static RegisterArg ParseRegisterArg()
    {
        var rTok = Consume(TokenType.REGISTER);
        return new RegisterArg(byte.Parse(rTok.Value.Replace("R", ""), NumberStyles.HexNumber));
    }

    private static void ParseLabel()
    {
        var lTok = Consume(TokenType.LABEL);
        Consume(TokenType.COLON);
        _labels.Add(lTok.Value, _currentAddress);
    }

    private static InstructionNode ParseInstruction()
    {
        var oTok = Consume(TokenType.OPCODE);
        switch (oTok.Value)
        {
            case "NOP":
            case "HLT":
            case "CFZ":
            case "SFZ":
            case "CFC":
            case "SFC":
            case "CFS":
            case "SFS":
            case "CFV":
            case "SFV":
            case "RET":
            {
                _currentAddress += 1;
                return new InstructionNode(
                    oTok.Value switch
                    {
                        "NOP" => 0x00,
                        "HLT" => 0x01,
                        "CFZ" => 0x02,
                        "SFZ" => 0x03,
                        "CFC" => 0x04,
                        "SFC" => 0x05,
                        "CFS" => 0x06,
                        "SFS" => 0x07,
                        "CFV" => 0x08,
                        "SFV" => 0x09,
                        "RET" => 0x0B,
                        _ => 0x00
                    }
                );
            }
            case "ADD":
            case "ADC":
            case "SUB":
            case "SBB":
            case "SHL":
            case "SHR":
            case "ROL":
            case "ROR":
            case "RCL":
            case "RCR":
            case "ASR":
            case "AND":
            case "IOR":
            case "XOR":
            case "CMP":
            case "ADDF":
            case "SUBF":
            {
                var dstReg = ParseRegisterArg();
                Consume(TokenType.COMMA);
                var srcReg = ParseRegisterArg();
                _currentAddress += 2;
                return new InstructionNode(oTok.Value switch
                {
                    "ADD" => 0x10,
                    "ADC" => 0x11,
                    "SUB" => 0x12,
                    "SBB" => 0x13,
                    "SHL" => 0x14,
                    "SHR" => 0x15,
                    "ROL" => 0x16,
                    "ROR" => 0x17,
                    "RCL" => 0x18,
                    "RCR" => 0x19,
                    "ASR" => 0x1A,
                    "AND" => 0x1B,
                    "IOR" => 0x1C,
                    "XOR" => 0x1D,
                    "CMP" => 0x1E,
                    "ADDF" => 0x50,
                    "SUBF" => 0x51,
                    _ => 0x1E
                }, dstReg, srcReg);
            }

            case "PUSH":
            case "POP":
            case "INC":
            case "DEC":
            case "INV":
            case "NEG":
            case "MIR":
            {
                var target = ParseRegisterArg();
                _currentAddress += 2;
                return new InstructionNode(
                    oTok.Value switch
                    {
                        "POP" => 0x25,
                        "PUSH" => 0x26,
                        "INC" => 0x30,
                        "DEC" => 0x31,
                        "INV" => 0x32,
                        "NEG" => 0x33,
                        "MIR" => 0x34,
                        _ => 0x30
                    }, target
                );
            }

            case "LDI":
            {
                var dstReg = ParseRegisterArg();
                Consume(TokenType.COMMA);
                var imm = ParseImmediateArg();
                _currentAddress += 3;
                return new InstructionNode(0x20, dstReg, imm);
            }
            case "LDR":
            {
                var dstReg = ParseRegisterArg();
                Consume(TokenType.COMMA);
                var adr = ParseAddressArg();
                _currentAddress += 4;
                return new InstructionNode(0x21, dstReg, adr);
            }
            case "MOV":
            {
                if (Peek().Type == TokenType.LBRACKET)
                {
                    var dstAdr = ParseAddressArg();
                    Consume(TokenType.COMMA);
                    var srcIx = ParseRegisterArg();
                    _currentAddress += 4;
                    return new InstructionNode(0x22, dstAdr, srcIx);
                }

                if (Peek().Type != TokenType.REGISTER)
                    throw new Exception($"[Line {_line}] Was expecting a register or address after MOV, got "
                                        + Peek().Type);

                var dstIx = ParseRegisterArg();
                Consume(TokenType.COMMA);
                if (Peek().Type == TokenType.LBRACKET)
                {
                    var srcAdr = ParseAddressArg();
                    _currentAddress += 4;
                    return new InstructionNode(0x23, dstIx, srcAdr);
                }
                else if (Peek().Type == TokenType.REGISTER)
                {
                    var srcIx = ParseRegisterArg();
                    _currentAddress += 2;
                    return new InstructionNode(0x24, dstIx, srcIx);
                }

                throw new Exception(
                    $"[Line {_line}] Was expecting a register or address after MOV, got "
                    + Peek().Type);
            }
            case "CALL":
            case "JMP":
            case "JZ":
            case "JNZ":
            case "JC":
            case "JNC":
            case "JS":
            case "JNS":
            case "JV":
            case "JNV":
            case "JG":
            case "JLE":
            case "JL":
            case "JGE":
            {
                var opc = (byte)(oTok.Value switch
                {
                    "CALL" => 0x0A,
                    "JMP" => 0x40,
                    "JZ" => 0x42,
                    "JNZ" => 0x43,
                    "JC" => 0x44,
                    "JNC" => 0x45,
                    "JS" => 0x46,
                    "JNS" => 0x47,
                    "JV" => 0x48,
                    "JNV" => 0x49,
                    "JG" => 0x4A,
                    "JLE" => 0x4B, 
                    "JL" => 0x4C,
                    "JGE" => 0x4D,
                    _ => 0x40
                });
                return new InstructionNode(opc, ParseJumpTargetArg());
            }
            default:
            {
                throw new Exception($"[Line {_line}] Unknown opcode: " + oTok.Value);
            }
        }
    }

    private static Queue<ParseNode> ParseLine()
    {
        Queue<ParseNode> q = [];
        if (Peek().Type == TokenType.ORIGIN)
        {
            var org = ushort.Parse(
                Consume().Value.Replace("@", ""),
                NumberStyles.HexNumber);
            q.Enqueue(new OriginNode(org));
            _currentAddress = org;
        }
        else
        {
            if (Peek().Type == TokenType.LABEL)
                ParseLabel();
            if (Peek().Type == TokenType.OPCODE)
                q.Enqueue(ParseInstruction());
        }

        if (Peek().Type == TokenType.COMMENT)
            Consume(TokenType.COMMENT);
        Consume(TokenType.NEWLINE);
        return q;
    }

    private static void ResolveLabels()
    {
        while (_unresolvedLabeledArgs.Count > 0)
        {
            var arg = _unresolvedLabeledArgs.Dequeue();
            if (arg.Label == null) continue;
            if (!_labels.TryGetValue(arg.Label, out var value)) throw new Exception("Unknown label: " + arg.Label);
            arg.Address = value;
        }
    }

    public static Queue<ParseNode> Parse(Queue<Token> tokens, ushort startingAddress)
    {
        _tokens = tokens;
        _unresolvedLabeledArgs = [];
        _labels = [];
        _currentAddress = startingAddress;
        _line = 1;

        Queue<ParseNode> q = [];

        while (_tokens.Count > 0)
        {
            var ln = ParseLine();
            while (ln.Count > 0) q.Enqueue(ln.Dequeue());
            _line++;
        }

        ResolveLabels();

        return q;
    }
}