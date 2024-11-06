using System.Text;

namespace clausndk.stronglogger;

internal class StrongLoggerConsoleAppender : IStrongLoggerAppender
{
    public void Write(LogLevel logLevel, Exception? exception, string logMessage)
    {
        switch (logLevel)
        {
            case LogLevel.Debug:
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                break;
            case LogLevel.Information:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogLevel.Fatal:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                break;
            default:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }

        var sb = new StringBuilder($"{DateTimeOffset.UtcNow:O}|{logLevel}|");
        LogException(sb, exception);
        sb.Append(logMessage);

        Console.Out.WriteLine(sb);
    }

    private void LogException(StringBuilder sb, Exception? exception)
    {
        if (exception == null)
            return;

        sb.Append(exception);
        sb.Append('|');
        if (exception.StackTrace != null)
        {
            sb.Append(exception.StackTrace);
            sb.Append('|');
        }

        LogException(sb, exception.InnerException);
    }
}