/*
    Filename: AudioProcessingDeciderTests.cs
    Last Updated: 2025-08-07 12:43 CEST
    Version: 1.2.E
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Comprehensive xUnit test suite for AudioProcessingDecider pure logic engine, covering all core, advanced, and "Defer to Rockit" processing scenarios.
*/

using Xunit;
using Audiobook_Compressor.Services;
using Audiobook_Compressor.Models;

namespace AudiobookCompressor.Tests
{
    public class AudioProcessingDeciderTests
    {
        [Fact]
        public void MonoFile_MonoMode_Copy_BelowThreshold_CopiesFile()
        {
            var settings = new CompressionSettings { ConversionThreshold = "64k" };
            var fileInfo = new DetailedFileInfo { Bitrate = 48000, Channels = 1, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Mono", "Copy", settings, fileInfo);
            Assert.Equal(ProcessingActionType.Copy, result.ActionType);
            Assert.Equal("Below threshold", result.Reason);
        }

        [Fact]
        public void MonoFile_MonoMode_Convert_AboveThreshold_ConvertsFile()
        {
            var settings = new CompressionSettings { ConversionThreshold = "48k" };
            var fileInfo = new DetailedFileInfo { Bitrate = 64000, Channels = 1, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Mono", "Convert", settings, fileInfo);
            Assert.Equal(ProcessingActionType.Convert, result.ActionType);
        }

        [Fact]
        public void StereoFile_MonoMode_Copy_ExceptionPath_CopiesFile()
        {
            var settings = new CompressionSettings { ConversionThreshold = "64k" };
            var fileInfo = new DetailedFileInfo { Bitrate = 48000, Channels = 2, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Mono", "Copy", settings, fileInfo);
            Assert.Equal(ProcessingActionType.Copy, result.ActionType);
            Assert.Equal("Exception path: Copy", result.Reason);
        }

        [Fact]
        public void StereoFile_MonoMode_Convert_ConvertsToMono()
        {
            var settings = new CompressionSettings { ConversionThreshold = "64k" };
            var fileInfo = new DetailedFileInfo { Bitrate = 128000, Channels = 2, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Mono", "Convert", settings, fileInfo);
            Assert.Equal(ProcessingActionType.Convert, result.ActionType);
            Assert.Equal("Stereo to Mono", result.Reason);
        }

        [Fact]
        public void MonoFile_StereoMode_Convert_UpmixesToStereo()
        {
            var settings = new CompressionSettings { ConversionThreshold = "64k" };
            var fileInfo = new DetailedFileInfo { Bitrate = 48000, Channels = 1, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Stereo", "Convert", settings, fileInfo);
            Assert.Equal(ProcessingActionType.Convert, result.ActionType);
            Assert.Equal("Mono to Stereo (upmix)", result.Reason);
        }

        [Fact]
        public void Advanced_DeferToRockit_SameChannels_CopiesFile()
        {
            var settings = new CompressionSettings { ConversionThreshold = "64k", SubThresholdAction = "DeferToRockit", ChannelMode = "Mono" };
            var fileInfo = new DetailedFileInfo { Bitrate = 48000, Channels = 1, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Mono", "Advanced", settings, fileInfo);
            Assert.Equal(ProcessingActionType.DeferToRockit_Copy, result.ActionType);
        }

        [Fact]
        public void Advanced_DeferToRockit_Downmix_VBRWithMaxrate()
        {
            var settings = new CompressionSettings { ConversionThreshold = "64k", SubThresholdAction = "DeferToRockit", ChannelMode = "Mono" };
            var fileInfo = new DetailedFileInfo { Bitrate = 96000, Channels = 2, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Mono", "Advanced", settings, fileInfo);
            Assert.Equal(ProcessingActionType.DeferToRockit_Downmix, result.ActionType);
            Assert.Equal(96000, result.MaxRate);
            Assert.Equal(1, result.Channels);
        }

        [Fact]
        public void Advanced_DeferToRockit_Upmix_VBRWithMaxrate()
        {
            var settings = new CompressionSettings { ConversionThreshold = "64k", SubThresholdAction = "DeferToRockit", ChannelMode = "Stereo" };
            var fileInfo = new DetailedFileInfo { Bitrate = 48000, Channels = 1, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Stereo", "Advanced", settings, fileInfo);
            Assert.Equal(ProcessingActionType.DeferToRockit_Upmix, result.ActionType);
            Assert.Equal((int)(48000 * 2 * 1.10), result.MaxRate);
            Assert.Equal(2, result.Channels);
        }

        [Fact]
        public void Advanced_ConvertToCustom_UsesCustomBitrate()
        {
            var settings = new CompressionSettings { ConversionThreshold = "64k", SubThresholdAction = "ConvertTo", CustomTargetBitrate = "96k" };
            var fileInfo = new DetailedFileInfo { Bitrate = 48000, Channels = 1, Codec = "aac" };
            var result = AudioProcessingDecider.Decide("Mono", "Advanced", settings, fileInfo);
            Assert.Equal(ProcessingActionType.ConvertToCustom, result.ActionType);
            Assert.Equal("96k", result.TargetBitrate);
        }
    }
}
