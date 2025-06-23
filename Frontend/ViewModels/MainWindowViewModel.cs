using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Assembler;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using MsBox.Avalonia;
using Simulator;
using Simulator.Peripheral;
using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Styling;

namespace Frontend.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<MemoryGridRowModel> RamPage { get; set; } = [];
        public ObservableCollection<MemoryGridRowModel> RomPage { get; set; } = [];

        public ObservableCollection<PeripheralDeviceModel> Devices { get; } = [];

        // Theme Management Properties using Avalonia's ThemeVariant
        [ObservableProperty] private bool _isWhiteTheme = false;
        
        public string ThemeToggleText => IsWhiteTheme ? "Dark" : "Light";
        public string CurrentThemeName => IsWhiteTheme ? "Light Theme" : "Dark Theme";
        
        // This property controls Avalonia's RequestedThemeVariant
        public ThemeVariant CurrentThemeVariant => IsWhiteTheme ? ThemeVariant.Light : ThemeVariant.Dark;

        partial void OnIsWhiteThemeChanged(bool value)
        {
            Console.WriteLine($"Theme changed to: {(value ? "Light" : "Dark")} Theme");
            
            // Notify all theme-related properties to update
            OnPropertyChanged(nameof(ThemeToggleText));
            OnPropertyChanged(nameof(CurrentThemeName));
            OnPropertyChanged(nameof(CurrentThemeVariant));
            
            Console.WriteLine($"Theme variant set to: {CurrentThemeVariant}");
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            Console.WriteLine($"ToggleTheme command called! Current theme: {(IsWhiteTheme ? "Light" : "Dark")}");
            IsWhiteTheme = !IsWhiteTheme;
            Console.WriteLine($"Theme toggled to: {(IsWhiteTheme ? "Light" : "Dark")}");
        }

        private ushort _ramAddress;

        private ushort RamAddress
        {
            get => _ramAddress;
            set
            {
                var temp = _ramAddress;
                _ramAddress = value;
                RamAddressString = _ramAddress.ToString("X4");
                if ((temp & 0xff00) != (value & 0xff00))
                {
                    UpdateRamPage();
                }

                OnPropertyChanged();
            }
        }

        private ushort _romAddress;

        private ushort RomAddress
        {
            get => _romAddress;
            set
            {
                var temp = _romAddress;
                _romAddress = value;
                RomAddressString = _romAddress.ToString("X4");
                if ((temp & 0xff00) != (value & 0xff00))
                {
                    UpdateRomPage();
                }

                OnPropertyChanged();
            }
        }

        [ObservableProperty] private string _ramAddressString = "0000";
        [ObservableProperty] private string _romAddressString = "0000";

        public string ProgramCounter => _cpu.Registers.ProgramCounter.ToString("X4");
        public string StackPointer => _cpu.Registers.StackPointer.ToString("X4");

        public string[] Flags =>
        [
            _cpu.Registers.ZeroFlag ? "1" : "0",
            _cpu.Registers.CarryFlag ? "1" : "0",
            _cpu.Registers.SignFlag ? "1" : "0",
            _cpu.Registers.OverflowFlag ? "1" : "0",
            _cpu.Registers.HaltFlag ? "1" : "0"
        ];

        [ObservableProperty] private string _currentProgram = "";

        private ExecutionResult? LastExecutionResult { get; set; }
        private readonly HekateInstance _cpu = new();

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
        private readonly ManualResetEvent _busy = new(false);

        public ObservableCollection<string> Registers { get; } = [];

        private void UpdateRegisters()
        {
            Registers.Clear();
            for (var i = 0; i < 16; i++) Registers.Add(_cpu.Registers[i].ToString("X2"));
        }

        public void UpdateRamPage()
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

        public byte[] GetRamImage() => _cpu.DumpRam();
        public void SetRamImage(byte[] image) {
            _cpu.LoadRamImage(image);
            UpdateRamPage();
        }

        public MainWindowViewModel()
        {
            UpdateRegisters();
            UpdateRamPage();
            UpdateRomPage();
            RefreshDevices();
            OnPropertyChanged(nameof(ProgramCounter));
            
            Console.WriteLine("MainWindowViewModel constructor completed");
        }

        private void StepSimulation()
        {
            if (_cpu.Registers.HaltFlag) return;
            LastExecutionResult = _cpu.Step();
            if (LastExecutionResult.Ram)
            {
                var addr = (ushort)((LastExecutionResult.RamPage << 8) | (LastExecutionResult.RamOffset));
                RamAddress = addr;
                var newData = _cpu.ReadRamLocation(addr, out var _).ToString("X2");
                var oldLoc = new string[16];
                RamPage[LastExecutionResult.RamOffset >> 4].Locations.CopyTo(oldLoc, 0);
                oldLoc[LastExecutionResult.RamOffset & 0xf] = newData;
                RamPage[LastExecutionResult.RamOffset >> 4] =
                    new MemoryGridRowModel((RamAddress & 0xfff0).ToString("X4"), oldLoc);
                OnPropertyChanged(nameof(RamPage));
            }

            RomAddress = _cpu.Registers.ProgramCounter;


            if (LastExecutionResult.RegisterIndex != 0) UpdateRegisters();
            if (LastExecutionResult.Flags) OnPropertyChanged(nameof(Flags));
            if (LastExecutionResult.Sp) OnPropertyChanged(nameof(StackPointer));
            OnPropertyChanged(nameof(ProgramCounter));
            if (_cpu.Registers.HaltFlag) OnPropertyChanged(nameof(IsHalted));
        }

        [RelayCommand]
        private void ClickStep()
        {
            if (!IsRunning)
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
                IsRunning = true;
            }

            _busy.Reset();
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
                ClickStop();
            }
            catch (Exception e)
            {
                MessageBoxManager.GetMessageBoxStandard("Error", e.Message + "\n" + e.StackTrace).ShowAsync();
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
            IsPaused = false;
            IsRunning = false;
        }

        private void RefreshDevices()
        {
            Console.WriteLine("Refreshing devices.");
            var ports = SerialPort.GetPortNames();
            foreach (var portName in ports)
            {
                if (Devices.All(elem => elem.PortName != portName))
                    Devices.Add(new PeripheralDeviceModel(portName));
            }

            List<PeripheralDeviceModel> removeList = [];
            foreach (var device in Devices)
                if (!ports.Contains(device.PortName))
                    removeList.Add(Devices.First(e => e.PortName == device.PortName));

            foreach (var rem in removeList)
                Devices.Remove(rem);

            Console.WriteLine("Found {0} devices.", Devices.Count);
        }

        [RelayCommand]
        private void OnRefreshDevices()
        {
            RefreshDevices();
        }

        [RelayCommand]
        private void OnApplyDevices()
        {
            _cpu.Mapper.ClearDevices();
            for (var i = 0; i < Devices.Count; i++)
            {
                var config = Devices[i];
                if (!config.IsActive) continue;
                var address = (ushort)int.Parse(config.StartAddress, NumberStyles.HexNumber);
                var length = (ushort)int.Parse(config.Length, NumberStyles.HexNumber);
                var success = false;
                PeripheralDevice? device = null;
                for (var tries = 0; tries < 11 && !success; tries++)
                {
                    try
                    {
                        device = new PeripheralDevice(config.PortName);
                        success = _cpu.Mapper.MapDevice(address, length, device);
                        device.SetTimeout(200);
                    }
                    catch (Exception e)
                    {
                        if (tries == 10)
                        {
                            MessageBoxManager.GetMessageBoxStandard("I/O Manager Error",
                                    $"Could not bind memory-mapped I/O at port {config.PortName} after 10 tries:\n\t" +
                                    e.Message)
                                .ShowAsync();
                        }
                        else
                        {
                            Console.Error.WriteLine(
                                $"Failed to bind memory-mapped I/O, retrying... (Tries: {tries + 1})");
                        }
                    }
                }
                
                Devices[i] = new PeripheralDeviceModel(config.PortName)
                {
                    StartAddress = config.StartAddress,
                    Length = config.Length,
                    IsActive = success,
                    PortName = config.PortName,
                    DeviceName = device?.DeviceName ?? "-"
                };
                
                OnPropertyChanged(nameof(Devices));
            }
        }

        [RelayCommand]
        private void ClickRamAddress()
        {
            RamAddress = (ushort)int.Parse(RamAddressString, NumberStyles.HexNumber);
        }
        
        [RelayCommand]
        private void ClickRomAddress()
        {
            RomAddress = (ushort)int.Parse(RomAddressString, NumberStyles.HexNumber);
        }
    }
}