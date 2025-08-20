/*
    Filename: SettingsBindingServiceTests.cs
    Last Updated: 2025-08-09 16:00 CEST
    Version: 1.2.J
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Comprehensive unit tests for SettingsBindingService per Focus 17.2.0 Phase 2 implementation.
    Tests all context-aware validation, property binding, and complex business logic scenarios.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Audiobook_Compressor.Models;
using Audiobook_Compressor.Services;
using Moq;
using Xunit;

namespace AudiobookCompressor.Tests.Services
{
    public class SettingsBindingServiceTests
    {
        #region Test Fixtures

        private Mock<IValidationService> CreateMockValidationService()
        {
            var mock = new Mock<IValidationService>();
            
            // Default valid validation responses
            mock.Setup(v => v.ValidateBitrate(It.IsAny<string>(), out It.Ref<string>.IsAny))
                .Returns((string input, out string normalized) =>
                {
                    normalized = input;
                    return new ValidationResult { IsValid = true };
                });
            
            mock.Setup(v => v.ValidateSampleRate(It.IsAny<string>()))
                .Returns(new ValidationResult { IsValid = true });
            
            return mock;
        }

        private ApplicationSettings CreateTestSettings()
        {
            return new ApplicationSettings
            {
                CurrentMode = "Mono",
                MonoMode = new ModeSettings
                {
                    SelectedAction = "Copy",
                    Main = new CompressionSettings
                    {
                        TargetBitrate = "64k",
                        SampleRate = "22050",
                        ConversionThreshold = "96k",
                        EncodingType = "ABR",
                        PassMode = "1-Pass"
                    }
                },
                StereoMode = new ModeSettings
                {
                    SelectedAction = "Convert",
                    Main = new CompressionSettings
                    {
                        TargetBitrate = "128k",
                        SampleRate = "44100",
                        ConversionThreshold = "192k",
                        EncodingType = "ABR",
                        PassMode = "2-Pass"
                    }
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidValidationService_InitializesCorrectly()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();

            // Act
            var service = new SettingsBindingService(mockValidationService.Object);

            // Assert
            Assert.Equal("", service.SelectedBitrate);
            Assert.Equal("", service.SelectedSampleRate);
            Assert.Equal("", service.SelectedThreshold);
            Assert.Equal("ABR", service.SelectedEncodingType);
            Assert.Equal("1-Pass", service.SelectedPassMode);
            Assert.True(service.IsPassModeEnabled);
        }

        [Fact]
        public void Constructor_WithNullValidationService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SettingsBindingService(null));
        }

        #endregion

        #region Context Management Tests

        [Fact]
        public void SetSettingsContext_WithMonoMode_UpdatesPropertiesCorrectly()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();

            // Act
            service.SetSettingsContext(settings, "Mono");

            // Assert
            Assert.Equal("64k", service.SelectedBitrate);
            Assert.Equal("22050", service.SelectedSampleRate);
            Assert.Equal("96k", service.SelectedThreshold);
            Assert.Equal("ABR", service.SelectedEncodingType);
            Assert.Equal("1-Pass", service.SelectedPassMode);
            Assert.True(service.IsPassModeEnabled);
        }

        [Fact]
        public void SetSettingsContext_WithStereoMode_UpdatesPropertiesCorrectly()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();

            // Act
            service.SetSettingsContext(settings, "Stereo");

            // Assert
            Assert.Equal("128k", service.SelectedBitrate);
            Assert.Equal("44100", service.SelectedSampleRate);
            Assert.Equal("192k", service.SelectedThreshold);
            Assert.Equal("ABR", service.SelectedEncodingType);
            Assert.Equal("2-Pass", service.SelectedPassMode);
            Assert.True(service.IsPassModeEnabled);
        }

        [Fact]
        public void SetSettingsContext_WithNullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.SetSettingsContext(null, "Mono"));
        }

        [Fact]
        public void RefreshBindings_RaisesPropertyChangedForAllProperties()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            service.SetSettingsContext(settings, "Mono");

            var propertiesChanged = new HashSet<string>();
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                    propertiesChanged.Add(e.PropertyName);
            };

            // Act
            service.RefreshBindings();

            // Assert
            Assert.Contains(nameof(ISettingsBindingService.SelectedBitrate), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.SelectedSampleRate), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.SelectedThreshold), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.SelectedEncodingType), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.SelectedPassMode), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.IsPassModeEnabled), propertiesChanged);
        }

        #endregion

        #region Bitrate Property Tests

        [Fact]
        public void SelectedBitrate_WithValidInput_UpdatesSettingsAndRaisesPropertyChanged()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            mockValidationService.Setup(v => v.ValidateBitrate("96k", out It.Ref<string>.IsAny))
                .Returns((string input, out string normalized) =>
                {
                    normalized = "96k";
                    return new ValidationResult { IsValid = true };
                });

            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            service.SetSettingsContext(settings, "Mono");

            bool propertyChangedRaised = false;
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ISettingsBindingService.SelectedBitrate))
                    propertyChangedRaised = true;
            };

            // Act
            service.SelectedBitrate = "96k";

            // Assert
            Assert.Equal("96k", service.SelectedBitrate);
            Assert.Equal("96k", settings.MonoMode.Main.TargetBitrate);
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void SelectedBitrate_WithInvalidInput_DoesNotUpdateSettings()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            mockValidationService.Setup(v => v.ValidateBitrate("invalid", out It.Ref<string>.IsAny))
                .Returns((string input, out string normalized) =>
                {
                    normalized = input;
                    return new ValidationResult 
                    { 
                        IsValid = false, 
                        Errors = new List<string> { "Invalid bitrate" } 
                    };
                });

            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            service.SetSettingsContext(settings, "Mono");

            bool validationErrorRaised = false;
            service.ValidationError += (s, e) => validationErrorRaised = true;

            // Act
            service.SelectedBitrate = "invalid";

            // Assert
            Assert.Equal("64k", service.SelectedBitrate); // Should remain unchanged
            Assert.Equal("64k", settings.MonoMode.Main.TargetBitrate);
            Assert.True(validationErrorRaised);
        }

        #endregion

        #region Encoding Type and Pass Mode Tests

        [Fact]
        public void SelectedEncodingType_ChangedToCBR_ForcesPassModeTo1Pass()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            service.SetSettingsContext(settings, "Stereo"); // Stereo has 2-Pass initially

            var propertiesChanged = new HashSet<string>();
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                    propertiesChanged.Add(e.PropertyName);
            };

            // Act
            service.SelectedEncodingType = "CBR";

            // Assert
            Assert.Equal("CBR", service.SelectedEncodingType);
            Assert.Equal("1-Pass", service.SelectedPassMode);
            Assert.False(service.IsPassModeEnabled);
            Assert.Contains(nameof(ISettingsBindingService.IsPassModeEnabled), propertiesChanged);
        }

        [Fact]
        public void IsPassModeEnabled_WithABR_ReturnsTrue()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            service.SetSettingsContext(settings, "Mono");

            // Act & Assert
            Assert.True(service.IsPassModeEnabled);
        }

        [Fact]
        public void IsPassModeEnabled_WithCBR_ReturnsFalse()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            settings.MonoMode.Main.EncodingType = "CBR";
            service.SetSettingsContext(settings, "Mono");

            // Act & Assert
            Assert.False(service.IsPassModeEnabled);
        }

        #endregion

        #region Cross-Field Validation Tests

        [Fact]
        public void ValidateCurrentSettings_WithTargetBitrateHigherThanThreshold_RaisesWarning()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            
            // Set target bitrate higher than threshold
            settings.MonoMode.Main.TargetBitrate = "128k";
            settings.MonoMode.Main.ConversionThreshold = "96k";
            service.SetSettingsContext(settings, "Mono");

            bool validationWarningRaised = false;
            ValidationWarningEventArgs? warningArgs = null;
            service.ValidationWarning += (s, e) =>
            {
                validationWarningRaised = true;
                warningArgs = e;
            };

            // Act
            service.ValidateCurrentSettings();

            // Assert
            Assert.True(validationWarningRaised);
            Assert.NotNull(warningArgs);
            Assert.Contains("Target bitrate is higher than conversion threshold", warningArgs.Warnings[0]);
        }

        #endregion

        #region Mode Switching Integration Tests

        [Fact]
        public void ModeSwitch_FromMonoToStereo_UpdatesAllProperties()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            service.SetSettingsContext(settings, "Mono");

            // Verify initial Mono state
            Assert.Equal("64k", service.SelectedBitrate);
            Assert.Equal("22050", service.SelectedSampleRate);

            // Act - Switch to Stereo mode
            service.SetSettingsContext(settings, "Stereo");

            // Assert - Verify Stereo state
            Assert.Equal("128k", service.SelectedBitrate);
            Assert.Equal("44100", service.SelectedSampleRate);
            Assert.Equal("192k", service.SelectedThreshold);
            Assert.Equal("2-Pass", service.SelectedPassMode);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void PropertySetters_WithoutSettingsContext_DoNotThrowException()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);

            // Act & Assert - Should not throw
            service.SelectedBitrate = "96k";
            service.SelectedSampleRate = "44100";
            service.SelectedThreshold = "128k";
            service.SelectedEncodingType = "CBR";
            service.SelectedPassMode = "2-Pass";

            // Properties should return defaults
            Assert.Equal("", service.SelectedBitrate);
            Assert.Equal("", service.SelectedSampleRate);
            Assert.Equal("", service.SelectedThreshold);
        }

        #endregion

        #region Property Change Notification Tests

        [Fact]
        public void AllPropertySetters_RaisePropertyChangedEvents()
        {
            // Arrange
            var mockValidationService = CreateMockValidationService();
            var service = new SettingsBindingService(mockValidationService.Object);
            var settings = CreateTestSettings();
            service.SetSettingsContext(settings, "Mono");

            var propertiesChanged = new HashSet<string>();
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                    propertiesChanged.Add(e.PropertyName);
            };

            // Act
            service.SelectedBitrate = "96k";
            service.SelectedSampleRate = "44100";
            service.SelectedThreshold = "128k";
            service.SelectedEncodingType = "CBR";
            service.SelectedPassMode = "1-Pass";

            // Assert
            Assert.Contains(nameof(ISettingsBindingService.SelectedBitrate), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.SelectedSampleRate), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.SelectedThreshold), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.SelectedEncodingType), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.SelectedPassMode), propertiesChanged);
            Assert.Contains(nameof(ISettingsBindingService.IsPassModeEnabled), propertiesChanged);
        }

        #endregion
    }
}