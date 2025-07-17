# [2025-07-13 01:48:00 CEST] - AudiobookCompressor-v6.2 (Complete and Verified)
<#
.SYNOPSIS
    A complete and verified version that fixes the file filter bug and restores the full
    processing logic that was missing from the previous version.
#>

try {
    $LogDirectory = Join-Path -Path $PSScriptRoot -ChildPath "logging"
    if (-not (Test-Path -LiteralPath $LogDirectory)) {
        New-Item -ItemType Directory -Path $LogDirectory | Out-Null
    }
    $ScriptVersion = "6.2"
    $ScriptRunTimestamp = Get-Date -Format "yyyy-MM-dd-HH-mm"
    $TranscriptLogPath = Join-Path -Path $LogDirectory -ChildPath "FullSession-Log-v$($ScriptVersion)-$($ScriptRunTimestamp).txt"
    Start-Transcript -LiteralPath $TranscriptLogPath -Append
    
    Add-Type -AssemblyName PresentationFramework, System.Windows.Forms

    [xml]$xaml = @"
    <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            Title="Audiobook Compressor v6.2" Height="600" Width="800" MinHeight="450" MinWidth="550">
        <Grid Margin="10">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <Label Grid.Row="0" Grid.Column="0" Grid.ColumnSpan="2" Content="Source Library:"/>
            <TextBox x:Name="SourcePathTextBox" Grid.Row="1" Grid.Column="0" Margin="5" VerticalContentAlignment="Center"/>
            <Button x:Name="SourceBrowseButton" Grid.Row="1" Grid.Column="1" Content="Browse..." Margin="5" Padding="10,5"/>

            <Label Grid.Row="2" Grid.Column="0" Grid.ColumnSpan="2" Content="Output Folder:"/>
            <TextBox x:Name="OutputPathTextBox" Grid.Row="3" Grid.Column="0" Margin="5" VerticalContentAlignment="Center"/>
            <Button x:Name="OutputBrowseButton" Grid.Row="3" Grid.Column="1" Content="Browse..." Margin="5" Padding="10,5"/>
            
            <StackPanel Grid.Row="4" Grid.Column="0" Grid.ColumnSpan="2" Orientation="Horizontal" HorizontalAlignment="Center" Margin="0,10,0,0">
                <Button x:Name="StartButton" Content="Start Compression" Margin="5" Padding="20,10" FontWeight="Bold" Background="#FF4CAF50" Foreground="White"/>
                <Button x:Name="CancelButton" Content="Cancel" Margin="5" Padding="20,10" FontWeight="Bold" Background="#FFE53935" Foreground="White" IsEnabled="False"/>
            </StackPanel>

            <ProgressBar x:Name="ProgressBar" Grid.Row="5" Grid.Column="0" Grid.ColumnSpan="2" Margin="5,10,5,0" Height="25"/>
            
            <TextBox x:Name="LogTextBox" Grid.Row="6" Grid.Column="0" Grid.ColumnSpan="2" Margin="5" IsReadOnly="True" VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Auto" TextWrapping="NoWrap" FontFamily="Consolas"/>

            <StatusBar Grid.Row="7" Grid.Column="0" Grid.ColumnSpan="2">
                <StatusBarItem><TextBlock x:Name="StatusTextBlock" Text="Ready"/></StatusBarItem>
                <Separator/>
                <StatusBarItem><TextBlock x:Name="ProgressTextBlock" Text=""/></StatusBarItem>
            </StatusBar>
        </Grid>
    </Window>
"@

    $reader = (New-Object System.Xml.XmlNodeReader $xaml)
    $window = [Windows.Markup.XamlReader]::Load($reader)

    $SourcePathTextBox  = $window.FindName("SourcePathTextBox")
    $SourceBrowseButton = $window.FindName("SourceBrowseButton")
    $OutputPathTextBox  = $window.FindName("OutputPathTextBox")
    $OutputBrowseButton = $window.FindName("OutputBrowseButton")
    $StartButton        = $window.FindName("StartButton")
    $CancelButton       = $window.FindName("CancelButton")
    $ProgressBar        = $window.FindName("ProgressBar")
    $LogTextBox         = $window.FindName("LogTextBox")
    $StatusTextBlock    = $window.FindName("StatusTextBlock")
    $ProgressTextBlock  = $window.FindName("ProgressTextBlock")

    $script:Job = $null
    $script:Timer = $null
    $scriptRootForJob = $PSScriptRoot

    $SourceBrowseButton.add_Click({
        $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
        if ($folderBrowser.ShowDialog() -eq "OK") {
            $SourcePathTextBox.Text = $folderBrowser.SelectedPath
            $OutputPathTextBox.Text = [System.IO.Path]::Combine($folderBrowser.SelectedPath, "Compressed_Audiobooks")
        }
    })

    $OutputBrowseButton.add_Click({
        $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
        if ($folderBrowser.ShowDialog() -eq "OK") {
            $OutputPathTextBox.Text = $folderBrowser.SelectedPath
        }
    })

    $StartButton.add_Click({
        $sourcePath = $SourcePathTextBox.Text
        $outputPath = $OutputPathTextBox.Text
        if (-not (Test-Path -LiteralPath $sourcePath) -or -not (Test-Path -LiteralPath (Split-Path $outputPath -Parent))) {
            [System.Windows.Forms.MessageBox]::Show("Please select valid source and output paths.", "Error", "OK", "Error"); return
        }

        $StartButton.IsEnabled = $false; $CancelButton.IsEnabled = $true
        $SourceBrowseButton.IsEnabled = $false; $OutputBrowseButton.IsEnabled = $false
        $StatusTextBlock.Text = "Scanning files..."; $LogTextBox.Clear()
        $ProgressBar.Value = 0; $ProgressTextBlock.Text = ""

        $AudioFileFilters = @("*.m4b", "*.mp3", "*.aac", "*.m4a", "*.flac", "*.ogg", "*.wma", "*.wav", "*.webma", "*.opus")
        $allAudioFiles = $AudioFileFilters | ForEach-Object {
            Get-ChildItem -LiteralPath $sourcePath -Filter $_ -File -Recurse -ErrorAction SilentlyContinue
        } | Where-Object { $_.DirectoryName -notlike "*\Compressed_Audiobooks_*" }
        
        $totalFiles = $allAudioFiles.Count
        $ProgressBar.Maximum = $totalFiles

        if ($totalFiles -eq 0) {
            $LogTextBox.Text = "Warning: No supported audio files found."; $StatusTextBlock.Text = "Ready"
            $StartButton.IsEnabled = $true; $CancelButton.IsEnabled = $false
            $SourceBrowseButton.IsEnabled = $true; $OutputBrowseButton.IsEnabled = $true
            return
        }
            
        $failsafeLogPath = Join-Path -Path $LogDirectory -ChildPath "failsafe_log.txt"
        if (Test-Path -LiteralPath $failsafeLogPath) { Remove-Item -LiteralPath $failsafeLogPath }

        $scriptBlock = {
            param($LibrarySourceRoot, $OutputRootFolder, $ScriptDir, $FailsafeLog)
            & {
                $filesProcessed = 0
                $TargetBitrate = "48k"; $MonoCopyThreshold = 64000; $TargetSampleRate = "22050"
                $FFprobeName = "ffprobe.exe"; $FFmpegName = "ffmpeg.exe"
                $FFmpegPath = Join-Path -Path $ScriptDir -ChildPath $FFmpegName 
                $FFprobePath = Join-Path -Path $ScriptDir -ChildPath $FFprobeName

                function Sanitize-Filename { param ([string]$FileName)
                    $invalidChars = '[\\/:*?"<>|∙]'; $sanitized = $FileName -replace $invalidChars, '-'
                    return $sanitized.Trim().TrimEnd('.').Replace('--','-') }

                $AudioFileFilters = @("*.m4b", "*.mp3", "*.aac", "*.m4a", "*.flac", "*.ogg", "*.wma", "*.wav", "*.webma", "*.opus")
                "======================================================"
                "Scanning for audio files in: $LibrarySourceRoot"
                $AllAudioFiles = $AudioFileFilters | ForEach-Object {
                    Get-ChildItem -LiteralPath $LibrarySourceRoot -Filter $_ -File -Recurse -ErrorAction SilentlyContinue
                } | Where-Object { $_.DirectoryName -notlike "*\Compressed_Audiobooks_*" } | Select-Object -Unique | Sort-Object FullName
                
                if (-not $AllAudioFiles) { "Warning: No supported audio files found. Exiting."; exit 0 }
                "Found $($AllAudioFiles.Count) supported audio file(s) to process."

                if (-not (Test-Path -LiteralPath $OutputRootFolder)) {
                    New-Item -ItemType Directory -Path $OutputRootFolder -Force | Out-Null
                    "Created main output directory: $OutputRootFolder"
                }

                foreach ($audioFile in $AllAudioFiles) { 
                    $filesProcessed++
                    "PROGRESS:$filesProcessed"
                    "`n======================================================"
                    "Processing: '$($audioFile.FullName)'"
                    "Probing original file..."
                    $probeArgs = @("-v", "error", "-select_streams", "a:0", "-show_entries", "stream=codec_name,bit_rate,channels", "-of", "json", $audioFile.FullName)
                    try {
                        $probeJson = & $FFprobePath $probeArgs 2>$null | Out-String -Stream
                        $probeData = $probeJson | ConvertFrom-Json
                        $audioStream = $probeData.streams | Select-Object -First 1 
                        $currentBitrate = if ($audioStream.bit_rate) { [int]$audioStream.bit_rate } else { 0 }
                        $currentChannels = [int]$audioStream.channels
                        $currentCodec = $audioStream.codec_name
                    } catch { "Error: Error probing file '$($audioFile.FullName)'. Skipping."; continue }
                    "Original Details: $($currentCodec), $($currentChannels)ch, $($currentBitrate/1000)kbps"

                    if ($currentChannels -eq 1 -and $currentBitrate -le $MonoCopyThreshold -and $currentBitrate -ne 0) {
                        "Action: File is already mono and within tolerance. Copying."
                        $RelativePath = $audioFile.FullName.Substring($LibrarySourceRoot.Length)
                        $TargetDirectory = Join-Path -Path $OutputRootFolder -ChildPath (Split-Path -Parent $RelativePath)
                        if (-not (Test-Path -LiteralPath $TargetDirectory)) { New-Item -ItemType Directory -Path $TargetDirectory -Force | Out-Null }
                        $DestinationFileCopy = Join-Path -Path $TargetDirectory -ChildPath $audioFile.Name
                        try { Copy-Item -LiteralPath $audioFile.FullName -Destination $DestinationFileCopy -Force; "Successfully copied to '$DestinationFileCopy'." } catch { "Error: Failed to copy file '$_'." }
                        continue 
                    }

                    "Action: Re-encoding to target format."
                    $RelativePath = $audioFile.FullName.Substring($LibrarySourceRoot.Length)
                    $TargetDirectory = Join-Path -Path $OutputRootFolder -ChildPath (Split-Path -Parent $RelativePath)
                    if (-not (Test-Path -LiteralPath $TargetDirectory)) { New-Item -ItemType Directory -Path $TargetDirectory -Force | Out-Null }
                    $SanitizedBaseName = Sanitize-Filename -FileName $audioFile.BaseName
                    $DestFileCompress = Join-Path -Path $TargetDirectory -ChildPath "$($SanitizedBaseName).m4b"
                    "Output to: '$DestFileCompress'"
                    
                    $ffmpegArgs = @("-i", $audioFile.FullName, "-vn", "-c:a", "aac", "-b:a", $TargetBitrate, "-ar", $TargetSampleRate,
                        "-af", "pan=mono|c0=0.5*c0+0.5*c1", "-map_metadata", "0", "-map_chapters", "0", "-movflags", "+faststart", "-y", "-v", "info", $DestFileCompress)
                    try {
                        & $FFmpegPath $ffmpegArgs 2>$null | Out-Null
                        if ($LASTEXITCODE -ne 0) { "Error: FFmpeg failed with exit code $LASTEXITCODE." }
                        else { "Successfully compressed '$($audioFile.FullName)'." }
                    } catch { "Error: An exception occurred while running FFmpeg." }
                }
                "`n======================================================"
                "All supported audio files processed."
                "======================================================="
            }
        }
        
        $script:Job = Start-Job -ScriptBlock $scriptBlock -ArgumentList $sourcePath, $outputPath, $scriptRootForJob

        $script:Timer = New-Object System.Windows.Threading.DispatcherTimer
        $script:Timer.Interval = [TimeSpan]::FromMilliseconds(250)
        $script:Timer.Tag = [PSCustomObject]@{ Job = $script:Job; TotalFiles = $totalFiles; FailsafeLog = $failsafeLogPath }

        $script:Timer.add_Tick({
            $context = $this.Tag; $currentJob = $context.Job
            if (-not $currentJob) { $this.Stop(); return }
            $outputLines = Receive-Job -Job $currentJob
            if ($outputLines) {
                $outputLines | Add-Content -LiteralPath $context.FailsafeLog
                foreach ($line in $outputLines) {
                    if ($line -like "PROGRESS:*") {
                        $progressCount = $line.Split(':')[1]
                        $ProgressBar.Value = $progressCount
                        $ProgressTextBlock.Text = "File $progressCount of $($context.TotalFiles)"
                    } else { $LogTextBox.AppendText($line + "`r`n") }
                }
                $LogTextBox.ScrollToEnd()
            }
            if ($currentJob.State -in ('Completed', 'Failed', 'Stopped')) {
                $this.Stop()
                $StatusTextBlock.Text = "Run finished: $($currentJob.State)"
                if ($currentJob.State -ne 'Stopped') {
                    $ProgressTextBlock.Text = "Completed $($context.TotalFiles) of $($context.TotalFiles) files."
                    $ProgressBar.Value = $ProgressBar.Maximum
                }
                $StartButton.IsEnabled = $true; $CancelButton.IsEnabled = $false
                $SourceBrowseButton.IsEnabled = $true; $OutputBrowseButton.IsEnabled = $true
                if (Get-Job -Id $currentJob.Id) { Remove-Job $currentJob -Force }
            } else {
                 $StatusTextBlock.Text = "Processing... ($($currentJob.State))"
            }
        })
        $script:Timer.Start()
    })

    $CancelButton.add_Click({
        if ($script:Job) {
            $StatusTextBlock.Text = "Cancelling..."
            $CancelButton.IsEnabled = $false
            Stop-Job -Job $script:Job
        }
    })

    $window.ShowDialog() | Out-Null
}
catch {
    Write-Host "An unexpected error occurred:" -ForegroundColor Red
    Write-Host ($_.Exception | Format-List | Out-String)
    Write-Host "Press Enter to exit."; Read-Host
}
finally {
    if ($global:Transcript) {
        Stop-Transcript
    }
}

# [2025-07-13 01:48:00 CEST] - AudiobookCompressor-v6.2 (Complete and Verified)