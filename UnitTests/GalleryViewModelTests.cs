using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using Moq;
using RandomBackgroundApp;
using Xunit;

namespace UnitTests;

public class GalleryViewModelTests
{
    private readonly GalleryViewModel _viewModel;

    public GalleryViewModelTests()
    {
        _viewModel = new GalleryViewModel();
    }

    [Fact]
    public void StartButtonCommand_ShouldStartTimer_WhenNotRunning()
    {
        // Arrange
        _viewModel.SelectedFolderPath = "C:\\Images";
        _viewModel.IntervalTextInput = "5";

        // Act
        _viewModel.StartButtonCommand.Execute(null);

        // Assert
        Assert.Equal("Stop", _viewModel.StartButtonText);
    }

    [Fact]
    public void StartButtonCommand_ShouldStopTimer_WhenRunning()
    {
        // Arrange
        _viewModel.SelectedFolderPath = "C:\\Images";
        _viewModel.IntervalTextInput = "5";
        _viewModel.StartButtonCommand.Execute(null); // Start the timer

        // Act
        _viewModel.StartButtonCommand.Execute(null); // Stop the timer

        // Assert
        Assert.Equal("Start", _viewModel.StartButtonText);
    }

    [Fact]
    public void SelectFolderButtonCommand_ShouldSetImageFiles_WhenFolderSelected()
    {
        // Arrange
        var dialogMock = new Mock<CommonOpenFileDialog>();
        dialogMock.Setup(d => d.ShowDialog()).Returns(CommonFileDialogResult.Ok);
        dialogMock.Setup(d => d.FileName).Returns("C:\\Images");

        // Act
        _viewModel.OpenFolderButtonCommand.Execute(null);

        // Assert
        Assert.NotNull(_viewModel.ImageFiles);
        Assert.True(_viewModel.ImageFiles.Count > 0);
    }

    [Fact]
    public void SetRandomBackground_ShouldSelectRandomImage_WhenImagesAvailable()
    {
        // Arrange
        _viewModel.ImageFiles = new ObservableCollection<ImageDto>
        {
            new ImageDto { Path = "C:\\Images\\image1.jpg" },
            new ImageDto { Path = "C:\\Images\\image2.jpg" }
        };

        // Act
        _viewModel.SetRandomBackground();

        // Assert
        Assert.NotNull(_viewModel.ImageFiles.FirstOrDefault(x => x.IsSelected));
    }

    [Fact]
    public void SetRandomBackground_ShouldShowMessage_WhenNoImagesAvailable()
    {
        // Arrange
        _viewModel.ImageFiles = new ObservableCollection<ImageDto>();

        // Act
        _viewModel.SetRandomBackground();

        // Assert
        // Check if MessageBox.Show was called with the expected message
    }
}
