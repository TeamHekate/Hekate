using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace Frontend.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

     
            this.FindControl<Button>("RunButton").Click += RunButton_Click;
            this.FindControl<Button>("StepButton").Click += StepButton_Click;
            this.FindControl<Button>("OpenButton").Click += OpenButton_Click;
            this.FindControl<Button>("SaveButton").Click += SaveButton_Click;
            this.FindControl<Button>("ExportButton").Click += ExportButton_Click;
            this.FindControl<Button>("ImportButton").Click += ImportButton_Click;
        }

        private void RunButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("RUN clicked");
        }

        private void StepButton_Click(object? sender, RoutedEventArgs e)
        {
          
            Console.WriteLine("STEP clicked");
        }

        private void OpenButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("OPEN clicked");
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("SAVE clicked");
        }

        private void ExportButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("EXPORT clicked");
        }

        private void ImportButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("IMPORT clicked");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

