using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class Logger
{
    [Conditional("UNITY_EDITOR")]
    public static void Log(string message)
    {
        Debug.Log($"<color=#ffff00>{message}</color>");
    }

    public static void LogError(string message)
    {
        Debug.LogError($"<color=#ffff00>{message}</color>");
    }
}