Filename: Testing-Architecture.md  
Last Updated: 2025-08-07 12:43 CEST  
Version: 1.2.E  
State: Experimental  
Signed: Vanguard

Synopsis:
Comprehensive architectural guide for the AudiobookCompressor.Tests project, documenting test infrastructure, usage patterns, and maintenance procedures for long-term project sustainability.

---

# **Testing Architecture Guide**

## **1.0 Overview**

The Audiobook Compressor project features a comprehensive automated test suite that provides robust regression protection for the complex audio processing logic. The test architecture is designed around clean separation of concerns, enabling fast, reliable unit testing without external dependencies.

### **1.1 Test Project Structure**
- **Project**: `AudiobookCompressor.Tests`
- **Framework**: xUnit with Moq for mocking
- **Target**: .NET 8.0-windows (matches main project)
- **Test Coverage**: All core processing logic, advanced behaviors, and "Defer to Rockit" scenarios

---

## **2.0 Running the Tests**

### **2.1 Visual Studio Test Explorer**
1. **Build the solution** (Ctrl+Shift+B) to ensure all projects are compiled
2. **Open Test Explorer** (Test → Test Explorer or Ctrl+E, T)
3. **Run all tests** by clicking "Run All Tests" button or pressing Ctrl+R, A
4. **View results** in the Test Explorer window with detailed pass/fail status

### **2.2 Command Line (dotnet test)**

**Run all tests:**
```bash
dotnet test "Audiobook Compressor.Tests/AudiobookCompressor.Tests.csproj"
```

**Run tests with verbose output:**
```bash
dotnet test "Audiobook Compressor.Tests/AudiobookCompressor.Tests.csproj" --logger "console;verbosity=detailed"
```

**Run tests with coverage (if coverage tools are installed):**
```bash
dotnet test "Audiobook Compressor.Tests/AudiobookCompressor.Tests.csproj" --collect:"XPlat Code Coverage"
```

### **2.3 Expected Results**
- **Total Tests**: 11 test cases covering all processing scenarios
- **Execution Time**: < 1 second (pure logic tests without I/O)
- **Success Criteria**: All tests should pass consistently

---

## **3.0 Architectural Overview**

The test architecture follows a **pure logic testing** approach that isolates business logic from external dependencies, enabling fast and reliable unit tests.

### **3.1 Core Components**

#### **3.1.1 AudioProcessingDecider (Pure Logic Engine)**
**Location**: `Audiobook Compressor\Services\AudioProcessingDecider.cs`

**Purpose**: Extracts all audio processing decision logic into a pure function that can be tested without side effects.

**Key Features**:
- **Input**: Mode (Mono/Stereo), Action (Copy/Convert/Advanced), Settings, File Information
- **Output**: ProcessingDecision with ActionType, Reason, and parameters
- **No Side Effects**: No file I/O, no process execution, no external dependencies
- **Complete Coverage**: All processing paths from Focus 5.0.0, 5.0.4, and "Defer to Rockit" logic

**Usage Pattern**:
```csharp
var settings = new CompressionSettings { ConversionThreshold = "64k", SubThresholdAction = "DeferToRockit" };
var fileInfo = new DetailedFileInfo { Bitrate = 48000, Channels = 1, Codec = "aac" };
var result = AudioProcessingDecider.Decide("Mono", "Advanced", settings, fileInfo);
Assert.Equal(ProcessingActionType.DeferToRockit_Copy, result.ActionType);
```

#### **3.1.2 Abstraction Interfaces**

**IProcessRunner Interface** (`Audiobook Compressor\Services\IProcessRunner.cs`):
- **Purpose**: Abstracts external process execution (FFmpeg, FFprobe)
- **Production**: DefaultProcessRunner uses System.Diagnostics.Process
- **Testing**: Mocked to simulate process behavior without actual execution

**IFileSystem Interface** (`Audiobook Compressor\Services\IFileSystem.cs`):
- **Purpose**: Abstracts file system operations (copy, exists, directory creation)
- **Production**: DefaultFileSystem uses System.IO
- **Testing**: Mocked to simulate file operations without actual I/O

### **3.2 Dependency Injection Integration**

The `AudioProcessor` class has been refactored to support dependency injection:

```csharp
public AudioProcessor(CancellationToken cancellationToken = default, 
                     IProcessRunner? processRunner = null, 
                     IFileSystem? fileSystem = null)
{
    _processRunner = processRunner ?? new DefaultProcessRunner();
    _fileSystem = fileSystem ?? new DefaultFileSystem();
}
```

**Benefits**:
- **Production**: Uses default implementations for normal operation
- **Testing**: Injects mocks for isolated unit testing
- **Backward Compatibility**: Existing code continues to work unchanged

---

## **4.0 Test Suite Structure**

### **4.1 Test Categories**

#### **4.1.1 Core Processing Logic (8 tests)**
- **Mono file in Mono mode**: Copy vs Convert based on threshold
- **Stereo file in Mono mode**: Exception path handling  
- **Mono file in Stereo mode**: Exception path with upmix logic
- **Stereo file in Stereo mode**: Standard processing

#### **4.1.2 Advanced Panel Logic (3 tests)**
- **DeferToRockit Same Channels**: Zero-loss file copying
- **DeferToRockit Downmix**: VBR downmix with original bitrate maxrate
- **DeferToRockit Upmix**: VBR upmix with estimated × 1.10 maxrate
- **ConvertTo Custom**: Custom bitrate conversion

### **4.2 Test Data Patterns**

**CompressionSettings Test Data**:
```csharp
var settings = new CompressionSettings 
{ 
    ConversionThreshold = "64k",
    SubThresholdAction = "DeferToRockit", 
    ChannelMode = "Mono",
    CustomTargetBitrate = "96k"
};
```

**DetailedFileInfo Test Data**:
```csharp
var fileInfo = new DetailedFileInfo 
{ 
    Bitrate = 48000, 
    Channels = 1, 
    Codec = "aac" 
};
```

---

## **5.0 Adding New Tests**

### **5.1 Test Case Development Process**

#### **Step 1: Identify the Scenario**
- Determine the processing scenario you want to test
- Identify the input parameters (mode, action, settings, file info)
- Define the expected output (action type, parameters, reason)

#### **Step 2: Create Test Method**
Follow the naming convention: `[FileType]_[Mode]_[Action]_[ExpectedBehavior]`

```csharp
[Fact]
public void MonoFile_MonoMode_Advanced_ConvertToCustom_UsesCustomBitrate()
{
    // Arrange: Set up test data
    var settings = new CompressionSettings 
    { 
        ConversionThreshold = "64k",
        SubThresholdAction = "ConvertTo",
        CustomTargetBitrate = "128k"
    };
    var fileInfo = new DetailedFileInfo { Bitrate = 48000, Channels = 1, Codec = "aac" };
    
    // Act: Execute the logic
    var result = AudioProcessingDecider.Decide("Mono", "Advanced", settings, fileInfo);
    
    // Assert: Verify expected behavior
    Assert.Equal(ProcessingActionType.ConvertToCustom, result.ActionType);
    Assert.Equal("128k", result.TargetBitrate);
    Assert.Equal("Advanced: Sub-threshold ConvertTo", result.Reason);
}
```

#### **Step 3: Test Edge Cases**
Consider boundary conditions and error scenarios:
- Invalid threshold values
- Null or empty settings
- Unusual bitrate/channel combinations

### **5.2 Testing New Features**

#### **5.2.1 Extending AudioProcessingDecider**

When adding new processing logic:

1. **Update ProcessingActionType enum** with new action types
2. **Extend ProcessingDecision class** if new parameters are needed
3. **Modify AudioProcessingDecider.Decide()** method logic
4. **Add comprehensive test cases** for all new paths

#### **5.2.2 Testing with Mocks (Future AudioProcessor Tests)**

For integration tests that need to verify actual AudioProcessor behavior:

```csharp
[Fact]
public async Task ProcessAudioFileAsync_WithMockedDependencies_CallsCorrectMethods()
{
    // Arrange
    var mockProcessRunner = new Mock<IProcessRunner>();
    var mockFileSystem = new Mock<IFileSystem>();
    var processor = new AudioProcessor(CancellationToken.None, mockProcessRunner.Object, mockFileSystem.Object);
    
    // Configure mocks
    mockFileSystem.Setup(fs => fs.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
    
    // Act & Assert
    // Test actual AudioProcessor integration behavior
}
```

### **5.3 Test Maintenance Guidelines**

#### **5.3.1 Keep Tests Independent**
- Each test should be completely self-contained
- No dependencies between test methods
- Use fresh test data for each test

#### **5.3.2 Clear Assertions**
- Use descriptive assertion messages
- Test one concept per test method
- Verify both positive and negative cases

#### **5.3.3 Update Tests with Logic Changes**
- When modifying processing logic, update corresponding tests immediately
- Add new tests for new decision paths
- Remove obsolete tests for deprecated logic

---

## **6.0 Troubleshooting**

### **6.1 Common Issues**

#### **Build Errors**
- **Framework Mismatch**: Ensure test project targets same framework as main project
- **Missing References**: Verify project reference to main application
- **Package Conflicts**: Check NuGet package versions for compatibility

#### **Test Failures**
- **Logic Changes**: Update test expectations when processing logic changes
- **Data Issues**: Verify test data matches expected input format
- **Assertion Errors**: Check that expected values match actual logic behavior

#### **Performance Issues**
- **Slow Tests**: Pure logic tests should complete in milliseconds
- **Memory Usage**: Avoid creating large test data objects unnecessarily
- **Parallel Execution**: xUnit runs tests in parallel by default

### **6.2 Debugging Tests**

#### **Visual Studio Debugging**
1. Set breakpoints in test methods
2. Right-click test → Debug Test
3. Step through test execution to diagnose issues

#### **Test Output Analysis**
Use xUnit output for debugging:
```csharp
[Fact]
public void TestMethod()
{
    var result = AudioProcessingDecider.Decide(...);
    _output.WriteLine($"Result: {result.ActionType}, Reason: {result.Reason}");
    // Assertions...
}
```

---

## **7.0 Future Enhancements**

### **7.1 Integration Tests**
- Add tests that verify AudioProcessor end-to-end behavior
- Mock external dependencies (FFmpeg, file system)
- Test actual processing pipeline integration

### **7.2 Performance Tests**
- Add benchmarks for processing logic performance
- Verify processing time remains acceptable
- Monitor memory usage patterns

### **7.3 Property-Based Testing**
- Use tools like FsCheck to generate random test inputs
- Verify processing logic properties hold across input ranges
- Discover edge cases through automated exploration

---

## **8.0 Conclusion**

The AudiobookCompressor.Tests project provides a robust foundation for maintaining code quality and preventing regressions. The pure logic testing approach enables fast, reliable tests that can be run frequently during development.

**Key Benefits**:
- **Regression Protection**: Immediate detection of logic changes
- **Development Confidence**: Safe refactoring with comprehensive coverage
- **Documentation Value**: Tests serve as executable specifications
- **Team Productivity**: Fast feedback loop for development changes

**Maintenance Principles**:
- Keep tests simple and focused
- Maintain test-to-logic correspondence
- Update tests immediately when logic changes
- Add new tests for all new features

This testing architecture will continue to serve the project well as it evolves, providing a solid foundation for confident development and maintenance of the audio processing logic.

---

**For questions or assistance with the testing infrastructure, refer to this guide or consult the implementation team.**