/*
    Filename: RadioButtonStateServiceTests.cs
    Last Updated: 2025-08-09 19:40 CEST
    Version: 1.2.K
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Phase 3 Modularization per Focus 18.2.0: Comprehensive unit tests for RadioButtonStateService.
    Tests radio button state coordination, mode management, and event-driven coordination.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Audiobook_Compressor.Models;
using Audiobook_Compressor.Services;
using Xunit;

namespace AudiobookCompressor.Tests.Services
{
    /// <summary>
    /// Unit tests for RadioButtonStateService
    /// </summary>
    public class RadioButtonStateServiceTests
    {
        #region Test Helpers

        /// <summary>
        /// Creates a test ApplicationSettings with default values
        /// </summary>
        private ApplicationSettings CreateTestSettings()
        {
            return new ApplicationSettings
            {
                CurrentMode = "Mono"
            };
        }

        /// <summary>
        /// Creates a RadioButtonStateService and initializes it with test settings
        /// </summary>
        private (RadioButtonStateService service, ApplicationSettings settings) CreateInitializedService()
        {
            var service = new RadioButtonStateService();
            var settings = CreateTestSettings();
            service.Initialize(settings);
            return (service, settings);
        }

        #endregion

        #region Initialization Tests

        [Fact]
        public void Initialize_WithValidSettings_SetsStateCorrectly()
        {
            // Arrange
            var service = new RadioButtonStateService();
            var settings = CreateTestSettings();
            settings.MonoMode.SelectedAction = "Convert";
            settings.StereoMode.SelectedAction = "Copy";

            // Act
            service.Initialize(settings);

            // Assert
            Assert.False(service.IsMonoCopySelected);
            Assert.True(service.IsMonoConvertSelected);
            Assert.False(service.IsMonoAdvancedSelected);
            Assert.True(service.IsStereoCopySelected);
            Assert.False(service.IsStereoConvertSelected);
            Assert.False(service.IsStereoAdvancedSelected);
        }

        [Fact]
        public void Initialize_WithNullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new RadioButtonStateService();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.Initialize(null!));
        }

        [Fact]
        public void Initialize_FiresPropertyChangedForAllStates()
        {
            // Arrange
            var service = new RadioButtonStateService();
            var settings = CreateTestSettings();
            var propertyChangedEvents = new List<string>();

            service.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            service.Initialize(settings);

            // Assert
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoCopySelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoConvertSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoAdvancedSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoCopySelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoConvertSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoAdvancedSelected), propertyChangedEvents);
        }

        #endregion

        #region Mono Action Tests

        [Theory]
        [InlineData("Copy")]
        [InlineData("Convert")]
        [InlineData("Advanced")]
        public void SetMonoSelectedAction_WithValidAction_UpdatesStateCorrectly(string action)
        {
            // Arrange
            var (service, settings) = CreateInitializedService();

            // Act
            service.SetMonoSelectedAction(action);

            // Assert
            Assert.Equal(action, settings.MonoMode.SelectedAction);
            Assert.Equal(action == "Advanced", settings.IsAdvancedMode);
            
            Assert.Equal(action == "Copy", service.IsMonoCopySelected);
            Assert.Equal(action == "Convert", service.IsMonoConvertSelected);
            Assert.Equal(action == "Advanced", service.IsMonoAdvancedSelected);
        }

        [Fact]
        public void SetMonoSelectedAction_WithSameAction_DoesNotTriggerEvents()
        {
            // Arrange
            var (service, settings) = CreateInitializedService();
            settings.MonoMode.SelectedAction = "Copy";
            
            var stateChangedFired = false;
            service.StateChanged += (sender, e) => stateChangedFired = true;

            // Act
            service.SetMonoSelectedAction("Copy");

            // Assert
            Assert.False(stateChangedFired);
        }

        [Fact]
        public void SetMonoSelectedAction_FiresStateChangedEvent()
        {
            // Arrange
            var (service, _) = CreateInitializedService();
            RadioButtonStateChangedEventArgs? eventArgs = null;
            
            service.StateChanged += (sender, e) => eventArgs = e;

            // Act
            service.SetMonoSelectedAction("Advanced");

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal("Mono", eventArgs.Mode);
            Assert.Equal("Advanced", eventArgs.Action);
            Assert.True(eventArgs.IsAdvancedMode);
            Assert.True(eventArgs.RequiresPanelVisibilityUpdate);
            Assert.True(eventArgs.RequiresSettingsSummaryUpdate);
        }

        [Fact]
        public void SetMonoSelectedAction_FiresPropertyChangedEvents()
        {
            // Arrange
            var (service, _) = CreateInitializedService();
            var propertyChangedEvents = new List<string>();

            service.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            service.SetMonoSelectedAction("Advanced");

            // Assert
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoCopySelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoConvertSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoAdvancedSelected), propertyChangedEvents);
        }

        #endregion

        #region Stereo Action Tests

        [Theory]
        [InlineData("Copy")]
        [InlineData("Convert")]
        [InlineData("Advanced")]
        public void SetStereoSelectedAction_WithValidAction_UpdatesStateCorrectly(string action)
        {
            // Arrange
            var (service, settings) = CreateInitializedService();

            // Act
            service.SetStereoSelectedAction(action);

            // Assert
            Assert.Equal(action, settings.StereoMode.SelectedAction);
            Assert.Equal(action == "Advanced", settings.IsAdvancedMode);
            
            Assert.Equal(action == "Copy", service.IsStereoCopySelected);
            Assert.Equal(action == "Convert", service.IsStereoConvertSelected);
            Assert.Equal(action == "Advanced", service.IsStereoAdvancedSelected);
        }

        [Fact]
        public void SetStereoSelectedAction_WithSameAction_DoesNotTriggerEvents()
        {
            // Arrange
            var (service, settings) = CreateInitializedService();
            settings.StereoMode.SelectedAction = "Copy";
            
            var stateChangedFired = false;
            service.StateChanged += (sender, e) => stateChangedFired = true;

            // Act
            service.SetStereoSelectedAction("Copy");

            // Assert
            Assert.False(stateChangedFired);
        }

        [Fact]
        public void SetStereoSelectedAction_FiresStateChangedEvent()
        {
            // Arrange
            var (service, _) = CreateInitializedService();
            RadioButtonStateChangedEventArgs? eventArgs = null;
            
            service.StateChanged += (sender, e) => eventArgs = e;

            // Act
            service.SetStereoSelectedAction("Advanced");

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal("Stereo", eventArgs.Mode);
            Assert.Equal("Advanced", eventArgs.Action);
            Assert.True(eventArgs.IsAdvancedMode);
            Assert.True(eventArgs.RequiresPanelVisibilityUpdate);
            Assert.True(eventArgs.RequiresSettingsSummaryUpdate);
        }

        #endregion

        #region Mode Change Tests

        [Fact]
        public void UpdateForModeChange_ToMono_UpdatesAdvancedModeCorrectly()
        {
            // Arrange
            var (service, settings) = CreateInitializedService();
            settings.MonoMode.SelectedAction = "Advanced";
            settings.StereoMode.SelectedAction = "Copy";
            settings.CurrentMode = "Stereo";
            settings.IsAdvancedMode = false;

            // Act
            service.UpdateForModeChange("Mono");

            // Assert
            Assert.True(settings.IsAdvancedMode); // Should be true because Mono is Advanced
        }

        [Fact]
        public void UpdateForModeChange_ToStereo_UpdatesAdvancedModeCorrectly()
        {
            // Arrange
            var (service, settings) = CreateInitializedService();
            settings.MonoMode.SelectedAction = "Advanced";
            settings.StereoMode.SelectedAction = "Copy";
            settings.CurrentMode = "Mono";
            settings.IsAdvancedMode = true;

            // Act
            service.UpdateForModeChange("Stereo");

            // Assert
            Assert.False(settings.IsAdvancedMode); // Should be false because Stereo is Copy
        }

        [Fact]
        public void UpdateForModeChange_FiresPropertyChangedForAllStates()
        {
            // Arrange
            var (service, settings) = CreateInitializedService();
            var propertyChangedEvents = new List<string>();

            service.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            service.UpdateForModeChange("Stereo");

            // Assert
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoCopySelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoConvertSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoAdvancedSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoCopySelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoConvertSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoAdvancedSelected), propertyChangedEvents);
        }

        #endregion

        #region Refresh Tests

        [Fact]
        public void RefreshAllStates_FiresPropertyChangedForAllStates()
        {
            // Arrange
            var (service, _) = CreateInitializedService();
            var propertyChangedEvents = new List<string>();

            service.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            service.RefreshAllStates();

            // Assert
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoCopySelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoConvertSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsMonoAdvancedSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoCopySelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoConvertSelected), propertyChangedEvents);
            Assert.Contains(nameof(IRadioButtonStateService.IsStereoAdvancedSelected), propertyChangedEvents);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void SetMonoSelectedAction_WithoutInitialization_DoesNotThrow()
        {
            // Arrange
            var service = new RadioButtonStateService();

            // Act & Assert - Should not throw
            service.SetMonoSelectedAction("Copy");
        }

        [Fact]
        public void SetStereoSelectedAction_WithoutInitialization_DoesNotThrow()
        {
            // Arrange
            var service = new RadioButtonStateService();

            // Act & Assert - Should not throw
            service.SetStereoSelectedAction("Copy");
        }

        [Fact]
        public void UpdateForModeChange_WithoutInitialization_DoesNotThrow()
        {
            // Arrange
            var service = new RadioButtonStateService();

            // Act & Assert - Should not throw
            service.UpdateForModeChange("Mono");
        }

        [Fact]
        public void StateProperties_WithoutInitialization_ReturnFalse()
        {
            // Arrange
            var service = new RadioButtonStateService();

            // Act & Assert
            Assert.False(service.IsMonoCopySelected);
            Assert.False(service.IsMonoConvertSelected);
            Assert.False(service.IsMonoAdvancedSelected);
            Assert.False(service.IsStereoCopySelected);
            Assert.False(service.IsStereoConvertSelected);
            Assert.False(service.IsStereoAdvancedSelected);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void StateCoordination_MonoAdvanced_UpdatesAllRelatedState()
        {
            // Arrange
            var (service, settings) = CreateInitializedService();

            // Act
            service.SetMonoSelectedAction("Advanced");

            // Assert
            Assert.Equal("Advanced", settings.MonoMode.SelectedAction);
            Assert.True(settings.IsAdvancedMode);
            Assert.False(service.IsMonoCopySelected);
            Assert.False(service.IsMonoConvertSelected);
            Assert.True(service.IsMonoAdvancedSelected);
        }

        [Fact]
        public void StateCoordination_StereoAdvanced_UpdatesAllRelatedState()
        {
            // Arrange
            var (service, settings) = CreateInitializedService();

            // Act
            service.SetStereoSelectedAction("Advanced");

            // Assert
            Assert.Equal("Advanced", settings.StereoMode.SelectedAction);
            Assert.True(settings.IsAdvancedMode);
            Assert.False(service.IsStereoCopySelected);
            Assert.False(service.IsStereoConvertSelected);
            Assert.True(service.IsStereoAdvancedSelected);
        }

        [Fact]
        public void StateCoordination_AdvancedToNonAdvanced_ClearsAdvancedMode()
        {
            // Arrange
            var (service, settings) = CreateInitializedService();
            service.SetMonoSelectedAction("Advanced");
            Assert.True(settings.IsAdvancedMode);

            // Act
            service.SetMonoSelectedAction("Copy");

            // Assert
            Assert.False(settings.IsAdvancedMode);
            Assert.True(service.IsMonoCopySelected);
            Assert.False(service.IsMonoAdvancedSelected);
        }

        #endregion
    }
}