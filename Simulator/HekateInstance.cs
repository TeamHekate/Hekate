using Simulator.Instructions;
using Simulator.Instructions.ArithmeticLogic;
using Simulator.Instructions.FlowControl;
using Simulator.Instructions.Memory;
using Simulator.Peripheral;

namespace Simulator;

public class HekateInstance
{
    private const uint MemorySize = 0x10000;

    public RegisterFile Registers { get; private set; } = new();

    private byte[] _ram = new byte[MemorySize];
    private byte[] _rom = new byte[MemorySize];
    private ushort _startAddress;

    public MemoryMapper Mapper { get; } = new();

    public Span<byte> GetRamPage(byte page) => new Span<byte>(_ram, page * 256, 256);
    public Span<byte> GetRomPage(byte page) => new Span<byte>(_rom, page * 256, 256);

    public ExecutionResult Step()
    {
        var pc = Registers.ProgramCounter;
        var ir = _rom[pc];
        Console.WriteLine(((Opcode)ir).ToString());
        return (Opcode)ir switch
        {
            Opcode.Noop => NoOperation.Execute(this),
            Opcode.ClearZero => ClearFlagZero.Execute(this),
            Opcode.Halt => Halt.Execute(this),
            
            // Arithmetic-Logic
            Opcode.Add => Add.Execute(this),
            Opcode.AddC => AddCarry.Execute(this),
            Opcode.Increment => Increment.Execute(this),
            Opcode.Decrement => Decrement.Execute(this),
            Opcode.DecimalAdjust => DecimalAdjust.Execute(this),
            Opcode.ArithmeticShiftLeft => ArithmeticShiftLeft.Execute(this),
            Opcode.ArithmeticShiftRight => ArithmeticShiftRight.Execute(this),
            Opcode.And => And.Execute(this),
            Opcode.Ior => Ior.Execute(this),
            Opcode.Xor => Xor.Execute(this),
            Opcode.Invert => Invert.Execute(this),
            Opcode.Negate => Negate.Execute(this),
            Opcode.Mirror => Mirror.Execute(this),
            Opcode.ShiftLeft => ShiftLeft.Execute(this),
            Opcode.ShiftRight => ShiftRight.Execute(this),
            Opcode.RotateLeft => RotateLeft.Execute(this),
            Opcode.RotateRight => RotateRight.Execute(this),
            Opcode.RotateCarryLeft => RotateCarryLeft.Execute(this),
            Opcode.RotateCarryRight => RotateCarryRight.Execute(this),
            
            // Memory
            Opcode.LoadImm => LoadImmediate.Execute(this),
            Opcode.LoadRom => LoadRom.Execute(this),
            Opcode.LoadRam => LoadRam.Execute(this),
            Opcode.StoreRam => StoreRam.Execute(this),
            Opcode.MoveReg => MoveRegister.Execute(this),
            
            // Control Flow
            Opcode.Jump => Jump.Execute(this),
            Opcode.JumpZero => JumpZero.Execute(this),
            Opcode.JumpNotZero => JumpNotZero.Execute(this),
            
            

            _ => throw new NotImplementedException("[SIMULATOR] Unknown Opcode: " + ir.ToString("X2"))
        };
    }

    public byte ReadRamLocation(ushort address, out bool wasMapped)
    {
        var mapping = Mapper.GetAddressMapping(address);
        wasMapped = mapping != null;
        if (mapping is null) return _ram[address];
        var offset = (ushort)(address - mapping.StartAddress);
        var rx = mapping.Device.Read(offset);
        if (rx == null) Mapper.RemoveDevice(mapping.Device);
        _ram[address] = rx ?? 0xff;
        return _ram[address];
    }

    public void WriteRamLocation(ushort address, byte value, ref bool wasMapped)
    {
        var mapping = Mapper.GetAddressMapping(address);
        wasMapped = mapping != null;
        if (mapping is null)
        {
            _ram[address] = value;
            return;
        }
        var offset = (ushort)(address - mapping.StartAddress);
        if (!mapping.Device.Write(value, offset)) Mapper.RemoveDevice(mapping.Device);
    }
    
    public void WriteRamLocation(ushort address, byte value)
    {
        var m = false;
        WriteRamLocation(address, value, ref m);
    }

    public byte ReadRomLocation(int address)
    {
        return _rom[address & 0xffff];
    }

    public byte ReadRomAtPc(ushort offset = 0)
    {
        return _rom[(Registers.ProgramCounter + offset) & 0xffff];
    }

    public void SaveRomImage(string path) //Save
    {
        File.WriteAllBytes(path, _rom);
        Console.WriteLine($"ROM saved to: {path}");
    }

    public void LoadRomImage(string path) //Open
    {
        var bytes = File.ReadAllBytes(path);

        if (bytes.Length != _rom.Length)
            throw new InvalidOperationException($"ROM size mismatch. Expected {_rom.Length}, got {bytes.Length}");

        Array.Copy(bytes, _rom, bytes.Length);
        Registers.ProgramCounter = _startAddress;
        Console.WriteLine($"ROM loaded from: {path}");
    }

    public void ClearRom()
    {
        _rom = new byte[MemorySize];
    }

    public void ClearRam()
    {
        _ram = new byte[MemorySize];
    }

    public void ClearRegisters()
    {
        Registers = new RegisterFile
        {
            ProgramCounter = _startAddress
        };
    }

    public void ClearRegisters(ushort startAddress)
    {
        _startAddress = startAddress;
        ClearRegisters();
    }

    public void LoadProgramAt(byte[] program, byte offset)
    {
        program.CopyTo(_rom, offset);
    }
}