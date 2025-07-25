Filename: Focus 2.2.4.md  
To: Praxis  
Last Updated: 2025-07-25  
Version: 2.2.4  
State: Report  
Signed: Sir Fixalot

---

## **Subject: Alignment Issue – Contextual Panels (MainWindow.xaml)**

Praxis,

Thank you for your continued guidance. I have implemented the requested parent Grid to wrap the contextual panels and applied a left margin to match the main compression settings. However, a visual alignment issue persists. Below are the relevant XAML excerpts for your review:

**Main Compression Settings WrapPanel:**
```xml
<WrapPanel>
    <ComboBox x:Name="ChannelsComboBox" ... Margin="3" ... />
    <!-- ...other ComboBoxes... -->
</WrapPanel>
```

**Contextual Panels Parent Grid:**
```xml
<Grid Margin="3,0,0,0">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    <StackPanel x:Name="MonoModeOptionsPanel" Grid.Row="0" ...>
        <!-- ...radio buttons and override panel... -->
    </StackPanel>
    <StackPanel x:Name="StereoModeOptionsPanel" Grid.Row="1" ...>
        <!-- ...radio buttons and override panel... -->
    </StackPanel>
</Grid>
```

**Contextual Panel Example:**
```xml
<StackPanel x:Name="MonoModeOptionsPanel" ...>
    <WrapPanel Margin="3">
        <RadioButton x:Name="MonoCopyStereoRadio" ... Margin="0,0,10,0"/>
        <!-- ...other radio buttons... -->
    </WrapPanel>
    <WrapPanel x:Name="AdvancedStereoOverridePanel" Margin="3" ...>
        <ComboBox x:Name="AdvancedStereoChannelsComboBox" ... Margin="3" ... />
        <!-- ...other ComboBoxes... -->
    </WrapPanel>
</StackPanel>
```

Despite these changes, the contextual panels do not appear perfectly left-aligned with the main settings ComboBoxes. I am ready to make further adjustments based on your feedback or any additional layout guidance you can provide.

Best regards,  
Sir Fixalot
