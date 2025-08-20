/*
    Filename: UIStateServiceTests.cs
    Last Updated: 2025-08-09 15:05 CEST
    Version: 1.2.I
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Comprehensive unit tests for UIStateService per Focus 16.2.0 Phase 1 implementation.
    Tests all status management, progress tracking, and logging functionality.
*/

using System.ComponentModel;
using Audiobook_Compressor.Services;
using Xunit;

namespace AudiobookCompressor.Tests.Services
{
    public class UIStateServiceTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesPropertiesWithDefaultValues()
        {
            // Arrange & Act
            var service = new UIStateService();

            // Assert
            Assert.Equal(0.0, service.StatusProgress);
            Assert.False(service.IsProgressVisible);
            Assert.Equal("Ready", service.StatusText);
            Assert.Equal(string.Empty, service.LogContent);
            Assert.False(service.IsProcessing);
        }

        #endregion

        #region UpdateStatus Tests

        [Fact]
        public void UpdateStatus_WithMessageOnly_UpdatesStatusTextAndHidesProgress()
        {
            // Arrange
            var service = new UIStateService();
            string testMessage = "Test status message";

            // Act
            service.UpdateStatus(testMessage);

            // Assert
            Assert.Equal(testMessage, service.StatusText);
            Assert.False(service.IsProgressVisible);
        }

        [Fact]
        public void UpdateStatus_WithMessageAndProgress_UpdatesStatusTextAndShowsProgress()
        {
            // Arrange
            var service = new UIStateService();
            string testMessage = "Processing...";
            double testProgress = 0.5;

            // Act
            service.UpdateStatus(testMessage, testProgress);

            // Assert
            Assert.Equal(testMessage, service.StatusText);
            Assert.Equal(testProgress, service.StatusProgress);
            Assert.True(service.IsProgressVisible);
        }

        [Fact]
        public void UpdateStatus_WithNullMessage_SetsStatusTextToEmpty()
        {
            // Arrange
            var service = new UIStateService();

            // Act
            service.UpdateStatus(null);

            // Assert
            Assert.Equal(string.Empty, service.StatusText);
        }

        [Theory]
        [InlineData(-0.1, 0.0)]
        [InlineData(1.1, 1.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(0.5, 0.5)]
        public void UpdateStatus_WithProgress_ClampsProgressToValidRange(double inputProgress, double expectedProgress)
        {
            // Arrange
            var service = new UIStateService();

            // Act
            service.UpdateStatus("Test", inputProgress);

            // Assert
            Assert.Equal(expectedProgress, service.StatusProgress);
        }

        #endregion

        #region UpdateProgress Tests

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.25)]
        [InlineData(0.5)]
        [InlineData(0.75)]
        [InlineData(1.0)]
        public void UpdateProgress_WithValidProgress_UpdatesProgressAndShowsProgress(double progress)
        {
            // Arrange
            var service = new UIStateService();

            // Act
            service.UpdateProgress(progress);

            // Assert
            Assert.Equal(progress, service.StatusProgress);
            Assert.True(service.IsProgressVisible);
        }

        [Theory]
        [InlineData(-0.1, 0.0)]
        [InlineData(1.1, 1.0)]
        public void UpdateProgress_WithInvalidProgress_ClampsToValidRange(double inputProgress, double expectedProgress)
        {
            // Arrange
            var service = new UIStateService();

            // Act
            service.UpdateProgress(inputProgress);

            // Assert
            Assert.Equal(expectedProgress, service.StatusProgress);
        }

        #endregion

        #region AppendLog Tests

        [Fact]
        public void AppendLog_WithMessage_AppendsMessageWithNewLine()
        {
            // Arrange
            var service = new UIStateService();
            string testMessage = "Test log message";

            // Act
            service.AppendLog(testMessage);

            // Assert
            Assert.Equal(testMessage + System.Environment.NewLine, service.LogContent);
        }

        [Fact]
        public void AppendLog_MultipleMessages_AppendsAllMessagesWithNewLines()
        {
            // Arrange
            var service = new UIStateService();
            string message1 = "First message";
            string message2 = "Second message";

            // Act
            service.AppendLog(message1);
            service.AppendLog(message2);

            // Assert
            string expected = message1 + System.Environment.NewLine + message2 + System.Environment.NewLine;
            Assert.Equal(expected, service.LogContent);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AppendLog_WithNullOrEmptyMessage_DoesNotModifyLogContent(string message)
        {
            // Arrange
            var service = new UIStateService();
            var initialContent = service.LogContent;

            // Act
            service.AppendLog(message);

            // Assert
            Assert.Equal(initialContent, service.LogContent);
        }

        #endregion

        #region ClearLog Tests

        [Fact]
        public void ClearLog_ClearsLogContent()
        {
            // Arrange
            var service = new UIStateService();
            service.AppendLog("Some log content");

            // Act
            service.ClearLog();

            // Assert
            Assert.Equal(string.Empty, service.LogContent);
        }

        #endregion

        #region SetProcessingState Tests

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetProcessingState_UpdatesIsProcessingProperty(bool processingState)
        {
            // Arrange
            var service = new UIStateService();

            // Act
            service.SetProcessingState(processingState);

            // Assert
            Assert.Equal(processingState, service.IsProcessing);
        }

        #endregion

        #region PropertyChanged Tests

        [Fact]
        public void UpdateStatus_RaisesPropertyChangedForStatusText()
        {
            // Arrange
            var service = new UIStateService();
            bool propertyChangedRaised = false;
            service.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(IUIStateService.StatusText))
                    propertyChangedRaised = true;
            };

            // Act
            service.UpdateStatus("New status");

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void UpdateProgress_RaisesPropertyChangedForStatusProgress()
        {
            // Arrange
            var service = new UIStateService();
            bool propertyChangedRaised = false;
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IUIStateService.StatusProgress))
                    propertyChangedRaised = true;
            };

            // Act
            service.UpdateProgress(0.5);

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void AppendLog_RaisesPropertyChangedForLogContent()
        {
            // Arrange
            var service = new UIStateService();
            bool propertyChangedRaised = false;
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IUIStateService.LogContent))
                    propertyChangedRaised = true;
            };

            // Act
            service.AppendLog("Test log");

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void SetProcessingState_RaisesPropertyChangedForIsProcessing()
        {
            // Arrange
            var service = new UIStateService();
            bool propertyChangedRaised = false;
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IUIStateService.IsProcessing))
                    propertyChangedRaised = true;
            };

            // Act
            service.SetProcessingState(true);

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void PropertyChangedNotRaised_WhenSettingSameValue()
        {
            // Arrange
            var service = new UIStateService();
            service.UpdateStatus("Initial");
            bool propertyChangedRaised = false;
            service.PropertyChanged += (s, e) => propertyChangedRaised = true;

            // Act
            service.UpdateStatus("Initial");

            // Assert
            Assert.False(propertyChangedRaised);
        }

        #endregion
    }
}