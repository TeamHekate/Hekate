using System.Text.RegularExpressions;

namespace Assembler;

public static class Tokenizer
{
    private static readonly Dictionary<TokenType, string> TokenPatterns = new()
    {
        {
            TokenType.OPCODE,
            @"^(NOP|HLT|CFZ|SFZ|CFC|SFC|CFS|SFS|CFV|SFV|CALL|RET|ADD|ADC|SUB|SBB|SHL|SHR|ROL|ROR|RCL|RCR|ASR|AND|IOR|XOR|CMP|ADDF|SUBF|INC|DEC|INV|NEG|MIR|LDI|LDR|MOV|POP|PUSH|JMP|JZ|JNZ|JC|JNC|JS|JNS|JV|JNV|JG|JLE|JL|JGE)\b"
        },
        { TokenType.REGISTER, @"^(R[\dA-F])" },
        { TokenType.FLOAT_IMM, @"^(-?(\d*.)?\d+F)\b" },
        { TokenType.BIN_IMM, @"^(-?0B[01]+)" },
        { TokenType.HEX_IMM, @"^(-?0X[\dA-F]+)" },
        { TokenType.DEC_IMM, @"^(-?(\d+))" },
        { TokenType.DOT, @"^\." },
        { TokenType.DB, @"^DB\b"},
        { TokenType.ORG, @"^ORG\b"},
        { TokenType.COLON, @"^:" },
        { TokenType.LBRACKET, @"^\[" },
        { TokenType.RBRACKET, @"^\]" },
        { TokenType.PLUS, @"^\+" },
        { TokenType.COMMENT, @"^;.*" },
        { TokenType.LABEL, @"^[_A-Z][_A-Z\d]*\b" },
        { TokenType.NEWLINE, @"^\n" },
        { TokenType.COMMA, @"^," }
    };

    private static Token TokenizeOnce(string code)
    {
        foreach (var t in TokenPatterns.Keys)
        {
            var pattern = TokenPatterns[t];
            var match = Regex.Match(code, pattern, RegexOptions.IgnoreCase);
            if (match.Success) return new Token(t, match.Value);
        }

        throw new Exception("Could not tokenize on: " + code.Split()[0]);
    }

    public static Queue<Token> Tokenize(string code)
    {
        Queue<Token> tokens = [];
        while ((code = code.Trim(['\t', '\r', ' '])).Length > 0)
        {
            var tok = TokenizeOnce(code);
            code = code[tok.Value.Length..];
            tokens.Enqueue(tok);
        }

        tokens.Enqueue(new Token(TokenType.NEWLINE, "\n"));
        return tokens;
    }
}

public record Token(TokenType Type, string Value)
{
    public override string ToString() => $"< {Type} : {Value} >";
}

public enum TokenType
{
    LABEL,
    DOT,
    DB,
    ORG,
    COLON,
    COMMA,
    OPCODE,
    REGISTER,
    BIN_IMM,
    HEX_IMM,
    DEC_IMM,
    FLOAT_IMM,
    LBRACKET,
    RBRACKET,
    PLUS,
    COMMENT,
    NEWLINE
}