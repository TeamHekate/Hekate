using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Assembler;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Simulator;
using Simulator.Peripheral;

namespace Frontend.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<MemoryGridRowModel> RamPage { get; set; } = [];
        public ObservableCollection<MemoryGridRowModel> RomPage { get; set; } = [];

        public PeripheralDeviceManagerModel DeviceManager { get; private set; } = new();

        [ObservableProperty] private ushort _ramAddress;
        [ObservableProperty] private ushort _romAddress;

        public string ProgramCounter => _cpu.Registers.ProgramCounter.ToString("X4");

        public string[] Flags =>
        [
            _cpu.Registers.ZeroFlag ? "1" : "0",
            _cpu.Registers.CarryFlag ? "1" : "0",
            _cpu.Registers.SignFlag ? "1" : "0",
            _cpu.Registers.OverflowFlag ? "1" : "0",
            _cpu.Registers.HaltFlag ? "1" : "0"
        ];

        public string CurrentProgram { get; set; } = "";

        private ExecutionResult? LastExecutionResult { get; set; }
        private readonly HekateInstance _cpu = new HekateInstance();

        private bool _isRunning;
        private bool _isPaused;

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        public bool IsHalted => _cpu.Registers.HaltFlag;

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged();
            }
        }

        private BackgroundWorker? _worker;
        private readonly ManualResetEvent _busy = new ManualResetEvent(false);

        public ObservableCollection<string> Registers { get; private set; } = [];

        private void UpdateRegisters()
        {
            Registers.Clear();
            for (var i = 0; i < 16; i++) Registers.Add(_cpu.Registers[i].ToString("X2"));
        }

        private void UpdateRamPage()
        {
            RamPage.Clear();
            var page = _cpu.GetRamPage((byte)(RamAddress >> 8)).ToArray();
            for (byte row = 0; row < 16; row++)
                RamPage.Add(new MemoryGridRowModel(
                    ((RamAddress & 0xff00) | (row << 4)).ToString("X4"),
                    new Span<byte>(page, (row << 4), 16)
                        .ToArray().Select(e => e.ToString("X2")).ToArray()
                ));
            OnPropertyChanged(nameof(RamPage));
        }

        private void UpdateRomPage()
        {
            RomPage.Clear();
            var page = _cpu.GetRomPage((byte)(RomAddress >> 8)).ToArray();
            for (byte row = 0; row < 16; row++)
                RomPage.Add(new MemoryGridRowModel(
                    ((RomAddress & 0xff00) | (row << 4)).ToString("X4"),
                    new Span<byte>(page, (row << 4), 16)
                        .ToArray().Select(e => e.ToString("X2")).ToArray()
                ));
            OnPropertyChanged(nameof(RomPage));
        }

        public MainWindowViewModel()
        {

            
            
            UpdateRegisters();
            UpdateRamPage();
            UpdateRomPage();
            RefreshDevices();
            OnPropertyChanged(nameof(ProgramCounter));
        }

        private void StepSimulation()
        {
            if (_cpu.Registers.HaltFlag) return;
            LastExecutionResult = _cpu.Step();
            if (LastExecutionResult.Ram)
            {
                var addr = (ushort)((LastExecutionResult.RamPage << 8) | (LastExecutionResult.RamOffset));
                if ((RamAddress & 0xff00) != (addr & 0xff00))
                {
                    RamAddress = addr;
                    UpdateRamPage();
                }
                else
                {
                    RamAddress = addr;
                    var newData = _cpu.ReadRamLocation(addr, out var _).ToString("X2");
                    var oldLoc = new string[16];
                    RamPage[LastExecutionResult.RamOffset >> 4].Locations.CopyTo(oldLoc, 0);
                    oldLoc[LastExecutionResult.RamOffset & 0xf] = newData;
                    RamPage[LastExecutionResult.RamOffset >> 4] =
                        new MemoryGridRowModel((RamAddress & 0xfff0).ToString("X4"), oldLoc);
                    OnPropertyChanged(nameof(RamPage));
                }
            }
            if ((_cpu.Registers.ProgramCounter & 0xff00) != (RomAddress & 0xff00))
            {
                if ((RomAddress & 0xff00) != (_cpu.Registers.ProgramCounter & 0xff00))
                {
                    RomAddress = _cpu.Registers.ProgramCounter;
                    UpdateRomPage();
                }
            }
            if (LastExecutionResult.RegisterIndex != 0) UpdateRegisters();
            if (LastExecutionResult.Flags) OnPropertyChanged(nameof(Flags));
            OnPropertyChanged(nameof(ProgramCounter));
            if (_cpu.Registers.HaltFlag) OnPropertyChanged(nameof(IsHalted));
        }

        [RelayCommand]
        private void ClickStep()
        {
            _busy.Reset();
            IsPaused = false;
            StepSimulation();
            IsPaused = true;
        }

        [RelayCommand]
        private void ClickRun()
        {
            ClickStop();
            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += (_, e) =>
            {
                while (!_cpu.Registers.HaltFlag)
                {
                    _busy.WaitOne();
                    if (_worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    Dispatcher.UIThread.Invoke(StepSimulation);
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }

                IsRunning = false;
                IsPaused = false;
            };
            _worker.RunWorkerCompleted += (_, _) => { IsRunning = false; };
            if (!_worker.IsBusy)
                _worker.RunWorkerAsync();
            _busy.Set();
            IsRunning = true;
            IsPaused = false;
        }

        [RelayCommand]
        private void ClickPause()
        {
            _busy.Reset();
            Console.WriteLine("Worker paused...");
            IsPaused = true;
        }

        [RelayCommand]
        private void ClickContinue()
        {
            _busy.Set();
            Console.WriteLine("Worker continues...");
            IsPaused = false;
        }

        [RelayCommand]
        private void ClickStop()
        {
            if (_worker is { IsBusy : true })
            {
                _worker.CancelAsync();
                Console.WriteLine("Worker stopped.");
            }

            _cpu.ClearRegisters();
            UpdateRegisters();
            OnPropertyChanged(nameof(ProgramCounter));
            IsPaused = false;
            IsRunning = false;
        }

        [RelayCommand]
        private void ClickCompile()
        {
            Console.WriteLine("Compiling...");
            try
            {
                var image = HekateAssembler.Assemble(CurrentProgram);
                Console.WriteLine("Compiled successfully.");
                _cpu.LoadProgramAt(image, 0x0000);
                UpdateRomPage();
            }
            catch (Exception e)
            {
                MessageBoxManager.GetMessageBoxStandard("Error", e.Message).ShowAsync();
            }
        }

        [RelayCommand]
        private void ClickReset()
        {
            _cpu.ClearRegisters();
            _cpu.ClearRam();
            UpdateRegisters();
            UpdateRamPage();
            OnPropertyChanged(nameof(ProgramCounter));
            OnPropertyChanged(nameof(IsHalted));
        }

        [RelayCommand]
        private void RefreshDevices()
        {
            DeviceManager.RefreshDevices();
        }

        [RelayCommand]
        private void ApplyDeviceConfig()
        {
            _cpu.Mapper.ClearDevices();
            foreach (var config in DeviceManager.Devices)
            {
                if (!config.IsActive) continue;
                var address = ushort.Parse(config.StartAddress, NumberStyles.HexNumber);
                var length = ushort.Parse(config.Length, NumberStyles.HexNumber);
                var device = PeripheralDevice.Connect(config.PortName);
                var success = (device != null) && _cpu.Mapper.MapDevice(address, length, device);
                config.IsActive = success;
                OnPropertyChanged(nameof(DeviceManager.Devices));
                Console.WriteLine("Device {0} was {1}activated: {2}", config.PortName, success ? "" : "not ",
                    device?.DeviceName);
            }
        }
    }
}