using System.Collections.ObjectModel;

namespace Simulator.Peripheral;

public class MemoryMapper
{
    private ObservableCollection<DeviceMapping> Mappings { get; set; } = [];

    public DeviceMapping? GetAddressMapping(ushort address)
    {
        return (from mapping in Mappings where mapping.ContainsAddress(address) select mapping).FirstOrDefault();
    }

    private bool HasCollision(ushort address, ushort length)
    {
        return Mappings.Any(mapping =>
            (address >= mapping.StartAddress && address < mapping.StartAddress + mapping.Length) ||
            (address + length >= mapping.StartAddress && address + length < mapping.StartAddress + mapping.Length));
    }

    public bool MapDevice(ushort address, ushort length, PeripheralDevice device)
    {
        if (HasCollision(address, length)) return false;
        Mappings.Add(new DeviceMapping(address, length, device));
        return true;
    }

    public bool RemoveDevice(string deviceName)
    {
        Console.WriteLine("Removing device " + deviceName);
        var target = Mappings.FirstOrDefault(map => map.Device.DeviceName.Equals(deviceName));
        if (target == null) return false;
        Mappings.Remove(target);
        return true;
    }

    public bool RemoveDevice(PeripheralDevice device)
    {
        Console.WriteLine("Removing device " + device.DeviceName);
        var mapping = Mappings.FirstOrDefault(map => map.Device == device);
        return mapping != null && Mappings.Remove(mapping);
    }

    public void ClearDevices()
    {
        foreach (var mapping in Mappings)
            mapping.Device.Close();
        Mappings.Clear();
    }
}