using System.Text;

namespace Assembler;

public static class HekateAssembler
{
    public static byte[] Assemble(string code)
    {
        var tokens = Tokenizer.Tokenize(code.ToUpperInvariant());
        var nodes = Parser.Parse(tokens, 0x0000);
        var image = Generator.Generate(nodes);
        return image;
    }
}