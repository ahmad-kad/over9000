using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScouterXR.Core
{
    public class SystemLogger : MonoBehaviour
    {
        [Header("Logging Configuration")]
        public bool enableFileLogging = true;
        public bool enableConsoleLogging = true;
        public LogLevel minimumLogLevel = LogLevel.Info;
        public string logFileName = "ScouterXR_Log.txt";

        [Header("Error Reporting")]
        public bool enableErrorReporting = false;
        public float errorReportCooldown = 5f;

        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }

        private string logFilePath;
        private Queue<LogEntry> logQueue = new Queue<LogEntry>();
        private float lastErrorReportTime = 0f;

        private static SystemLogger instance;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLogging();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeLogging()
        {
            // Create log file path
            logFilePath = Path.Combine(Application.persistentDataPath, logFileName);

            // Clear old log on startup
            if (File.Exists(logFilePath))
            {
                try
                {
                    File.Delete(logFilePath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not clear old log file: {e.Message}");
                }
            }

            // Log system startup
            Log(LogLevel.Info, "SystemLogger", "Scouter XR logging system initialized");
            Log(LogLevel.Info, "SystemLogger", $"Device: {SystemInfo.deviceModel}");
            Log(LogLevel.Info, "SystemLogger", $"Platform: {Application.platform}");
            Log(LogLevel.Info, "SystemLogger", $"Unity Version: {Application.unityVersion}");

            // Hook into Unity's logging system
            Application.logMessageReceived += HandleUnityLog;
        }

        void Update()
        {
            // Process queued logs (write to file on main thread)
            while (logQueue.Count > 0)
            {
                LogEntry entry = logQueue.Dequeue();
                WriteToFile(entry);
            }
        }

        void OnApplicationQuit()
        {
            Log(LogLevel.Info, "SystemLogger", "Application shutting down");
            FlushLogs();
        }

        // Public logging methods
        public static void LogInfo(string category, string message, params object[] args)
        {
            if (instance != null) instance.Log(LogLevel.Info, category, string.Format(message, args));
        }

        public static void LogWarning(string category, string message, params object[] args)
        {
            if (instance != null) instance.Log(LogLevel.Warning, category, string.Format(message, args));
        }

        public static void LogError(string category, string message, params object[] args)
        {
            if (instance != null) instance.Log(LogLevel.Error, category, string.Format(message, args));
        }

        public static void LogCritical(string category, string message, params object[] args)
        {
            if (instance != null) instance.Log(LogLevel.Critical, category, string.Format(message, args));
        }

        public static void LogDebug(string category, string message, params object[] args)
        {
            if (instance != null) instance.Log(LogLevel.Debug, category, string.Format(message, args));
        }

        private void Log(LogLevel level, string category, string message)
        {
            if (level < minimumLogLevel) return;

            LogEntry entry = new LogEntry
            {
                timestamp = DateTime.Now,
                level = level,
                category = category,
                message = message
            };

            // Console logging
            if (enableConsoleLogging)
            {
                string consoleMessage = $"[{entry.timestamp:HH:mm:ss}] [{level}] {category}: {message}";
                switch (level)
                {
                    case LogLevel.Debug:
                    case LogLevel.Info:
                        Debug.Log(consoleMessage);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(consoleMessage);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Debug.LogError(consoleMessage);
                        break;
                }
            }

            // Queue for file writing
            if (enableFileLogging)
            {
                logQueue.Enqueue(entry);
            }

            // Error reporting for critical issues
            if (enableErrorReporting && level >= LogLevel.Error && Time.time - lastErrorReportTime > errorReportCooldown)
            {
                ReportError(entry);
                lastErrorReportTime = Time.time;
            }
        }

        private void WriteToFile(LogEntry entry)
        {
            try
            {
                string logLine = $"{entry.timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.level}] {entry.category}: {entry.message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logLine);
            }
            catch (Exception e)
            {
                // Fallback to console if file writing fails
                Debug.LogError($"Failed to write to log file: {e.Message}");
            }
        }

        private void FlushLogs()
        {
            while (logQueue.Count > 0)
            {
                LogEntry entry = logQueue.Dequeue();
                WriteToFile(entry);
            }
        }

        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            // Convert Unity log types to our log levels
            LogLevel level = LogLevel.Info;
            switch (type)
            {
                case LogType.Log: level = LogLevel.Info; break;
                case LogType.Warning: level = LogLevel.Warning; break;
                case LogType.Error: level = LogLevel.Error; break;
                case LogType.Exception: level = LogLevel.Critical; break;
                case LogType.Assert: level = LogLevel.Error; break;
            }

            Log(level, "Unity", $"{logString}{(string.IsNullOrEmpty(stackTrace) ? "" : "\n" + stackTrace)}");
        }

        private void ReportError(LogEntry entry)
        {
            // In a real implementation, this would send to analytics service
            // For now, just log it with special marker
            Debug.LogError($"[ERROR REPORT] {entry.category}: {entry.message}");

            // Could integrate with services like:
            // - Firebase Crashlytics
            // - Sentry
            // - Custom analytics endpoint
        }

        // Utility methods
        public string GetLogFilePath() => logFilePath;

        public long GetLogFileSize()
        {
            try
            {
                return new FileInfo(logFilePath).Length;
            }
            catch
            {
                return 0;
            }
        }

        public void ClearLogFile()
        {
            try
            {
                File.Delete(logFilePath);
                Log(LogLevel.Info, "SystemLogger", "Log file cleared");
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, "SystemLogger", $"Failed to clear log file: {e.Message}");
            }
        }

        private struct LogEntry
        {
            public DateTime timestamp;
            public LogLevel level;
            public string category;
            public string message;
        }
    }
}
