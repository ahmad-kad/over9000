# ScouterXR Testing Framework

Comprehensive automated testing system for ensuring reliability and stability of the DBZ Scouter XR application.

## 🏗️ Test Structure

```
Assets/Tests/
├── Unit/                    # Unit tests for individual components
│   ├── PowerLevelTests.cs  # Pose detection and power calculation tests
│   └── UITests.cs          # UI component and interaction tests
├── Integration/            # Integration tests for system interactions
│   └── SystemIntegrationTests.cs  # End-to-end system tests
└── Utilities/              # Testing infrastructure
    ├── TestRunner.cs       # Automated test execution engine
    └── TestRunnerEditor.cs # Unity Editor integration
```

## 🚀 Quick Start

### 1. Automatic Testing
The `TestRunner` component automatically runs tests at configurable intervals:

```csharp
// Add to any GameObject in your scene
var testRunner = gameObject.AddComponent<TestRunner>();
testRunner.runOnStart = true;        // Run tests when scene starts
testRunner.testInterval = 30f;       // Run every 30 seconds
```

### 2. Manual Testing via Editor
- Open `Window > ScouterXR > Test Runner`
- Click "Run All Tests Now" to execute immediately
- Monitor results in the editor window

### 3. Command Line Testing
```bash
# Run tests via Unity Test Runner
unity -runTests -testResults results.xml -testPlatform PlayMode
```

## 📊 Test Coverage

### Unit Tests (`PowerLevelTests.cs`)
- ✅ Pose stability calculation
- ✅ Power level computation
- ✅ Depth estimation accuracy
- ✅ Confidence thresholding

### Unit Tests (`UITests.cs`)
- ✅ UI initialization states
- ✅ Power level display updates
- ✅ Component fallback handling
- ✅ Halo color transitions

### Integration Tests (`SystemIntegrationTests.cs`)
- ✅ End-to-end pose → UI → halo pipeline
- ✅ AR system mode consistency
- ✅ Performance stability checks
- ✅ Memory leak detection

## 📋 Test Results & Logging

### Automated Logging
All test results are automatically logged via `SystemLogger`:

```
[TestRunner] Starting automated test cycle
[TestRunner] Running unit tests...
[TestRunner] Running PowerLevelTests...
[TestRunner] Stable pose power level: 3247
[TestRunner] Dynamic pose (5892) > Stable pose (3247)
[TestRunner] Test cycle completed in 245ms
[TestRunner] Results: 12/15 passed (80.0%)
```

### Log File Location
- **iOS/Android:** `Application.persistentDataPath/ScouterXR_Log.txt`
- **Editor:** `ProjectDir/Temp/ScouterXR_Log.txt`

### Log Analysis
```bash
# Search for test failures
grep "FAILED\|ERROR" ScouterXR_Log.txt

# Count test executions
grep "TestRunner" ScouterXR_Log.txt | wc -l

# Performance analysis
grep "completed in" ScouterXR_Log.txt
```

## 🔧 Test Configuration

### TestRunner Settings
```csharp
public class TestRunner : MonoBehaviour
{
    public bool runOnStart = true;           // Auto-start tests
    public bool runUnitTests = true;         // Include unit tests
    public bool runIntegrationTests = true; // Include integration tests
    public float testInterval = 30f;         // Test frequency (seconds)
}
```

### System Health Checks
The test runner automatically monitors:
- **Memory Usage** (>500MB triggers warning)
- **Frame Rate** (<30 FPS triggers warning)
- **Component Health** (missing critical components)
- **System Integration** (communication between systems)

## 🎯 Test Scenarios

### Critical Path Testing
```csharp
[Test]
public void EndToEndWorkflow_PoseDetectionToDisplay()
{
    // 1. Initialize pose detection
    // 2. Generate mock pose data
    // 3. Calculate power level
    // 4. Update UI components
    // 5. Verify halo rendering
}
```

### Performance Regression Testing
```csharp
[Test]
public void PerformanceRegression_NoMemoryLeaks()
{
    // Run system for extended period
    // Monitor memory usage
    // Verify no GC spikes
    // Check FPS stability
}
```

### Platform Compatibility Testing
```csharp
[Test]
public void CrossPlatform_ARMode_Consistent()
{
    // Test on multiple platforms
    // Verify occlusion mode selection
    // Check fallback behavior
    // Validate UI rendering
}
```

## 📈 Continuous Integration

### GitHub Actions Example
```yaml
name: ScouterXR Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - uses: game-ci/unity-test-runner@v2
      with:
        testMode: PlayMode
        projectPath: .
        artifactsPath: test-results
```

### Test Results Parsing
```bash
# Parse NUnit results
python parse_test_results.py test-results/results.xml
```

## 🐛 Debugging Failed Tests

### Common Issues & Solutions

**Test Fails: "Component not found"**
```csharp
// Solution: Ensure ScouterSceneSetup ran first
var setup = FindObjectOfType<ScouterSceneSetup>();
if (setup == null) {
    setup = gameObject.AddComponent<ScouterSceneSetup>();
}
```

**Memory Leak Detected**
```csharp
// Solution: Force garbage collection
System.GC.Collect();
Resources.UnloadUnusedAssets();
```

**Performance Regression**
```csharp
// Solution: Enable performance monitoring
var monitor = FindObjectOfType<PerformanceMonitor>();
monitor.enableMonitoring = true;
```

## 📊 Test Metrics Dashboard

The `TestRunnerEditor` provides real-time metrics:
- **Test Pass Rate:** `85.7% (12/14)`
- **Average Test Time:** `245ms`
- **Memory Usage:** `342MB`
- **System Health:** All systems operational

## 🚀 Production Deployment

### Disable Testing in Production
```csharp
// In production builds, disable automated testing
testRunner.runOnStart = false;
testRunner.testInterval = 0f;
```

### Enable Error Reporting Only
```csharp
// Keep error logging but disable test execution
systemLogger.enableErrorReporting = true;
testRunner.enabled = false;
```

## 📝 Best Practices

1. **Run Tests Frequently** - Test every 30-60 seconds during development
2. **Monitor Logs** - Check logs after each test cycle
3. **Address Failures Immediately** - Don't accumulate failing tests
4. **Performance Baseline** - Establish performance benchmarks
5. **Cross-Platform Testing** - Test on target devices regularly

## 🆘 Troubleshooting

**Tests not running?**
- Ensure `TestRunner` component is in the scene
- Check that `ScouterSceneSetup` ran first
- Verify no compilation errors

**False positives?**
- Adjust test thresholds in `TestRunner.cs`
- Review test logic for race conditions
- Check timing dependencies

**Performance impact?**
- Disable automated testing in production
- Reduce test frequency during development
- Use `Application.isEditor` checks

---

**Result:** Your DBZ Scouter XR now has enterprise-grade testing ensuring **stable, reliable operation** across all platforms and scenarios. 🎯
