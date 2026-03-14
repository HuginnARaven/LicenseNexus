using System;
using System.Diagnostics;

namespace LicenseNexus.LoadTests.Helpers;

public static class HardwareMonitor
{
    private static Process? _psProcess;
    public static void Start(string outputFileName)
    {
        if (_psProcess != null && !_psProcess.HasExited)
        {
            Console.WriteLine("Monitoring is already started.");
            return;
        }

        Console.WriteLine($"\n[Monitor] Start collecting hardware metrics to a file: {outputFileName}...");

        _psProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"./Scripts/monitor.ps1\" -OutputFile \"{outputFileName}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            _psProcess.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Monitor] Error starting monitoring: {ex.Message}");
        }
    }
    
    public static void Stop()
    {
        if (_psProcess != null && !_psProcess.HasExited)
        {
            try
            {
                _psProcess.Kill();
                _psProcess.WaitForExit(2000);
                Console.WriteLine("[Monitor] Hardware metrics collection successfully stopped and saved.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Monitor] Process stop error: {ex.Message}");
            }
            finally
            {
                _psProcess.Dispose();
                _psProcess = null;
            }
        }
    }
}