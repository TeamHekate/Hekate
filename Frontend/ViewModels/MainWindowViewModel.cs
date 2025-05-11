using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Simulator;

namespace Frontend.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<MemoryGridRowModel> RamPage { get; set; } = [];
        public ObservableCollection<MemoryGridRowModel> RomPage { get; set; } = [];

        [ObservableProperty] private ushort _ramAddress;
        [ObservableProperty] private ushort _romAddress;

        public string ProgramCounter => _cpu.Registers.ProgramCounter.ToString("X4");
        public string[] Flags => [
            _cpu.Registers.ZeroFlag ? "1" : "0",
            _cpu.Registers.CarryFlag ? "1" : "0",
            _cpu.Registers.SignFlag ? "1" : "0",
            _cpu.Registers.OverflowFlag ? "1" : "0",
            _cpu.Registers.HaltFlag ? "1" : "0"
        ];

        private ExecutionResult? LastExecutionResult { get; set; }

        private readonly HekateInstance _cpu = new HekateInstance();

        private bool _isRunning = false;
        private bool _isPaused = false;
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

        private void WorkCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            IsRunning = false;
        }

        private void DoWork(object? sender, DoWorkEventArgs e)
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
                Thread.Sleep(10);
            }
            
            Console.WriteLine("HALT!");
            IsRunning = false;
            IsPaused = false;
        }

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
                    (row << 4).ToString("X4"),
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
                    (row << 4).ToString("X4"),
                    new Span<byte>(page, (row << 4), 16)
                        .ToArray().Select(e => e.ToString("X2")).ToArray()
                ));
            OnPropertyChanged(nameof(RomPage));
        }

        public MainWindowViewModel()
        {

            _cpu.LoadProgramAt(
                [0x20, 0x20, 0x10, 0x10, 0x12, 0x41, 0x83, 0x00, 0xFF], 0x80);
            _cpu.ClearRegisters(0x80);
            UpdateRegisters();
            UpdateRamPage();
            UpdateRomPage();
            OnPropertyChanged(nameof(ProgramCounter));
        }

        private void StepSimulation()
        {
            if (_cpu.Registers.HaltFlag) return;
            LastExecutionResult = _cpu.Step();
            if (LastExecutionResult.Ram)
                RamAddress =
                    (ushort)((LastExecutionResult.RamPage << 8) | (LastExecutionResult.RamOffset));
            if (LastExecutionResult.RegisterIndex != 0) UpdateRegisters();
            if (LastExecutionResult.Flags) OnPropertyChanged(nameof(Flags));
            OnPropertyChanged(nameof(ProgramCounter));
            if (_cpu.Registers.HaltFlag) OnPropertyChanged(nameof(IsHalted));
        }
        
        [RelayCommand]
        private void ClickStep()
        {
            IsRunning = true;
            StepSimulation();
            IsRunning = false;
        }

        [RelayCommand]
        private void ClickRun()
        {
            ClickStop();
            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += DoWork;
            _worker.RunWorkerCompleted += WorkCompleted;
            if (!_worker.IsBusy)
            {
                _worker.RunWorkerAsync();
            }
            _busy.Set();
            Console.WriteLine("Worker running!");
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
            if (_worker is {IsBusy : true})
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
        private void ClickReset()
        {
            _cpu.ClearRegisters();
            UpdateRegisters();
            OnPropertyChanged(nameof(ProgramCounter));
            OnPropertyChanged(nameof(IsHalted));
        }
    }
}