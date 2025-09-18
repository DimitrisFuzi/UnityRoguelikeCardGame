using UnityEngine;

public static class Logger
{
    // Master switch (dev convenience)
    public static bool EnableLogs = true;

    // Simple levels (Error = least noisy)
    public enum LogLevel { Error = 0, Warning = 1, Info = 2 }
    public static LogLevel Level = LogLevel.Info;

    // Dev-only switch: logs (Info/Warning) εμφανίζονται σε Editor ή Development build
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static bool DevLoggingAllowed => EnableLogs;
#else
    private static bool DevLoggingAllowed => false; // Silence Info/Warning on release builds
#endif

    // INFO
    public static void Log(string message, Object context = null)
    {
        if (DevLoggingAllowed && Level >= LogLevel.Info)
            Debug.Log(message, context);
    }

    // WARNING
    public static void LogWarning(string message, Object context = null)
    {
        if (DevLoggingAllowed && Level >= LogLevel.Warning)
            Debug.LogWarning(message, context);
    }

    // ERROR: Keep errors visible in all builds
    public static void LogError(string message, Object context = null)
    {
        Debug.LogError(message, context);
    }

    // --- Optional: category overloads (non-breaking sugar) ---
    public static void Log(string category, string message, Object context = null)
        => Log($"[{category}] {message}", context);

    public static void LogWarning(string category, string message, Object context = null)
        => LogWarning($"[{category}] {message}", context);

    public static void LogError(string category, string message, Object context = null)
        => LogError($"[{category}] {message}", context);
}
