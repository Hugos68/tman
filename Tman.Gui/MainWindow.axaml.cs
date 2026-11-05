using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Tman.Core.Alterations;
using Tman.Interface;

namespace Tman.Gui
{
    public sealed partial class MainWindow : Window
    {
        private const string MapExtension = ".Map.Gbx";

        private const string MapPattern = "*" + MapExtension;

        private const string LoggerName = "tman";

        private readonly ObservableCollection<string> maps = [];

        private readonly LogPane logPane;

        private TmanHost? host;

        private CancellationTokenSource? running;

        public MainWindow()
        {
            InitializeComponent();
            logPane = new LogPane(line => Dispatcher.UIThread.Post(() => Append(line)));
            Alterations.ItemsSource = AlterationPipeline.Catalog.ToList();
            Maps.ItemsSource = maps;
            AddHandler(DragDrop.DropEvent, OnDrop);
            AddHandler(DragDrop.DragOverEvent, (_, e) => e.DragEffects = e.DataTransfer.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None);
            maps.CollectionChanged += (_, _) => Refresh();
        }

        private AlterationInfo? Selected => Alterations.SelectedItem as AlterationInfo;

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => Refresh();

        private void OnDrop(object? sender, DragEventArgs e)
        {
            foreach (var item in e.DataTransfer.TryGetFiles() ?? [])
            {
                var path = item.TryGetLocalPath();
                if (path is null)
                {
                    continue;
                }
                if (Directory.Exists(path))
                {
                    AddMaps(Directory.GetFiles(path, MapPattern, SearchOption.AllDirectories));
                }
                else
                {
                    AddMaps([path]);
                }
            }
        }

        private async void OnChooseMaps(object? sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Maps to alter",
                AllowMultiple = true,
                FileTypeFilter = [new FilePickerFileType("Trackmania maps") { Patterns = [MapPattern] }]
            });
            AddMaps(files.Select(file => file.TryGetLocalPath()).OfType<string>());
        }

        private async void OnChooseFolder(object? sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Folder of maps to alter" });
            foreach (var path in folders.Select(folder => folder.TryGetLocalPath()).OfType<string>())
            {
                AddMaps(Directory.GetFiles(path, MapPattern, SearchOption.AllDirectories));
            }
        }

        private async void OnChooseOutput(object? sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Output folder" });
            if (folders.FirstOrDefault()?.TryGetLocalPath() is { } path)
            {
                Output.Text = path;
            }
        }

        private void OnClear(object? sender, RoutedEventArgs e) => maps.Clear();

        private void OnCancel(object? sender, RoutedEventArgs e) => running?.Cancel();

        private async void OnRun(object? sender, RoutedEventArgs e)
        {
            if (Selected is not { } alteration || maps.Count == 0)
            {
                return;
            }
            var outputFolder = string.IsNullOrWhiteSpace(Output.Text) ? Path.Combine(Path.GetDirectoryName(maps[0])!, alteration.Name) : Output.Text;
            var paths = MapPaths.ForFolder([.. maps], outputFolder);
            host ??= TmanHost.Create(logging => logging.SetMinimumLevel(LogLevel.Information).AddProvider(logPane));
            var logger = host.Logger(LoggerName);
            using var cancellation = new CancellationTokenSource();
            running = cancellation;
            Progress.Value = 0;
            Refresh();
            try
            {
                await Task.Run(() => Batch.RunAsync(host.Pipeline, alteration.Name, paths, (done, total, output) =>
                {
                    logger.LogInformation("[{Done}/{Total}] {Alteration}: {Output}", done, total, alteration.Name, output);
                    Dispatcher.UIThread.Post(() => Progress.Value = (double)done / total);
                }, cancellation.Token), cancellation.Token);
                logger.LogInformation("Done: {Count} maps in {Folder}", maps.Count, outputFolder);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Cancelled");
            }
            catch (Exception exception)
            {
                logger.LogError("{Message}", exception.Message);
            }
            finally
            {
                running = null;
                Refresh();
            }
        }

        private void AddMaps(IEnumerable<string> paths)
        {
            foreach (var path in paths.Where(path => path.EndsWith(MapExtension, StringComparison.OrdinalIgnoreCase) && !maps.Contains(path)).Order(StringComparer.OrdinalIgnoreCase))
            {
                maps.Add(path);
            }
        }

        private void Refresh()
        {
            var busy = running is not null;
            Run.IsEnabled = !busy && Selected is not null && maps.Count > 0;
            Cancel.IsEnabled = busy;
            DropText.Text = maps.Count == 0 ? "Drop .Map.Gbx files or a folder here" : $"{maps.Count} maps";
        }

        private void Append(string line)
        {
            Log.Text += line + Environment.NewLine;
            LogScroll.ScrollToEnd();
        }
    }
}
