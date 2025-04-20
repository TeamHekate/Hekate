using System.Text.RegularExpressions;

namespace Assembler;

public static class Tokenizer
{
    public static readonly Dictionary<TokenType, string> TokenPatterns = new()
    {
        { TokenType.OPCODE, @"^(NOP|HALT|ADD|SUB|ADDC|SUBB|AND|IOR|XOR|NOT|MOV|JMP|JZ|JNZ|JC|JNC|JS|JNS|JV|JNV)\b" },
        { TokenType.REGISTER, @"^(R[\dA-F])" },
        { TokenType.IMMEDIATE, @"^((0B[01]+)|(0X[\dA-F]+)|(-?(\d*.)?\d+))" },
        { TokenType.LBRACKET, @"^\[" },
        { TokenType.RBRACKET, @"^\]" },
        { TokenType.PLUS, @"^\+" },
        { TokenType.COMMENT, @"^;.*" },
        { TokenType.LABEL, @"^[_A-Z][A-Z\d_]*\s*:"},
        { TokenType.NEWLINE, @"^\n"},
        { TokenType.COMMA, @"^,"}
    };

    private static Token TokenizeOnce(string code)
    {
        foreach(var t in TokenPatterns.Keys)
        {
            var pattern = TokenPatterns[t];
            var match = Regex.Match(code, pattern, RegexOptions.IgnoreCase);
            if (match.Success) return new Token(t, match.Value);
        }
        throw new Exception("Could not tokenize on: " + code.Split()[0]);
    }

    public static List<Token> Tokenize(string code)
    {
        List<Token> tokens = [];
        while ((code = code.Trim(['\t', '\r', ' '])).Length > 0)
        {
            var tok = TokenizeOnce(code);
            code = code[tok.Value.Length..];
            tokens.Add(tok);
        }

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
    COMMA,
    OPCODE,
    REGISTER,
    IMMEDIATE,
    LBRACKET,
    RBRACKET,
    PLUS,
    COMMENT,
    NEWLINE
}