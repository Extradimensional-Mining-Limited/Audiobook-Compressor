Filename: Focus 11.1.0.md  
To: Axion (Strategist)  
From: Vanguard (Consultant)  
Last Updated: 2025-08-07 11:00 CEST  
Version: 11.1.0  
State: Proposal  
Signed: Vanguard

---

### **Subject: Updated Proposal for Automated Test Suite (Post-Refactor, Focus 11.0.0)**

Dear Axion,

In response to your Focus 11.0.0 directive, I have conducted a fresh analysis of the current AudioProcessor and Settings logic, with special attention to the "Defer to Rockit" feature, dynamic maxrate logic, and advanced panel UI controls. Below is the updated proposal for the automated unit test suite.

---

## **1.0 Methodology**

- **Framework:** xUnit (preferred for .NET 8, but MSTest or NUnit are also viable)
- **Isolation:** Use dependency injection and interface abstraction for process execution (FFmpeg/FFprobe), file I/O, and settings access. Mock these in tests.
- **Test Coverage:** Focus on all logical branches in AudioProcessor, including normal, exception, and advanced/sub-threshold paths.
- **Test Data:** Use in-memory or temp file stubs for audio file metadata; do not require real audio files for logic tests.

---

## **2.0 Refactoring Plan**

- **Process Abstraction:** Introduce interfaces (e.g., IProcessRunner) for external process execution.
- **File I/O Abstraction:** Abstract file system operations (copy, exists, directory creation) for mocking.
- **Settings Injection:** Allow injection of settings/context for deterministic test setup.
- **Event Testing:** Expose events for progress and completion; allow test hooks.
- **No breaking changes** to public API; all refactoring is additive and reversible.

---

## **3.0 Test Case Specification (Expanded)**

### **3.1 Core Processing Paths**
| # | Scenario | Mode | File Type | Action | Expected Behavior |
|---|----------|------|-----------|--------|------------------|
| 1 | Mono file, Mono mode | Mono | Mono | Copy | File copied if below threshold |
| 2 | Mono file, Mono mode | Mono | Mono | Convert | File converted if above threshold |
| 3 | Stereo file, Mono mode | Mono | Stereo | Copy | File copied (exception path) |
| 4 | Stereo file, Mono mode | Mono | Stereo | Convert | File converted to mono |
| 5 | Mono file, Stereo mode | Stereo | Mono | Copy | File copied (exception path) |
| 6 | Mono file, Stereo mode | Stereo | Mono | Convert | File upmixed to stereo |
| 7 | Stereo file, Stereo mode | Stereo | Stereo | Copy | File copied if below threshold |
| 8 | Stereo file, Stereo mode | Stereo | Stereo | Convert | File converted if above threshold |

### **3.2 Advanced Panel & Sub-Threshold Logic**
| # | Scenario | Sub-Threshold Action | File Bitrate | Channels | Expected Behavior |
|---|----------|---------------------|--------------|----------|------------------|
| 9 | Advanced, Copy | Copy | Below threshold | Any | File copied |
| 10 | Advanced, Defer to Rockit (same channels) | DeferToRockit | Below threshold | File channels = target | File copied (zero generational loss) |
| 11 | Advanced, Defer to Rockit (downmix) | DeferToRockit | Below threshold | File channels > target | VBR downmix, maxrate = original bitrate |
| 12 | Advanced, Defer to Rockit (upmix) | DeferToRockit | Below threshold | File channels < target | VBR upmix, maxrate = est. × 1.10 |
| 13 | Advanced, ConvertTo | ConvertTo | Below threshold | Any | File converted to custom bitrate |
| 14 | Advanced, Above threshold | Any | Above threshold | Any | File converted using advanced settings |

### **3.3 Dynamic Maxrate Cap Logic**
| # | Scenario | File Channels | Target Channels | File Bitrate | Expected Maxrate |
|---|----------|---------------|----------------|--------------|------------------|
| 15 | Defer to Rockit, downmix | 2 | 1 | 96k | 96k |
| 16 | Defer to Rockit, upmix | 1 | 2 | 48k | 105.6k (48k × 2 × 1.10) |
| 17 | Defer to Rockit, same | 2 | 2 | 128k | Copy (no conversion) |

### **3.4 UI/Settings Integration**
| # | Scenario | UI Control | Setting | Expected Effect |
|---|----------|------------|---------|-----------------|
| 18 | MonoAdvancedConvertRadio checked | SubThresholdAction | "ConvertTo" | Custom bitrate used |
| 19 | MonoAdvancedDeferRadio checked | SubThresholdAction | "DeferToRockit" | Rockit logic used |
| 20 | MonoAdvancedCopyRadio checked | SubThresholdAction | "Copy" | File copied |
| 21 | Custom bitrate entered | CustomTargetBitrate | "64k" | Used for conversion |
| 22 | Threshold changed | ConversionThreshold | "96k" | Affects copy/convert decision |

### **3.5 Error and Edge Cases**
| # | Scenario | Condition | Expected Behavior |
|---|----------|-----------|------------------|
| 23 | Invalid bitrate string | "abc" | Default/fallback used, warning triggered |
| 24 | FFmpeg/FFprobe failure | Process error | FileProcessed event with success=false |
| 25 | File I/O error | Copy/convert | FileProcessed event with success=false |

---

## **4.0 Reversibility and Documentation Plan**
- All refactoring will be documented inline and in a dedicated section of the ChangelogExperimental.md.
- New interfaces and abstractions will be clearly marked and can be reverted by removing test hooks and mocks.
- Test suite will be documented with usage instructions and coverage matrix.

---

## **5.0 New Dependencies**
- xUnit (or MSTest/NUnit)
- Moq (for mocking interfaces)
- System.IO.Abstractions (optional, for file I/O abstraction)

---

## **6.0 Conclusion**

This proposal provides a comprehensive, up-to-date plan for automated testing of the Audiobook Compressor's advanced logic, including all new "Defer to Rockit" and sub-threshold behaviors. Upon approval, I will proceed with implementation as directed.

Best regards,  
Vanguard
