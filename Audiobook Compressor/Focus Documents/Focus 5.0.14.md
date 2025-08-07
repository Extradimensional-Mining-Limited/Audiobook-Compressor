---
Filename: Focus 5.0.14.md  
To: Axion (Strategist)  
From: Orion (Implementer)  
Date: 2025-08-07  
Version: 5.0.14  
State: Request for Guidance
---

### Subject: Sub-threshold ComboBox Alignment Issue in Advanced Panels

#### 1. Context
During the UI refinement of the Advanced panels in MainWindow.xaml, a need arose to precisely position the `MonoAdvancedCustomBitrateComboBox` and `StereoAdvancedCustomBitrateComboBox` relative to their associated "Convert to:" RadioButtons. The goal was to move these ComboBoxes 2 pixels to the left and 7 pixels up for improved visual alignment.

#### 2. Issue
Attempts to adjust the ComboBox position using the `Margin` property within the existing `WrapPanel` layout had no visible effect. This is due to the layout behavior of `WrapPanel`, which arranges its children horizontally and aligns them to the tallest element in the row, limiting the effectiveness of margin-based fine-tuning for vertical and horizontal placement.

#### 3. Investigation
- Negative or small margin values on the ComboBox do not achieve the desired offset.
- The `WrapPanel` does not provide granular control over the relative positioning of individual elements.
- The ComboBox and RadioButton remain aligned to the same baseline, regardless of margin adjustments.

#### 4. Suggested Solutions
**A. Use a Grid for Fine Control**  
Replace the `WrapPanel` containing the "Convert to:" RadioButton and ComboBox with a `Grid` with two columns. This allows independent control of each element’s alignment and margin, enabling precise placement.

**B. Use a DockPanel or StackPanel**  
A `DockPanel` or horizontal `StackPanel` offers slightly more control than a `WrapPanel`, but still lacks the pixel-level precision of a `Grid`.

**C. Avoid Canvas**  
A `Canvas` allows absolute positioning but is not recommended for resizable or responsive layouts.

#### 5. Recommendation
The most robust and maintainable solution is to use a `Grid` for the "Convert to:" RadioButton and ComboBox pair in both Mono and Stereo advanced panels. This will allow for the requested pixel-perfect alignment and future flexibility.

#### 6. Action Requested
Please advise if you approve the transition to a `Grid` layout for these controls, or if you have alternative design requirements or constraints.

---
