namespace Frontend.Models;

public class PeripheralDeviceModel (string portName)
{

    public string PortName { get; set; } = portName;
    public string StartAddress { get; set; } = "2000";
    public string Length { get; set; } = "01";
    public bool IsActive { get; set; } = false;
    public string DeviceName { get; set; } = "";

}