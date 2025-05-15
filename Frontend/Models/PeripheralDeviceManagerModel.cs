using System;
using System.Collections.ObjectModel;
using System.IO.Ports;

namespace Frontend.Models;

public class PeripheralDeviceManagerModel
{
    
    private static string[] GetDeviceNames => SerialPort.GetPortNames();

    public ObservableCollection<PeripheralDeviceModel> Devices { get; private set; } = [];

    public void RefreshDevices()
    {
        Console.WriteLine("Refreshing devices.");
        Devices.Clear();
        foreach (var portName in GetDeviceNames)
            Devices.Add(new PeripheralDeviceModel(portName));
        Console.WriteLine("Found {0} devices.", Devices.Count);

    }
}