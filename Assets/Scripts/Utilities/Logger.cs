using UnityEngine;

public static class Logger
{
    // Master switch for enabling/disabling logs
    public static bool EnableLogs = true;

    // Logs only appear in the Editor by default
#if UNITY_EDITOR
    private static bool IsLoggingAllowed => EnableLogs;
#else
    private static bool IsLoggingAllowed => false; // Disable in builds
#endif

    public static void Log(string message, Object context = null)
    {
        if (IsLoggingAllowed)
            Debug.Log(message, context);
    }

    public static void LogWarning(string message, Object context = null)
    {
        if (IsLoggingAllowed)
            Debug.LogWarning(message, context);
    }

    public static void LogError(string message, Object context = null)
    {
        if (IsLoggingAllowed)
            Debug.LogError(message, context);
    }
}
