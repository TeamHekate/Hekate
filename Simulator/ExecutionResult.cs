using System.Text;

namespace Simulator;

public class ExecutionResult(
    byte registerIndex,
    bool flags,
    bool sp,
    bool ram,
    byte ramPage,
    byte ramOffset
    )
{
    public readonly byte RegisterIndex = registerIndex;
    public readonly bool Flags = flags;
    public readonly bool Sp = sp;
    
    public readonly bool Ram = ram;
    public readonly byte RamPage = ramPage;
    public readonly byte RamOffset = ramOffset;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("-= Execution Result =-");
        if(RegisterIndex != 0) sb.Append("\n\r\tDestination Register: " + RegisterIndex);
        if (Flags) sb.Append("\n\r\tFlags Modified.");
        if (Sp) sb.Append("\n\r\\tStack Pointer Modified.");
        if (Ram) sb.Append("\n\r\tRAM Destination: [" + RamPage + ":" + RamOffset+ "].");
        return sb.ToString();
    }
}