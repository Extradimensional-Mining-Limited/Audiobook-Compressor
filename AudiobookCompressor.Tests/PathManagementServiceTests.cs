/*
    Filename: PathManagementServiceTests.cs
    Last Updated: 2025-08-09 19:55 CEST
    Version: 1.2.K
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Phase 4 Modularization per Focus 18.2.0: Comprehensive unit tests for PathManagementService.
    Tests path operations, validation, collision detection, and event coordination.
*/

using System;
using System.Threading.Tasks;
using Audiobook_Compressor.Services;
using Moq;
using Xunit;

namespace AudiobookCompressor.Tests.Services
{
    /// <summary>
    /// Unit tests for PathManagementService
    /// </summary>
    public class PathManagementServiceTests
    {
        #region Test Helpers

        /// <summary>
        /// Creates a PathManagementService with mocked dependencies
        /// </summary>
        private (PathManagementService service, Mock<IDialogService> dialogMock, Mock<IValidationService> validationMock, Mock<ISettingsService> settingsMock) CreateService()
        {
            var dialogMock = new Mock<IDialogService>();
            var validationMock = new Mock<IValidationService>();
            var settingsMock = new Mock<ISettingsService>();

            var service = new PathManagementService(dialogMock.Object, validationMock.Object, settingsMock.Object);

            return (service, dialogMock, validationMock, settingsMock);
        }

        /// <summary>
        /// Creates a valid ServiceValidationResult
        /// </summary>
        private ServiceValidationResult CreateValidValidationResult()
        {
            return new ServiceValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates an invalid ServiceValidationResult
        /// </summary>
        private ServiceValidationResult CreateInvalidValidationResult(string error)
        {
            return new ServiceValidationResult 
            { 
                IsValid = false, 
                Errors = { error } 
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesService()
        {
            // Arrange & Act
            var (service, _, _, _) = CreateService();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullDialogService_ThrowsArgumentNullException()
        {
            // Arrange
            var validationMock = new Mock<IValidationService>();
            var settingsMock = new Mock<ISettingsService>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new PathManagementService(null!, validationMock.Object, settingsMock.Object));
        }

        [Fact]
        public void Constructor_WithNullValidationService_ThrowsArgumentNullException()
        {
            // Arrange
            var dialogMock = new Mock<IDialogService>();
            var settingsMock = new Mock<ISettingsService>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new PathManagementService(dialogMock.Object, null!, settingsMock.Object));
        }

        [Fact]
        public void Constructor_WithNullSettingsService_ThrowsArgumentNullException()
        {
            // Arrange
            var dialogMock = new Mock<IDialogService>();
            var validationMock = new Mock<IValidationService>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new PathManagementService(dialogMock.Object, validationMock.Object, null!));
        }

        #endregion

        #region BrowseSourcePathAsync Tests

        [Fact]
        public async Task BrowseSourcePathAsync_WithValidSelection_ReturnsSuccess()
        {
            // Arrange
            var (service, dialogMock, validationMock, _) = CreateService();
            var selectedPath = @"C:\TestSource";
            var currentPath = @"C:\OldSource";

            dialogMock.Setup(d => d.ShowFolderDialog("Select Source Library Folder", currentPath))
                      .Returns(selectedPath);
            validationMock.Setup(v => v.ValidatePaths(selectedPath, selectedPath))
                          .Returns(CreateValidValidationResult());

            // Act
            var result = await service.BrowseSourcePathAsync(currentPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(selectedPath, result.NewPath);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task BrowseSourcePathAsync_WithUserCancel_ReturnsFailure()
        {
            // Arrange
            var (service, dialogMock, _, _) = CreateService();
            var currentPath = @"C:\OldSource";

            dialogMock.Setup(d => d.ShowFolderDialog("Select Source Library Folder", currentPath))
                      .Returns(string.Empty);

            // Act
            var result = await service.BrowseSourcePathAsync(currentPath);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.NewPath);
        }

        [Fact]
        public async Task BrowseSourcePathAsync_WithInvalidPath_ReturnsFailure()
        {
            // Arrange
            var (service, dialogMock, validationMock, _) = CreateService();
            var selectedPath = @"C:\InvalidPath";
            var currentPath = @"C:\OldSource";
            var validationError = "Path does not exist";

            dialogMock.Setup(d => d.ShowFolderDialog("Select Source Library Folder", currentPath))
                      .Returns(selectedPath);
            validationMock.Setup(v => v.ValidatePaths(selectedPath, selectedPath))
                          .Returns(CreateInvalidValidationResult(validationError));

            // Act
            var result = await service.BrowseSourcePathAsync(currentPath);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.NewPath);
            Assert.Contains(validationError, result.Errors);
        }

        [Fact]
        public async Task BrowseSourcePathAsync_RaisesPathChangedEvent()
        {
            // Arrange
            var (service, dialogMock, validationMock, _) = CreateService();
            var selectedPath = @"C:\TestSource";
            var currentPath = @"C:\OldSource";
            PathChangedEventArgs? eventArgs = null;

            dialogMock.Setup(d => d.ShowFolderDialog("Select Source Library Folder", currentPath))
                      .Returns(selectedPath);
            validationMock.Setup(v => v.ValidatePaths(selectedPath, selectedPath))
                          .Returns(CreateValidValidationResult());

            service.PathChanged += (sender, e) => eventArgs = e;

            // Act
            await service.BrowseSourcePathAsync(currentPath);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal("Source", eventArgs.PathType);
            Assert.Equal(selectedPath, eventArgs.NewPath);
            Assert.Equal(currentPath, eventArgs.PreviousPath);
            Assert.True(eventArgs.RequiresCollisionCheck);
        }

        #endregion

        #region BrowseOutputPathAsync Tests

        [Fact]
        public async Task BrowseOutputPathAsync_WithValidSelection_ReturnsSuccess()
        {
            // Arrange
            var (service, dialogMock, validationMock, _) = CreateService();
            var selectedPath = @"C:\TestOutput";
            var currentPath = @"C:\OldOutput";

            dialogMock.Setup(d => d.ShowFolderDialog("Select Output Folder", currentPath))
                      .Returns(selectedPath);
            validationMock.Setup(v => v.ValidatePaths(selectedPath, selectedPath))
                          .Returns(CreateValidValidationResult());

            // Act
            var result = await service.BrowseOutputPathAsync(currentPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(selectedPath, result.NewPath);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task BrowseOutputPathAsync_RaisesPathChangedEvent()
        {
            // Arrange
            var (service, dialogMock, validationMock, _) = CreateService();
            var selectedPath = @"C:\TestOutput";
            var currentPath = @"C:\OldOutput";
            PathChangedEventArgs? eventArgs = null;

            dialogMock.Setup(d => d.ShowFolderDialog("Select Output Folder", currentPath))
                      .Returns(selectedPath);
            validationMock.Setup(v => v.ValidatePaths(selectedPath, selectedPath))
                          .Returns(CreateValidValidationResult());

            service.PathChanged += (sender, e) => eventArgs = e;

            // Act
            await service.BrowseOutputPathAsync(currentPath);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal("Output", eventArgs.PathType);
            Assert.Equal(selectedPath, eventArgs.NewPath);
            Assert.Equal(currentPath, eventArgs.PreviousPath);
            Assert.True(eventArgs.RequiresCollisionCheck);
        }

        #endregion

        #region SaveDefaultOutputPathAsync Tests

        [Fact]
        public async Task SaveDefaultOutputPathAsync_WithValidPath_ReturnsSuccess()
        {
            // Arrange
            var (service, _, _, settingsMock) = CreateService();
            var outputPath = @"C:\TestOutput";

            // Act
            var result = await service.SaveDefaultOutputPathAsync(outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Default output path saved successfully.", result.Message);
            settingsMock.Verify(s => s.SetDefaultOutputPath(outputPath), Times.Once);
        }

        [Fact]
        public async Task SaveDefaultOutputPathAsync_WithEmptyPath_ReturnsFailure()
        {
            // Arrange
            var (service, _, _, _) = CreateService();

            // Act
            var result = await service.SaveDefaultOutputPathAsync(string.Empty);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Output path cannot be empty", result.Errors);
        }

        [Fact]
        public async Task SaveDefaultOutputPathAsync_WithNullPath_ReturnsFailure()
        {
            // Arrange
            var (service, _, _, _) = CreateService();

            // Act
            var result = await service.SaveDefaultOutputPathAsync(null!);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Output path cannot be empty", result.Errors);
        }

        #endregion

        #region RestoreDefaultOutputPathAsync Tests

        [Fact]
        public async Task RestoreDefaultOutputPathAsync_WithValidDefault_ReturnsSuccess()
        {
            // Arrange
            var (service, _, validationMock, settingsMock) = CreateService();
            var defaultPath = @"C:\DefaultOutput";

            settingsMock.Setup(s => s.GetDefaultOutputPath()).Returns(defaultPath);
            validationMock.Setup(v => v.ValidatePaths(defaultPath, defaultPath))
                          .Returns(CreateValidValidationResult());

            // Act
            var result = await service.RestoreDefaultOutputPathAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(defaultPath, result.NewPath);
        }

        [Fact]
        public async Task RestoreDefaultOutputPathAsync_WithNoDefault_ReturnsFailure()
        {
            // Arrange
            var (service, _, _, settingsMock) = CreateService();

            settingsMock.Setup(s => s.GetDefaultOutputPath()).Returns(string.Empty);

            // Act
            var result = await service.RestoreDefaultOutputPathAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains("No default output path is configured", result.Errors);
        }

        [Fact]
        public async Task RestoreDefaultOutputPathAsync_WithInvalidDefault_ReturnsFailure()
        {
            // Arrange
            var (service, _, validationMock, settingsMock) = CreateService();
            var defaultPath = @"C:\InvalidDefault";

            settingsMock.Setup(s => s.GetDefaultOutputPath()).Returns(defaultPath);
            validationMock.Setup(v => v.ValidatePaths(defaultPath, defaultPath))
                          .Returns(CreateInvalidValidationResult("Path not found"));

            // Act
            var result = await service.RestoreDefaultOutputPathAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Default output path is no longer valid", result.Errors);
        }

        #endregion

        #region ValidatePathCombination Tests

        [Fact]
        public void ValidatePathCombination_WithValidPaths_ReturnsValid()
        {
            // Arrange
            var (service, _, validationMock, _) = CreateService();
            var sourcePath = @"C:\Source";
            var outputPath = @"C:\Output";

            validationMock.Setup(v => v.ValidatePaths(sourcePath, outputPath))
                          .Returns(new ServiceValidationResult { IsValid = true });

            // Act
            var result = service.ValidatePathCombination(sourcePath, outputPath);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidatePathCombination_WithInvalidPaths_ReturnsInvalid()
        {
            // Arrange
            var (service, _, validationMock, _) = CreateService();
            var sourcePath = @"C:\Source";
            var outputPath = @"C:\Output";
            var validationError = "Invalid path combination";

            validationMock.Setup(v => v.ValidatePaths(sourcePath, outputPath))
                          .Returns(new ServiceValidationResult 
                          { 
                              IsValid = false, 
                              Errors = { validationError } 
                          });

            // Act
            var result = service.ValidatePathCombination(sourcePath, outputPath);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(validationError, result.Errors);
        }

        #endregion

        #region CheckPathCollisions Tests

        [Fact]
        public void CheckPathCollisions_WithSamePaths_DetectsCollision()
        {
            // Arrange
            var (service, _, _, _) = CreateService();
            var path = @"C:\SamePath";

            // Act
            var result = service.CheckPathCollisions(path, path);

            // Assert
            Assert.True(result.HasCollisions);
            Assert.True(result.RequiresUserConfirmation);
            Assert.Contains("Source and Output folders are the same", result.Collisions);
            Assert.NotNull(result.ConfirmationMessage);
        }

        [Fact]
        public void CheckPathCollisions_WithDifferentPaths_NoCollision()
        {
            // Arrange
            var (service, _, _, _) = CreateService();
            var sourcePath = @"C:\Source";
            var outputPath = @"C:\Output";

            // Act
            var result = service.CheckPathCollisions(sourcePath, outputPath);

            // Assert
            Assert.False(result.HasCollisions);
            Assert.False(result.RequiresUserConfirmation);
            Assert.Empty(result.Collisions);
        }

        [Fact]
        public void CheckPathCollisions_CaseInsensitive_DetectsCollision()
        {
            // Arrange
            var (service, _, _, _) = CreateService();
            var sourcePath = @"C:\Path";
            var outputPath = @"c:\path";

            // Act
            var result = service.CheckPathCollisions(sourcePath, outputPath);

            // Assert
            Assert.True(result.HasCollisions);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task BrowseSourcePathAsync_WithException_ReturnsError()
        {
            // Arrange
            var (service, dialogMock, _, _) = CreateService();
            var currentPath = @"C:\Source";

            dialogMock.Setup(d => d.ShowFolderDialog(It.IsAny<string>(), It.IsAny<string>()))
                      .Throws(new Exception("Dialog error"));

            // Act
            var result = await service.BrowseSourcePathAsync(currentPath);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error browsing source path", result.Errors[0]);
        }

        [Fact]
        public async Task SaveDefaultOutputPathAsync_WithException_ReturnsError()
        {
            // Arrange
            var (service, _, _, settingsMock) = CreateService();
            var outputPath = @"C:\Output";

            settingsMock.Setup(s => s.SetDefaultOutputPath(It.IsAny<string>()))
                        .Throws(new Exception("Settings error"));

            // Act
            var result = await service.SaveDefaultOutputPathAsync(outputPath);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error saving default output path", result.Errors[0]);
        }

        #endregion
    }
}