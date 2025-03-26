using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using RandomBackgroundApp.Commands;

namespace RandomBackgroundApp;

public class GalleryViewModel : INotifyPropertyChanged
{

    private static readonly string[] ValidExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
    ImageDto SelectedImage;
    string SelectedFolderPath;
    public ObservableCollection<ImageDto> ImageFiles { get; set; }
    private DispatcherTimer timer;
    private bool isTimerRunning = false;
    public ICommand StartButtonCommand { get; }
    public ICommand OpenFolderButtonCommand { get; }
    private string _startButtonText = "Start";
    public string StartButtonText
    {
        get => _startButtonText;
        set
        {
            _startButtonText = value;
            OnPropertyChanged(nameof(StartButtonText));
        }
    }
    private string _intervalTextInput = "5";
    public string IntervalTextInput
    {
        get => _intervalTextInput;
        set
        {
            _intervalTextInput = value;
            OnPropertyChanged(nameof(IntervalTextInput));
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    public GalleryViewModel()
    {

        timer = new DispatcherTimer();
        timer.Tick += Timer_Tick;
        StartButtonCommand = new RelayCommand(StartStop_Click);
        OpenFolderButtonCommand = new RelayCommand(SelectFolderButton_Click);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        SetRandomBackground();
    }

    private void StartStop_Click(object sender)
    {
        try
        {
            if (isTimerRunning)
            {
                timer.Stop();
                _startButtonText = "Start";
                //statusText.Text = "Stopped";
                isTimerRunning = false;
            }
            else
            {
                if (string.IsNullOrEmpty(SelectedFolderPath))
                {
                    MessageBox.Show("Please select a folder first", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(IntervalTextInput, out int minutes) || minutes <= 0)
                {
                    MessageBox.Show("Please enter a valid interval in minutes", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                timer.Interval = TimeSpan.FromMinutes(minutes);
                timer.Start();
                _startButtonText = "Stop";
                //statusText.Text = $"Running - Changing every {minutes} minutes";
                isTimerRunning = true;
            }

            OnPropertyChanged(nameof(StartButtonText));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error controlling timer: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SelectFolderButton_Click(object sender)
    {
        try
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = "Select Image Folder";

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    SelectedFolderPath = dialog.FileName;
                    ImageFiles = [.. Directory.GetFiles(SelectedFolderPath).Where(f => ValidExtensions.Contains(Path.GetExtension(f).ToLower())).Select(x => new ImageDto {
                            Path = x,
                        })];
                    OnPropertyChanged(nameof(ImageFiles));
                    SetRandomBackground();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error selecting folder: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetRandomBackground()
    {
        try
        {
            if (ImageFiles.Count == 0)
            {
                MessageBox.Show("No valid images found in selected folder", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedImage = ImageFiles[Random.Shared.Next(ImageFiles.Count)];
            var selected = ImageFiles.FirstOrDefault(x => x.Path == SelectedImage.Path);
            if (selected != null)
            {
                selected.IsSelected = true;
                OnPropertyChanged(nameof(ImageFiles));
            }
            WallpaperSetter.SetWallpaper(SelectedImage.Path);
            //statusText.Text = $"Last changed: {DateTime.Now.ToString("HH:mm:ss")}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error setting background: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


}
