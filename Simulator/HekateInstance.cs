using Simulator.Instructions;
using Simulator.Instructions.ArithmeticLogic;
using Simulator.Instructions.FlowControl;

namespace Simulator;

public class HekateInstance
{
    private const uint MemorySize = 0x10000;

    public RegisterFile Registers { get; private set; } = new();

    private byte[] _ram = new byte[MemorySize];
    private byte[] _rom = new byte[MemorySize];

    public Span<byte> GetRamPage(byte page) => new Span<byte>(_ram, page * 256, 256);
    public Span<byte> GetRomPage(byte page) => new Span<byte>(_rom, page * 256, 256);

    public ExecutionResult Step()
    {
        var pc = Registers.ProgramCounter;
        var ir = _rom[pc];
        Console.WriteLine(((Opcode)ir).ToString());
        return (Opcode)ir switch
        {
            Opcode.Noop => NoOperation.Execute(this, []),
            
            Opcode.Halt => Halt.Execute(this, []),
            Opcode.Add => Add.Execute(this, [_rom[pc + 1]]),
            Opcode.AddC => AddCarry.Execute(this, [_rom[pc + 1]]),
            
            Opcode.LoadImm => LoadImmediate.Execute(this, [_rom[pc + 1], _rom[pc + 2]]),
            
            Opcode.JumpZero => JumpZero.Execute(this, [_rom[pc + 1], _rom[pc + 2]]),
            Opcode.JumpNotZero => JumpNotZero.Execute(this, [_rom[pc + 1], _rom[pc + 2]]),
            
            _ => throw new NotImplementedException("Unknown Opcode: " + ir.ToString("X2"))
        };
    }

    public void Run() //Run method
    {
        while (true)
        {
            var pc = Registers.ProgramCounter;
            var opcode = (Opcode)_rom[pc];

      
            Console.WriteLine($"[PC={pc:X4}] Executing: {opcode}");

            var result = Step();

            Console.WriteLine(result.ToString()); //Burda konsola değilde ekrana basmalı

            if (opcode == Opcode.Halt)
            {
                Console.WriteLine("HALT encountered. Stopping execution.");
                break;
            }
        }

        Console.WriteLine("Program execution completed.");
    }

    public void SaveRom(string path)  //Save
    {
        File.WriteAllBytes(path, _rom);
        Console.WriteLine($"ROM saved to: {path}");
    }

    public void LoadRom(string path) //Open
    {
        var bytes = File.ReadAllBytes(path);

        if (bytes.Length != _rom.Length)
            throw new InvalidOperationException($"ROM size mismatch. Expected {_rom.Length}, got {bytes.Length}");

        Array.Copy(bytes, _rom, bytes.Length);
        Registers.ProgramCounter = 0; // Optional: reset PC
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
        Registers = new RegisterFile();
    }
    
    public void LoadProgramAt(byte[] program, byte offset)
    {
        program.CopyTo(_rom, offset);
    }
}