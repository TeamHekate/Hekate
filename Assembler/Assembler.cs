using System.Text;

namespace Assembler;

public static class Assembler
{
    public const uint RomSize = 0x10000;

    private static string? _code;
    private static StringBuilder _sb = new StringBuilder();

    public static byte[] Assemble(string code)
    {
        var image = new byte[RomSize];
        _code = code.ToUpperInvariant();
        _sb.Clear();

        var tokens = Tokenizer.Tokenize(code);
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
        
        return image;
    }
    
}