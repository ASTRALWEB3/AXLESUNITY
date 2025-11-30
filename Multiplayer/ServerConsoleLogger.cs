using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices; // Required for Windows Console Functions
using System.IO; // Required for FileStream

/// <summary>
/// Intercepts Unity Debug Logs and prints them to the Windows Console using ANSI Colors.
/// Bypasses Unity's standard log redirection to ensure colored logs appear on screen 
/// even when standard logs are hidden in a file.
/// </summary>
public class ServerConsoleLogger : MonoBehaviour
{
    // --- WIN32 API CALLS ---
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    private const uint GENERIC_WRITE = 0x40000000;
    private const uint FILE_SHARE_WRITE = 0x00000002;
    private const uint OPEN_EXISTING = 3;
    // -----------------------

    // ANSI Color Codes
    private const string RESET = "\u001b[0m";
    private const string RED = "\u001b[31m";
    private const string GREEN = "\u001b[32m";
    private const string YELLOW = "\u001b[33m";
    private const string CYAN = "\u001b[36m";
    private const string MAGENTA = "\u001b[35m";
    private const string GREY = "\u001b[37m";

    void Awake()
    {
        if (Application.isBatchMode)
        {
            // 1. Force Console.WriteLine to write to the Screen (CONOUT$)
            // even if Unity has redirected standard output to a text file.
            RecoverConsoleOutput();

            // 2. Subscribe to logs
            Application.logMessageReceived += HandleLog;
            Console.WriteLine($"{MAGENTA}--- ASTRAL SERVER LOGGER INITIALIZED ---{RESET}");
        }
    }

    void OnDestroy()
    {
        if (Application.isBatchMode)
        {
            Application.logMessageReceived -= HandleLog;
        }
    }

    private void RecoverConsoleOutput()
    {
        // Only do this on Windows
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            // Open the "Active Console Screen Buffer" directly
            IntPtr stdHandle = CreateFile("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if (stdHandle != new IntPtr(-1))
            {
                // Create a stream writer that writes to this handle
                Microsoft.Win32.SafeHandles.SafeFileHandle safeHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(stdHandle, true);
                FileStream fileStream = new FileStream(safeHandle, FileAccess.Write);
                StreamWriter standardOutput = new StreamWriter(fileStream, System.Text.Encoding.ASCII);
                standardOutput.AutoFlush = true;

                // Tell C# "Use this stream for Console.WriteLine"
                Console.SetOut(standardOutput);
            }
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 1. Clean up the message
        string cleanMessage = Regex.Replace(logString, "<.*?>", string.Empty);

        // 2. Create a timestamp
        string timestamp = DateTime.UtcNow.ToString("HH:mm:ss");

        // 3. Format based on Log Type
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                Console.WriteLine($"{GREY}[{timestamp}] {RED}[ERR] {cleanMessage}{RESET}");
                // Only print stack trace if it's not a harmless warning
                if (!string.IsNullOrEmpty(stackTrace))
                    Console.WriteLine($"{RED}{stackTrace}{RESET}");
                break;

            case LogType.Warning:
                Console.WriteLine($"{GREY}[{timestamp}] {YELLOW}[WARN] {cleanMessage}{RESET}");
                break;

            case LogType.Log:
                if (cleanMessage.Contains("[Fusion]"))
                {
                    Console.WriteLine($"{GREY}[{timestamp}] {CYAN}{cleanMessage}{RESET}");
                }
                else if (cleanMessage.Contains("Joined") || cleanMessage.Contains("Left"))
                {
                    Console.WriteLine($"{GREY}[{timestamp}] {GREEN}{cleanMessage}{RESET}");
                }
                else
                {
                    Console.WriteLine($"{GREY}[{timestamp}] [INFO] {cleanMessage}{RESET}");
                }
                break;
        }
    }
}