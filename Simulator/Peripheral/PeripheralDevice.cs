using System.IO.Ports;
using System.Text;

namespace Simulator.Peripheral;

public class PeripheralDevice
{
    private readonly SerialPort? _port;
    public string DeviceName { get; set; }

    private bool SendBytes(byte[] data)
    {
        try
        {
            if (_port is not { IsOpen: true }) return false;
            _port.Write(data, 0, data.Length);
            return true;
        }
        catch (TimeoutException e)
        {
            Console.Error.WriteLine(e.ToString());
            return false;
        }
    }

    public bool Write(byte data, ushort offset)
    {
        byte[] bytes = [(byte)'W', data, (byte)(offset & 0xff), (byte)(offset >> 8)];
        return SendBytes(bytes);
    }

    public byte? Read(ushort offset)
    {
        if (_port is not { IsOpen: true }) return null;
        try
        {
            SendBytes([(byte)'R', (byte)(offset & 0xff), (byte)(offset >> 8)]);
            var rx = (byte)_port.ReadByte();
            return rx;
        }
        catch (TimeoutException e)
        {
            Console.Error.WriteLine(e.ToString());
            return null;
        }
    }

    private string Identify()
    {
        if (_port is not { IsOpen: true }) return "-";
        SendBytes("HK25\0"u8.ToArray());
        var buffer = new byte[32];
        for (var i = 0; i < 32; i++)
        {
            var rx = _port.ReadByte();
            if (rx == -1) Console.WriteLine("EOL????");
            buffer[i] = (byte)rx;
        }

        var res = Encoding.UTF8.GetString(buffer);
        return res;
    }

    public PeripheralDevice(string portName)
    {
        DeviceName = "-";
        _port = null;
        _port = new SerialPort(portName, 19200, Parity.None, 8, StopBits.One);
        _port.ReadTimeout = 500;
        _port.WriteTimeout = 500;
        _port.Open();
        if (!_port.IsOpen) _port.Open();
        DeviceName = Identify();
        _port.ReadTimeout = _port.WriteTimeout = 200;
    }

    public void SetTimeout(ushort timeout)
    {
        if (_port == null) return;
        _port.ReadTimeout = _port.WriteTimeout = timeout;
    }

    public void Close()
    {
        if (_port is not { IsOpen: true }) return;
        _port.Close();
        _port.Dispose();
    }
}