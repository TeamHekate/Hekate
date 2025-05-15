namespace Simulator.Peripheral;

public class DeviceMapping(ushort startAddress, ushort length, PeripheralDevice device)
{

    public ushort StartAddress { get; set; } = startAddress;
    public ushort Length { get; set; } = length;
    public PeripheralDevice Device { get; private set; } = device;
    
    public bool ContainsAddress(ushort address) => (address >= StartAddress && address < StartAddress + Length);
    
}