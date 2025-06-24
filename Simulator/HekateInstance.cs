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

    public Span<byte> GetRamPage(byte page) => new(_ram, page * 256, 256);
    public Span<byte> GetRomPage(byte page) => new(_rom, page * 256, 256);
    public byte[] DumpRam() => _ram;

    public ExecutionResult Step()
    {
        var pc = Registers.ProgramCounter;
        var ir = _rom[pc];
        Registers.InstructionRegister = ir;
        Console.WriteLine(((Opcode)ir).ToString());
        return (Opcode)ir switch
        {
            Opcode.Noop => NoOperation.Execute(this),
            Opcode.Halt => Halt.Execute(this),
            Opcode.ClearZero => ClearFlagZero.Execute(this),
            Opcode.SetZero => SetFlagZero.Execute(this),
            Opcode.ClearCarry => ClearFlagCarry.Execute(this),
            Opcode.SetCarry => SetFlagCarry.Execute(this),
            Opcode.ClearSign => ClearFlagSign.Execute(this),
            Opcode.SetSign => SetFlagSign.Execute(this),
            Opcode.ClearOverflow => ClearFlagOverflow.Execute(this),
            Opcode.SetOverflow => SetFlagOverflow.Execute(this),
            Opcode.Call => Call.Execute(this),
            Opcode.Ret => Ret.Execute(this),
            
            // Arithmetic-Logic
            Opcode.Add => Add.Execute(this),
            Opcode.AddC => AddCarry.Execute(this),
            Opcode.Increment => Increment.Execute(this),
            Opcode.Decrement => Decrement.Execute(this),
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
            Opcode.Sub => Sub.Execute(this),
            Opcode.SubB => SubBorrow.Execute(this),
            Opcode.Compare => Compare.Execute(this),
            Opcode.AddF => AddFloat.Execute(this),
            Opcode.SubF => SubFloat.Execute(this),
            
            // Memory
            Opcode.LoadImm => LoadImmediate.Execute(this),
            Opcode.LoadRom => LoadRom.Execute(this),
            Opcode.LoadRam => LoadRam.Execute(this),
            Opcode.StoreRam => StoreRam.Execute(this),
            Opcode.MoveReg => MoveRegister.Execute(this),
            Opcode.Push => Push.Execute(this),
            Opcode.Pop => Pop.Execute(this),
            
            // Control Flow
            Opcode.Jump => Jump.Execute(this),
            Opcode.JumpZero => JumpZero.Execute(this),
            Opcode.JumpNotZero => JumpNotZero.Execute(this),
            Opcode.JumpCarry => JumpCarry.Execute(this),
            Opcode.JumpNotCarry => JumpNotCarry.Execute(this),
            Opcode.JumpSign => JumpSign.Execute(this),
            Opcode.JumpNotSign => JumpNotSign.Execute(this),
            Opcode.JumpOverflow => JumpOverflow.Execute(this),
            Opcode.JumpNotOverflow => JumpNotOverflow.Execute(this),
            Opcode.JumpGreater => JumpGreater.Execute(this),
            Opcode.JumpLessEqual => JumpLessEqual.Execute(this),
            Opcode.JumpLess => JumpLess.Execute(this),
            Opcode.JumpGreaterEqual => JumpGreaterEqual.Execute(this),
            _ => throw new InvalidOperationException($"Invalid opcode: {ir:X2}")
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

    public bool WriteRamLocation(ushort address, byte value)
    {
        var mapping = Mapper.GetAddressMapping(address);
        var wasMapped = mapping != null;
        if (mapping is null)
        {
            _ram[address] = value;
            return wasMapped;
        }
        var offset = (ushort)(address - mapping.StartAddress);
        if (mapping.Device.Write(value, offset)) return wasMapped;
        Console.WriteLine("Could not write do device, disconnecting.");
        Mapper.RemoveDevice(mapping.Device);
        return false;
    }

    public byte ReadRomLocation(int address)
    {
        return _rom[address & 0xffff];
    }

    public byte ReadRomAtPc(ushort offset = 0)
    {
        return _rom[(Registers.ProgramCounter + offset) & 0xffff];
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

    public void LoadRamImage(byte[] image)
    {
        Array.Copy(image, _ram, _ram.Length);
    }
}