using UnityEngine;
using System.Collections.Generic;

namespace ScouterXR.Core
{
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        public bool enableMonitoring = true;
        public float updateInterval = 1.0f;
        public bool logToConsole = false;

        [Header("Performance Thresholds")]
        public float targetFPS = 60f;
        public float warningMemoryMB = 500f;
        public float criticalMemoryMB = 800f;

        // Performance metrics
        private float[] fpsHistory = new float[60];
        private int fpsIndex = 0;
        private float fpsSum = 0f;

        private float deltaTime = 0f;
        private float lastUpdateTime = 0f;

        // System info
        private string deviceInfo = "";

        void Start()
        {
            InitializeMonitoring();
        }

        void Update()
        {
            if (!enableMonitoring) return;

            UpdateFPS();
            CheckPerformance();

            if (Time.time - lastUpdateTime >= updateInterval)
            {
                LogPerformanceMetrics();
                lastUpdateTime = Time.time;
            }
        }

        private void InitializeMonitoring()
        {
            deviceInfo = $"Device: {SystemInfo.deviceModel} | OS: {SystemInfo.operatingSystem} | CPU: {SystemInfo.processorType}";

            if (logToConsole)
            {
                Debug.Log($"[PerformanceMonitor] Initialized on {deviceInfo}");
                Debug.Log($"[PerformanceMonitor] Target FPS: {targetFPS}, Memory Warning: {warningMemoryMB}MB");
            }
        }

        private void UpdateFPS()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float currentFPS = 1.0f / deltaTime;

            // Update rolling average
            fpsSum -= fpsHistory[fpsIndex];
            fpsHistory[fpsIndex] = currentFPS;
            fpsSum += currentFPS;
            fpsIndex = (fpsIndex + 1) % fpsHistory.Length;
        }

        private void CheckPerformance()
        {
            float avgFPS = fpsSum / fpsHistory.Length;

            if (avgFPS < targetFPS * 0.5f) // Below 50% of target
            {
                HandleCriticalPerformance(avgFPS);
            }
            else if (avgFPS < targetFPS * 0.8f) // Below 80% of target
            {
                HandleWarningPerformance(avgFPS);
            }

            // Memory check
            long currentMemory = System.GC.GetTotalMemory(false) / (1024 * 1024); // MB
            if (currentMemory > criticalMemoryMB)
            {
                HandleCriticalMemory(currentMemory);
            }
            else if (currentMemory > warningMemoryMB)
            {
                HandleWarningMemory(currentMemory);
            }
        }

        private void HandleCriticalPerformance(float avgFPS)
        {
            Debug.LogWarning($"[PerformanceMonitor] CRITICAL FPS: {avgFPS:F1} (Target: {targetFPS})");

            // Trigger performance fallbacks
            QualitySettings.SetQualityLevel(Mathf.Max(0, QualitySettings.GetQualityLevel() - 1));
            Application.targetFrameRate = Mathf.RoundToInt(targetFPS * 0.5f);
        }

        private void HandleWarningPerformance(float avgFPS)
        {
            Debug.Log($"[PerformanceMonitor] WARNING FPS: {avgFPS:F1} (Target: {targetFPS})");

            // Reduce quality settings
            if (QualitySettings.GetQualityLevel() > 0)
            {
                QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel() - 1);
            }
        }

        private void HandleCriticalMemory(long memoryMB)
        {
            Debug.LogError($"[PerformanceMonitor] CRITICAL MEMORY: {memoryMB}MB");

            // Force garbage collection
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        private void HandleWarningMemory(long memoryMB)
        {
            Debug.LogWarning($"[PerformanceMonitor] WARNING MEMORY: {memoryMB}MB");

            // Suggest cleanup
            System.GC.Collect();
        }

        private void LogPerformanceMetrics()
        {
            if (!logToConsole) return;

            float avgFPS = fpsSum / fpsHistory.Length;
            long memoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024);

            Debug.Log($"[PerformanceMonitor] FPS: {avgFPS:F1} | Memory: {memoryMB}MB | Platform: {Application.platform}");
        }

        // Public API
        public float GetAverageFPS() => fpsSum / fpsHistory.Length;
        public long GetMemoryUsageMB() => System.GC.GetTotalMemory(false) / (1024 * 1024);
        public string GetDeviceInfo() => deviceInfo;

        public Dictionary<string, object> GetPerformanceReport()
        {
            return new Dictionary<string, object>
            {
                ["averageFPS"] = GetAverageFPS(),
                ["memoryUsageMB"] = GetMemoryUsageMB(),
                ["deviceInfo"] = deviceInfo,
                ["qualityLevel"] = QualitySettings.GetQualityLevel(),
                ["targetFrameRate"] = Application.targetFrameRate
            };
        }
    }
}
