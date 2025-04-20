using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace Frontend.ViewModels
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            InitializeMemoryGrid();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitializeMemoryGrid()
        {
            // Find the ItemsControl for the memory grid
            var memoryGrid = this.FindControl<ItemsControl>("MemoryGrid");
            if (memoryGrid != null)
            {
                // Create 1024 items (32x32 grid)
                var items = new List<int>();
                for (int i = 0; i < 1024; i++)
                {
                    items.Add(i);
                }
                // Use ItemsSource instead of Items
                memoryGrid.ItemsSource = items;
            }
        }
    }
}