using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Simulator;

namespace Frontend.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<MemoryGridRowModel> RamPageZero { get; set; } = [];
        public ObservableCollection<MemoryGridRowModel> RomPageZero { get; set; } = [];

        private byte[] _ram = new byte[512];

        private HekateInstance _cpu = new HekateInstance();

        public ObservableCollection<string> Registers { get; private set; } = [];
        [ObservableProperty] private string _programCounter = "0000";

        private void UpdateRegisters()
        {
            Registers.Clear();
            for (var i = 0; i < 16; i++) Registers.Add(_cpu.Registers.Registers[i].ToString("X2"));
        }
        
        public MainWindowViewModel()
        {
            UpdateRegisters();

            _cpu.LoadProgramAt(
                [0x20, 0x20, 0x80, 0x10, 0x12, 0x41, 0x83, 0x00, 0xFF], 0x80);
            _cpu.Registers.ProgramCounter = 0x80;

            {
                RamPageZero.Clear();
                var page = _cpu.GetRamPage(0).ToArray();
                for (byte row = 0; row < 16; row++)
                    RamPageZero.Add(new MemoryGridRowModel(
                        (row << 4).ToString("X4"),
                        new Span<byte>(page, (row << 4), 16)
                            .ToArray().Select(e => e.ToString("X2")).ToArray()
                    ));
            }
            {
                RomPageZero.Clear();
                var page = _cpu.GetRomPage(0).ToArray();
                for (byte row = 0; row < 16; row++)
                    RomPageZero.Add(new MemoryGridRowModel(
                        (row << 4).ToString("X4"),
                        new Span<byte>(page, (row << 4), 16)
                            .ToArray().Select(e => e.ToString("X2")).ToArray()
                    ));
            }
        }

        [RelayCommand] private void ClickStep()
        {
            var result = _cpu.Step();
            UpdateRegisters();
            // Console.WriteLine(result);
            ProgramCounter = _cpu.Registers.ProgramCounter.ToString("X4");
        }
    }
}