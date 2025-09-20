using UnityEngine;

/// <summary>
/// Lightweight logger with build-aware verbosity switches.
/// In Editor/Development builds you see Info/Warning (if enabled); in Release only Errors are emitted.
/// </summary>
public static class Logger
{
    /// <summary>Master on/off switch for non-error logs.</summary>
    public static bool EnableLogs = true;

    /// <summary>Verbosity level for non-error logs.</summary>
    public enum LogLevel { Error = 0, Warning = 1, Info = 2 }
    public static LogLevel Level = LogLevel.Info;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static bool DevLoggingAllowed => EnableLogs;
#else
    private static bool DevLoggingAllowed => false; // Silence Info/Warning on release builds
#endif

    /// <summary>Info.</summary>
    public static void Log(string message, Object context = null)
    {
        if (DevLoggingAllowed && Level >= LogLevel.Info)
            Debug.Log(message, context);
    }

    /// <summary>Warning.</summary>
    public static void LogWarning(string message, Object context = null)
    {
        if (DevLoggingAllowed && Level >= LogLevel.Warning)
            Debug.LogWarning(message, context);
    }

    /// <summary>Error (always visible).</summary>
    public static void LogError(string message, Object context = null)
    {
        Debug.LogError(message, context);
    }

    // --- Optional: category overloads (sugar) ---
    public static void Log(string category, string message, Object context = null)
        => Log($"[{category}] {message}", context);

    public static void LogWarning(string category, string message, Object context = null)
        => LogWarning($"[{category}] {message}", context);

    public static void LogError(string category, string message, Object context = null)
        => LogError($"[{category}] {message}", context);
}
