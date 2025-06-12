using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.IO;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.ViewModels;

namespace Frontend.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OpenButton_Clicked(object sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return;
            var file = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions()
                {
                    Title = "Open Assembly File",
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Hekate25 Assembly File")
                        {
                            Patterns = ["*.hkt", "*.s", "*.asm", "*.txt"],
                            MimeTypes = ["text/plain"]
                        }
                    ]
                });
            if (file.Count != 1) return;
            await using var stream = await file[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            var fileContent = await streamReader.ReadToEndAsync();
            (DataContext as MainWindowViewModel)!.CurrentProgram = fileContent;
        }

        private async void SaveButton_Clicked(object sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return;
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Assembly File",
                FileTypeChoices =
                [
                    new FilePickerFileType("Hekate25 Assembly File")
                    {
                        Patterns = ["*.hkt"], MimeTypes = ["text/plain"]
                    }
                ]
            });
            if (file is null) return;
            var text = (DataContext as MainWindowViewModel)!.CurrentProgram;
            await using var stream = await file.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(text);
        }

        private async void ExportButton_Clicked(object sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return;
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export RAM Image",
                FileTypeChoices =
                [
                    new FilePickerFileType("Hekate25 RAM Image")
                    {
                        Patterns = ["*.hkr"], MimeTypes = ["application/octet-stream"]
                    }
                ]
            });
            if (file is null) return;
            var image = (DataContext as MainWindowViewModel)!.GetRamImage();
            await using var stream = await file.OpenWriteAsync();
            var streamWriter = new BinaryWriter(stream);
            streamWriter.Write(image);
        }

        private async void ImportButton_Clicked(object sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return;
            var file = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions()
                {
                    Title = "Import RAM Image",
                    AllowMultiple = false,
                    FileTypeFilter = 
                    [
                        new FilePickerFileType("Hekate25 RAM Image")
                        {
                            Patterns = ["*.hkr"], MimeTypes = ["application/octet-stream"]
                        }
                    ]
                });
            if (file.Count != 1) return;
            await using var stream = await file[0].OpenReadAsync();
            var streamReader = new BinaryReader(stream);
            var image = streamReader.ReadBytes((int) stream.Length);
            
            (DataContext as MainWindowViewModel)!.SetRamImage(image);
        }
    }
}