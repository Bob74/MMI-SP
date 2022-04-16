using System;
using System.IO;

using MMI_SP.Common;

/// <summary>
/// Static logger class that allows direct logging of anything to a text file
/// </summary>
static class Logger
{
    private const string logFileName = "MMI-SP.log";
    public static void ResetLogFile()
    {
        FileStream fs = File.Create(logFileName);
        fs.Close();
    }
    public static void Debug(object message)
    {
        if (MMI_SP.MMI.IsDebug)
        {
            Log("Debug - " + Utils.GetCurrentMethod(1) + " " + message);
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
        Log("Error - " + Utils.GetCurrentMethod(1) + " " + message);
    }
    public static void Exception(Exception ex)
    {
        Log("Exception - " + ex.Message + "\r\n" + ex.StackTrace);
    }

    private static void Log(object message)
    {
        File.AppendAllText(logFileName, DateTime.Now + " : " + message + Environment.NewLine);
    }
}