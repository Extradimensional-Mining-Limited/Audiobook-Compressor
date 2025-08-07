/*
    Filename: AudioProcessingDecider.cs
    Last Updated: 2025-08-07 12:43 CEST
    Version: 1.2.E
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Pure logic decision engine for audio processing paths enabling comprehensive unit testing of all processing scenarios without file or process side effects.
*/

using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    public enum ProcessingActionType
    {
        Copy,
        Convert,
        Advanced,
        DeferToRockit_Copy,
        DeferToRockit_Downmix,
        DeferToRockit_Upmix,
        ConvertToCustom,
        Error
    }

    public class ProcessingDecision
    {
        public ProcessingActionType ActionType { get; set; }
        public string? Reason { get; set; }
        public string? TargetBitrate { get; set; }
        public int? MaxRate { get; set; }
        public int? Channels { get; set; }
    }

    public static class AudioProcessingDecider
    {
        public static ProcessingDecision Decide(
            string mode,
            string selectedAction,
            CompressionSettings settings,
            DetailedFileInfo fileInfo)
        {
            // Advanced path always checks sub-threshold logic
            if (selectedAction == "Advanced")
            {
                if (settings.SubThresholdAction == "DeferToRockit")
                {
                    var targetChannels = settings.ChannelMode == "Mono" ? 1 : 2;
                    if (fileInfo.Channels > targetChannels)
                        return new ProcessingDecision { ActionType = ProcessingActionType.DeferToRockit_Downmix, MaxRate = fileInfo.Bitrate, Channels = targetChannels, Reason = "DeferToRockit: Downmix" };
                    if (fileInfo.Channels < targetChannels)
                        return new ProcessingDecision { ActionType = ProcessingActionType.DeferToRockit_Upmix, MaxRate = (int)(fileInfo.Bitrate * 2 * 1.10), Channels = targetChannels, Reason = "DeferToRockit: Upmix" };
                    if (fileInfo.Channels == targetChannels)
                        return new ProcessingDecision { ActionType = ProcessingActionType.DeferToRockit_Copy, Reason = "DeferToRockit: Same channels" };
                }
                if (Settings.TryParseBitrate(settings.ConversionThreshold, out int threshold))
                {
                    if (fileInfo.Bitrate >= threshold)
                        return new ProcessingDecision { ActionType = ProcessingActionType.Convert, Reason = "Advanced: Above threshold" };
                    // Sub-threshold
                    switch (settings.SubThresholdAction)
                    {
                        case "Copy":
                            return new ProcessingDecision { ActionType = ProcessingActionType.Copy, Reason = "Advanced: Sub-threshold Copy" };
                        case "ConvertTo":
                            return new ProcessingDecision { ActionType = ProcessingActionType.ConvertToCustom, TargetBitrate = settings.CustomTargetBitrate, Reason = "Advanced: Sub-threshold ConvertTo" };
                    }
                }
                return new ProcessingDecision { ActionType = ProcessingActionType.Error, Reason = "Advanced: Invalid threshold" };
            }
            // Main mode match (non-advanced)
            if ((mode == "Mono" && fileInfo.Channels == 1) || (mode == "Stereo" && fileInfo.Channels == 2))
            {
                if (Settings.TryParseBitrate(settings.ConversionThreshold, out int threshold))
                {
                    if (fileInfo.Bitrate <= threshold)
                        return new ProcessingDecision { ActionType = ProcessingActionType.Copy, Reason = "Below threshold" };
                    else
                        return new ProcessingDecision { ActionType = ProcessingActionType.Convert, Reason = "Above threshold" };
                }
                return new ProcessingDecision { ActionType = ProcessingActionType.Error, Reason = "Invalid threshold" };
            }
            // Exception path
            if (selectedAction == "Copy")
                return new ProcessingDecision { ActionType = ProcessingActionType.Copy, Reason = "Exception path: Copy" };
            if (selectedAction == "Convert")
            {
                if (mode == "Mono")
                    return new ProcessingDecision { ActionType = ProcessingActionType.Convert, Reason = "Stereo to Mono" };
                else
                    return new ProcessingDecision { ActionType = ProcessingActionType.Convert, Reason = "Mono to Stereo (upmix)" };
            }
            return new ProcessingDecision { ActionType = ProcessingActionType.Error, Reason = "Unmatched path" };
        }
    }
}
