using Simulator.Instructions;

namespace Simulator;

public class HekateInstance
{
    private const uint MemorySize = 0x10000;

    public RegisterFile Registers { get; } = new();
    
    private byte[] _ram;
    private byte[] _rom;

    public Span<byte> GetRamPage(byte page) => new Span<byte>(_ram, page * 256, 256);
    public Span<byte> GetRomPage(byte page) => new Span<byte>(_rom, page * 256, 256);

    public ExecutionResult Step()
    {
        return null;
    }
    
    public HekateInstance()
    {
        _ram = new byte[MemorySize];
        _rom = new byte[MemorySize];
    }
}