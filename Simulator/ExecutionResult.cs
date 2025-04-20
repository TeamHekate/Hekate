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
}