Filename: ChangelogExperimental.md  
Last Updated: 2025-08-09 16:00 CEST
Version: 1.2.K  
State: Experimental  
Signed: Vanguard

# **Experimental Changelog**

This file contains a detailed, granular log of all changes made on the experimental branch.

## **\[Unreleased\]**

### **Added**

* 1.2.K: feat: Focus 18.2.0 Final Modularization Phases 3 & 4 Implementation - Complete MainViewModel transformation with RadioButtonStateService and PathManagementService achieving 48-52% size reduction and 9-service architecture excellence.
* 1.2.K: feat: Phase 3 IRadioButtonStateService Implementation per Focus 18.2.0 - Created comprehensive radio button state management service extracting ~150 lines of complex coordination logic from MainViewModel with atomic state updates and event-driven coordination.
* 1.2.K: feat: Phase 4 IPathManagementService Implementation per Focus 18.2.0 - Created complete path operations service extracting ~100 lines of path management logic from MainViewModel with async operations, validation integration, and collision detection.
* 1.2.K: test: Final Modularization Test Suite per Focus 18.2.0 - Created comprehensive unit test suites for RadioButtonStateService (25+ tests) and PathManagementService (30+ tests) achieving 100% method coverage with extensive scenario testing.
* 1.2.K: feat: Complete Service Ecosystem Integration per Focus 18.2.0 - Updated MainViewModel to streamlined orchestration layer delegating all specialized concerns to 9 focused services with event-driven coordination and professional resource management.
* 1.2.K: feat: Enhanced Dependency Injection Container per Focus 18.2.0 - Registered IRadioButtonStateService and IPathManagementService completing the service-oriented architecture transformation with singleton lifecycle management.
* 1.2.J: feat: Phase 2 ISettingsBindingService Implementation per Focus 17.2.0 \- Created comprehensive settings binding service with context-aware validation, extracting ~120 lines of complex validation logic from MainViewModel into focused, testable service.  
* 1.2.J: feat: Phase 2 ISettingsBindingService Implementation per Focus 17.2.0 \- Implemented ValidationService integration with event-based validation feedback system for bitrate, sample rate, threshold, and cross-field business rule validation.  
* 1.2.J: feat: Phase 2 ISettingsBindingService Implementation per Focus 17.2.0 \- Enhanced MainViewModel with SettingsBindingService delegation, maintaining XAML binding compatibility while centralizing all settings validation logic in focused service.  
* 1.2.J: feat: Phase 2 ISettingsBindingService Implementation per Focus 17.2.0 \- Added encoding type coordination logic with CBR/ABR pass mode management, preserving complex UI business rules in service-oriented architecture.  
* 1.2.J: test: Phase 2 ISettingsBindingService Implementation per Focus 17.2.0 \- Created comprehensive unit test suite for SettingsBindingService with 23 test methods covering context-aware validation, property binding, mode switching, and cross-field business logic scenarios.  
* 1.2.J: fix: Advanced Panel Visibility Bug Resolution per Focus 17.2.0 \- Implemented multi-part comprehensive bug fix including leftover code cleanup, atomic settings updates, enhanced validation, and debug logging to resolve persistent advanced panel visibility issues.  
* 1.2.I: fix: Comprehensive polishing pass implementation per Focus 16.6.0 \- Corrected versioning discrepancy by standardizing all files to version 1.2.I following MAJOR.MINOR.LETTER format for experimental cycles.  
* 1.2.I: fix: Advanced panel visibility bug per Focus 16.6.0 \- Added Initialize method to IPanelVisibilityService and PanelVisibilityService to properly initialize service with loaded settings state, preventing advanced panel display issues when switching modes.  
* 1.2.I: refactor: Test project organization per Focus 16.6.0 \- Updated test project namespace to AudiobookCompressor.Tests.Services following standardized \[ProjectName\].Tests.\[Purpose\] naming convention for better organization and future extensibility.  
* 1.2.I: docs: Enhanced Testing-Architecture.md per Focus 16.6.0 \- Added comprehensive section documenting test execution procedures for AudiobookCompressor.Tests.Services suite including Visual Studio and command-line execution methods, test structure, and expected results.  
* 1.2.I: test: Enhanced PanelVisibilityService test coverage per Focus 16.6.0 \- Added 4 comprehensive unit tests for new Initialize method covering parameter validation, state setting, advanced panel scenarios, and property change notifications.  
* 1.2.I: feat: Phase 1 Modularization Implementation per Focus 16.2.0 \- Created IUIStateService and UIStateService for centralized UI state management including status, progress, and logging extracted from MainViewModel.  
* 1.2.I: feat: Phase 1 Modularization Implementation per Focus 16.2.0 \- Created IPanelVisibilityService and PanelVisibilityService for centralized panel visibility management based on mode and action selection extracted from MainViewModel.  
* 1.2.I: feat: Phase 1 Modularization Implementation per Focus 16.2.0 \- Updated MainViewModel to delegate UI state and panel visibility logic to specialized services while maintaining XAML binding compatibility through property delegation and event forwarding.  
* 1.2.I: feat: Phase 1 Modularization Implementation per Focus 16.2.0 \- Updated App.xaml.cs dependency injection container to register new IUIStateService and IPanelVisibilityService as singletons for proper service lifecycle management.  
* 1.2.I: test: Phase 1 Modularization Implementation per Focus 16.2.0 \- Created comprehensive unit test suites for UIStateService and PanelVisibilityService with 100% method coverage and extensive scenario testing including property change notifications and edge cases.  
* 1.2.H: fix: Resolved multiple window startup issue per Focus 15.0.0 by removing StartupUri="MainWindow.xaml" attribute from App.xaml, ensuring App.xaml.cs dependency injection container is sole authority for MainWindow creation with proper MainViewModel DataContext injection.  
* 1.2.H: feat: Implemented settings persistence recovery per Focus 15.4.0 by adding OnApplicationExit() method to MainViewModel for graceful shutdown with automatic settings save, enhancing App.xaml.cs OnExit() to call ViewModel cleanup, and changing MainViewModel to Singleton registration for application exit access.  
* 1.2.G: feat: Complete MVVM Data Binding Implementation \- Executed comprehensive XAML data binding conversion per Focus 14.2.0, achieving complete transformation from event-driven code-behind to professional MVVM architecture.  
* 1.2.G: feat: MainViewModel Enhancement \- Added comprehensive binding-specific properties including SelectedChannel, SelectedBitrate with validation, radio button state properties, panel visibility management, and advanced settings integration per Focus 14.1.0 Phase 1\.  
* 1.2.G: feat: Value Converters Infrastructure \- Created complete converter ecosystem including BooleanToVisibilityConverter, RadioButtonToStringConverter, BitrateValidationConverter, and updated ProgressWidthConverter in dedicated Converters namespace per Focus 14.1.0 Phase 2\.  
* 1.2.G: feat: XAML Data Binding Conversion \- Converted all UI controls from event handlers to declarative data binding including path TextBoxes, browse buttons, settings ComboBoxes, radio button groups, advanced panels, status bar, and action buttons per Focus 14.1.0 Phases 3-5.  
* 1.2.G: feat: Code-Behind Reduction \- Achieved 97.5% reduction in MainWindow.xaml.cs from 1,000+ lines to 25 lines of essential view-specific logic, eliminating 50+ event handlers and centralizing all UI logic in MainViewModel per Focus 14.1.0 Phase 6\.  
* 1.2.G: feat: Advanced Panel Data Binding \- Implemented complex binding scenarios for advanced settings panels including sub-threshold behavior controls with RadioButtonToStringConverter and conditional ComboBox enabling based on radio button selections.  
* 1.2.G: feat: Validation Integration \- Seamlessly integrated ValidationService with ViewModel property setters providing real-time validation with DialogService error display for bitrate, sample rate, and threshold inputs.  
* 1.2.G: feat: Settings Context Management \- Implemented intelligent settings context switching with proxy properties that delegate to correct settings context (main vs advanced) based on current mode and radio button selections.  
* 1.2.G: feat: LogContent Property \- Added LogContent property to MainViewModel for binding-based log display, replacing manual TextBox manipulation with automatic content updates through property binding.  
* 1.2.G: docs: Focus 14.3.0 MVVM Data Binding Implementation Report \- Comprehensive implementation report documenting complete success of Focus 14.2.0 execution, achieving architectural excellence with professional MVVM standards, zero functional regression, and transformational code quality improvements.  
* 1.2.F: docs: Created Focus 13.1.0 MVVM Refactoring Proposal in response to Focus 13.0.0 directive, providing comprehensive analysis of current monolithic MainWindow architecture and detailed phased plan for implementing professional MVVM pattern with dependency injection, service-oriented architecture, and comprehensive unit testing capabilities.  
* 1.2.F: feat: Phase 1 MVVM Implementation \- Created service interfaces (ISettingsService, IAudioService, IDialogService, IValidationService) abstracting MainWindow dependencies for dependency injection per Focus 13.1.0.  
* 1.2.F: feat: Phase 1 MVVM Implementation \- Created MainViewModel class as core of MVVM architecture, centralizing all UI logic, state management, and commands from MainWindow.xaml.cs with complete service-oriented design.  
* 1.2.F: feat: Phase 1 MVVM Implementation \- Created RelayCommand implementation enabling replacement of event handlers with testable commands in ViewModels per MVVM pattern.  
* 1.2.F: feat: Phase 2 MVVM Implementation \- Created concrete service implementations (SettingsService, AudioService, DialogService, ValidationService) migrating all logic from MainWindow.xaml.cs per Focus 13.1.0.  
* 1.2.F: feat: Phase 2 MVVM Implementation \- SettingsService implements complete XML persistence with migration support, path management, and validation migrated from MainWindow.  
* 1.2.F: feat: Phase 2 MVVM Implementation \- AudioService wraps AudioProcessor with proper event forwarding and cancellation management for MVVM architecture.  
* 1.2.F: feat: Phase 2 MVVM Implementation \- DialogService abstracts all dialog operations (MessageBox, folder browser) enabling testable UI interactions.  
* 1.2.F: feat: Phase 2 MVVM Implementation \- ValidationService centralizes all validation logic with comprehensive bitrate, sample rate, path, and settings consistency checking.  
* 1.2.F: feat: Phase 3 MVVM Implementation \- Configured dependency injection container in App.xaml.cs with Microsoft.Extensions.DependencyInjection for service-oriented architecture per Focus 13.1.0.  
* 1.2.F: docs: Focus 13.3.0 MVVM Implementation Report \- Comprehensive report documenting successful completion of foundational MVVM architectural refactor per Focus 13.2.0 directive, achieving transformational service-oriented architecture with professional dependency injection, complete testability, and zero-risk backward compatibility.  
* 1.2.E: test: Created AudiobookCompressor.Tests project with xUnit and Moq targeting .NET 8.0-windows per Focus 11.1.0/11.2.0.  
* 1.2.E: test: Introduced IProcessRunner and IFileSystem abstraction interfaces enabling dependency injection and comprehensive mocking for AudioProcessor testing.  
* 1.2.E: test: Implemented AudioProcessingDecider static class for pure logic testing of all processing decision paths (copy/convert/advanced/sub-threshold/Defer to Rockit) without file/process side effects.  
* 1.2.E: test: Created comprehensive xUnit test suite with AudioProcessingDeciderTests covering all Focus 11.1.0 scenarios, including Defer to Rockit, dynamic maxrate, and advanced panel logic.  
* 1.2.E: feat: Refactored AudioProcessor constructor to support dependency injection of IProcessRunner and IFileSystem with fallback to default implementations for backward compatibility.  
* 1.2.E: feat: Exposed DetailedFileInfo class as public to enable unit testing accessibility while maintaining internal logic separation.  
* 1.2.E: docs: Created comprehensive Testing-Architecture.md documentation per Focus 11.4.0, providing architectural guide, usage instructions, and maintenance procedures for the automated test suite.  
* 1.2.D: feat: Advanced Panel Sub-threshold Behavior \- Added Copy/Defer to Rockit/Convert to radio button controls in both Mono and Stereo advanced panels per Focus 5.0.4.  
* 1.2.D: feat: "Defer to Rockit" Quality Logic \- Implemented intelligent quality-preserving VBR processing with dynamic maxrate caps for channel reduction/expansion scenarios.  
* 1.2.D: feat: Custom Bitrate Conversion \- Added support for "Convert to:" option with user-specified target bitrate for sub-threshold files.  
* 1.2.D: feat: ProcessWithAdvancedLogic method in AudioProcessor for complete advanced panel decision tree including sub-threshold behavior routing.  
* 1.2.D: feat: VBR Command Generation \- Added BuildFFmpegVBRCommand method with sophisticated maxrate and buffer size calculation for quality preservation.  
* 1.2.B: feat: Created ProcessingContext class to bridge UI hierarchical settings with AudioProcessor logic per Focus 5.0.0 directive.  
* 1.2.B: feat: Implemented complete Focus 5.0.0 contextual file handling decision tree in AudioProcessor with support for all 6 user scenarios.  
* 1.2.B: feat: Added ProcessAsException method for handling mismatched file types (stereo files in mono mode, mono files in stereo mode).  
* 1.2.B: feat: Implemented HandleUpmixLogic method for mono-to-stereo conversion with threshold-based quality decisions.  
* 1.2.B: feat: Added contextual settings integration where radio button states and advanced overrides directly influence processing behavior.
* 1.2.J: feat: Focus 17.6.0 Final Polishing Pass per cycle conclusion - Comprehensive gremlin hunt with definitive fixes for ComboBox focus loss, default radio button logic, advanced panel visibility, and settings summary live updates.
* 1.2.J: fix: Focus 17.6.0 Bug #23 ComboBox Focus Loss Fix - Added UpdateSourceTrigger=LostFocus to all editable ComboBoxes in main settings and advanced panels to ensure user-entered values commit to settings model when control loses focus.
* 1.2.J: fix: Focus 17.6.0 Bug #37 Default Radio Button Logic Review - Explicitly corrected ApplicationSettings initializers to ensure "Convert" default for Mono mode and "Copy" default for Stereo mode with proper bold styling applied to correct defaults.
* 1.2.J: fix: Focus 17.6.0 Gremlin #36 Settings Summary Live Updates - Added property change subscription to MonoAdvancedSettings and StereoAdvancedSettings to trigger SettingsSummary updates when advanced panel radio buttons or settings change.
* 1.2.J: feat: Focus 17.4.0 Polishing Pass per cycle conclusion - Implemented comprehensive UI/UX polish including settings summary refactor, cancel confirmation dialog, bold radio button styling, and Stereo mode default fix