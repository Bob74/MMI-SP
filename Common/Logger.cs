using System;
using System.IO;

/// <summary>
/// Static logger class that allows direct logging of anything to a text file
/// </summary>
static class Logger
{
    private static string logFileName = "MMI-SP.log";
    public static void ResetLogFile()
    {
        FileStream fs = File.Create(logFileName);
        fs.Close();
    }
    public static void Debug(object message)
    {
        if (MMI_SP.MMI.IsDebug)
        {
            Log("Debug - " + MMI_SP.Tools.GetCurrentMethod(1) + " " + message);
        }
    }
    public static void Info(object message)
    {
        Log("Info - " + message);
    }
    
    public static void Warning(object message)
    {
        Log("Warning - " + message);
    }
    
    public static void Error(object message)
    {
        Log("Error - " + message);
    }

    private static void Log(object message)
    {
        File.AppendAllText(logFileName, DateTime.Now + " : " + message + Environment.NewLine);
    }
}