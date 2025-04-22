using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Models;
using Simulator;

namespace Frontend.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<MemoryGridRowModel> RamPageZero { get; set; } = [];

        private byte[] _ram = new byte[512];

        [ObservableProperty] private string _greet = "Hello";

        public MainWindowViewModel()
        {
            Random.Shared.NextBytes(_ram);
            RamPageZero.Clear();
            for (byte row = 0; row < 16; row++)
                RamPageZero.Add(new MemoryGridRowModel(
                    (row<<4).ToString("X4"),
                    new Span<byte>(_ram, (row<<4), 16)
                        .ToArray().Select(e => e.ToString("X2")).ToArray()
                    ));
        }
    }
}