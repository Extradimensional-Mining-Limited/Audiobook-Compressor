/*
    Filename: PanelVisibilityServiceTests.cs
    Last Updated: 2025-08-09 15:05 CEST
    Version: 1.2.I
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Comprehensive unit tests for PanelVisibilityService per Focus 16.2.0 Phase 1 implementation.
    Tests all visibility logic for mode-dependent panel management.
*/

using System.Collections.Generic;
using System.ComponentModel;
using Audiobook_Compressor.Services;
using Xunit;

namespace AudiobookCompressor.Tests.Services
{
    public class PanelVisibilityServiceTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Arrange & Act
            var service = new PanelVisibilityService();

            // Assert
            Assert.True(service.IsMonoModeVisible);
            Assert.False(service.IsStereoModeVisible);
            Assert.False(service.IsAdvancedPanelVisible);
            Assert.False(service.IsMonoAdvancedPanelVisible);
            Assert.False(service.IsStereoAdvancedPanelVisible);
        }

        #endregion

        #region Mode Visibility Tests

        [Fact]
        public void UpdateVisibilityForMode_WithMonoMode_ShowsMonoModeHidesStereoMode()
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.UpdateVisibilityForMode("Mono", "Copy", "Copy");

            // Assert
            Assert.True(service.IsMonoModeVisible);
            Assert.False(service.IsStereoModeVisible);
        }

        [Fact]
        public void UpdateVisibilityForMode_WithStereoMode_ShowsStereoModeHidesMonoMode()
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.UpdateVisibilityForMode("Stereo", "Copy", "Copy");

            // Assert
            Assert.False(service.IsMonoModeVisible);
            Assert.True(service.IsStereoModeVisible);
        }

        #endregion

        #region Advanced Panel Visibility Tests

        [Theory]
        [InlineData("Mono", "Advanced", "Copy", true)]
        [InlineData("Mono", "Copy", "Copy", false)]
        [InlineData("Mono", "Convert", "Copy", false)]
        [InlineData("Stereo", "Copy", "Advanced", true)]
        [InlineData("Stereo", "Copy", "Copy", false)]
        [InlineData("Stereo", "Copy", "Convert", false)]
        public void UpdateVisibilityForMode_WithAdvancedAction_ShowsHidesAdvancedPanel(
            string mode, string monoAction, string stereoAction, bool expectedAdvancedVisible)
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.UpdateVisibilityForMode(mode, monoAction, stereoAction);

            // Assert
            Assert.Equal(expectedAdvancedVisible, service.IsAdvancedPanelVisible);
        }

        #endregion

        #region Mono Advanced Panel Tests

        [Theory]
        [InlineData("Mono", "Advanced", "Copy", true)]
        [InlineData("Mono", "Copy", "Copy", false)]
        [InlineData("Mono", "Convert", "Copy", false)]
        [InlineData("Stereo", "Advanced", "Copy", false)]
        [InlineData("Stereo", "Copy", "Advanced", false)]
        public void UpdateVisibilityForMode_MonoAdvancedPanelVisibility(
            string mode, string monoAction, string stereoAction, bool expectedVisible)
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.UpdateVisibilityForMode(mode, monoAction, stereoAction);

            // Assert
            Assert.Equal(expectedVisible, service.IsMonoAdvancedPanelVisible);
        }

        #endregion

        #region Stereo Advanced Panel Tests

        [Theory]
        [InlineData("Stereo", "Copy", "Advanced", true)]
        [InlineData("Stereo", "Copy", "Copy", false)]
        [InlineData("Stereo", "Copy", "Convert", false)]
        [InlineData("Mono", "Copy", "Advanced", false)]
        [InlineData("Mono", "Advanced", "Copy", false)]
        public void UpdateVisibilityForMode_StereoAdvancedPanelVisibility(
            string mode, string monoAction, string stereoAction, bool expectedVisible)
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.UpdateVisibilityForMode(mode, monoAction, stereoAction);

            // Assert
            Assert.Equal(expectedVisible, service.IsStereoAdvancedPanelVisible);
        }

        #endregion

        #region Null Parameter Handling Tests

        [Fact]
        public void UpdateVisibilityForMode_WithNullParameters_UsesDefaultValues()
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.UpdateVisibilityForMode(null, null, null);

            // Assert
            Assert.True(service.IsMonoModeVisible);
            Assert.False(service.IsStereoModeVisible);
            Assert.False(service.IsAdvancedPanelVisible);
            Assert.False(service.IsMonoAdvancedPanelVisible);
            Assert.False(service.IsStereoAdvancedPanelVisible);
        }

        #endregion

        #region RefreshAllVisibility Tests

        [Fact]
        public void RefreshAllVisibility_RaisesPropertyChangedForAllProperties()
        {
            // Arrange
            var service = new PanelVisibilityService();
            var propertiesChanged = new HashSet<string>();
            
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                    propertiesChanged.Add(e.PropertyName);
            };

            // Act
            service.RefreshAllVisibility();

            // Assert
            Assert.Contains(nameof(IPanelVisibilityService.IsMonoModeVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsStereoModeVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsAdvancedPanelVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsMonoAdvancedPanelVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsStereoAdvancedPanelVisible), propertiesChanged);
        }

        #endregion

        #region PropertyChanged Tests

        [Fact]
        public void UpdateVisibilityForMode_ModeChange_RaisesPropertyChangedForModeVisibility()
        {
            // Arrange
            var service = new PanelVisibilityService();
            var propertiesChanged = new HashSet<string>();
            
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                    propertiesChanged.Add(e.PropertyName);
            };

            // Act
            service.UpdateVisibilityForMode("Stereo", "Copy", "Copy");

            // Assert
            Assert.Contains(nameof(IPanelVisibilityService.IsMonoModeVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsStereoModeVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsAdvancedPanelVisible), propertiesChanged);
        }

        [Fact]
        public void UpdateVisibilityForMode_MonoActionChange_RaisesPropertyChangedForMonoAdvanced()
        {
            // Arrange
            var service = new PanelVisibilityService();
            service.UpdateVisibilityForMode("Mono", "Copy", "Copy");
            var propertiesChanged = new HashSet<string>();
            
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                    propertiesChanged.Add(e.PropertyName);
            };

            // Act
            service.UpdateVisibilityForMode("Mono", "Advanced", "Copy");

            // Assert
            Assert.Contains(nameof(IPanelVisibilityService.IsMonoAdvancedPanelVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsAdvancedPanelVisible), propertiesChanged);
        }

        [Fact]
        public void UpdateVisibilityForMode_StereoActionChange_RaisesPropertyChangedForStereoAdvanced()
        {
            // Arrange
            var service = new PanelVisibilityService();
            service.UpdateVisibilityForMode("Stereo", "Copy", "Copy");
            var propertiesChanged = new HashSet<string>();
            
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                    propertiesChanged.Add(e.PropertyName);
            };

            // Act
            service.UpdateVisibilityForMode("Stereo", "Copy", "Advanced");

            // Assert
            Assert.Contains(nameof(IPanelVisibilityService.IsStereoAdvancedPanelVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsAdvancedPanelVisible), propertiesChanged);
        }

        [Fact]
        public void UpdateVisibilityForMode_NoChanges_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var service = new PanelVisibilityService();
            service.UpdateVisibilityForMode("Mono", "Copy", "Copy");
            bool propertyChangedRaised = false;
            
            service.PropertyChanged += (s, e) => propertyChangedRaised = true;

            // Act
            service.UpdateVisibilityForMode("Mono", "Copy", "Copy");

            // Assert
            Assert.False(propertyChangedRaised);
        }

        #endregion

        #region Complex Scenario Tests

        [Fact]
        public void ComplexScenario_MonoAdvancedToStereoConvert_UpdatesAllVisibilityCorrectly()
        {
            // Arrange
            var service = new PanelVisibilityService();
            
            // Start with Mono Advanced
            service.UpdateVisibilityForMode("Mono", "Advanced", "Copy");
            
            // Verify initial state
            Assert.True(service.IsMonoModeVisible);
            Assert.False(service.IsStereoModeVisible);
            Assert.True(service.IsAdvancedPanelVisible);
            Assert.True(service.IsMonoAdvancedPanelVisible);
            Assert.False(service.IsStereoAdvancedPanelVisible);

            // Act - Switch to Stereo Convert
            service.UpdateVisibilityForMode("Stereo", "Advanced", "Convert");

            // Assert - Verify final state
            Assert.False(service.IsMonoModeVisible);
            Assert.True(service.IsStereoModeVisible);
            Assert.False(service.IsAdvancedPanelVisible);
            Assert.False(service.IsMonoAdvancedPanelVisible);
            Assert.False(service.IsStereoAdvancedPanelVisible);
        }

        #endregion

        #region Initialize Method Tests

        [Fact]
        public void Initialize_WithValidParameters_SetsInternalStateCorrectly()
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.Initialize("Stereo", "Advanced", "Copy");

            // Assert
            Assert.False(service.IsMonoModeVisible);
            Assert.True(service.IsStereoModeVisible);
            Assert.False(service.IsAdvancedPanelVisible); // Stereo mode with Copy action
            Assert.False(service.IsMonoAdvancedPanelVisible);
            Assert.False(service.IsStereoAdvancedPanelVisible);
        }

        [Fact]
        public void Initialize_WithAdvancedAction_ShowsCorrectAdvancedPanel()
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.Initialize("Mono", "Advanced", "Convert");

            // Assert
            Assert.True(service.IsMonoModeVisible);
            Assert.False(service.IsStereoModeVisible);
            Assert.True(service.IsAdvancedPanelVisible); // Mono mode with Advanced action
            Assert.True(service.IsMonoAdvancedPanelVisible);
            Assert.False(service.IsStereoAdvancedPanelVisible);
        }

        [Fact]
        public void Initialize_WithNullParameters_UsesDefaultValues()
        {
            // Arrange
            var service = new PanelVisibilityService();

            // Act
            service.Initialize(null, null, null);

            // Assert
            Assert.True(service.IsMonoModeVisible); // Default mode
            Assert.False(service.IsStereoModeVisible);
            Assert.False(service.IsAdvancedPanelVisible); // Default Copy action
            Assert.False(service.IsMonoAdvancedPanelVisible);
            Assert.False(service.IsStereoAdvancedPanelVisible);
        }

        [Fact]
        public void Initialize_RaisesPropertyChangedForAllProperties()
        {
            // Arrange
            var service = new PanelVisibilityService();
            var propertiesChanged = new HashSet<string>();
            
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                    propertiesChanged.Add(e.PropertyName);
            };

            // Act
            service.Initialize("Stereo", "Copy", "Advanced");

            // Assert - Verify all properties received change notifications
            Assert.Contains(nameof(IPanelVisibilityService.IsMonoModeVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsStereoModeVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsAdvancedPanelVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsMonoAdvancedPanelVisible), propertiesChanged);
            Assert.Contains(nameof(IPanelVisibilityService.IsStereoAdvancedPanelVisible), propertiesChanged);
        }

        #endregion
    }
}