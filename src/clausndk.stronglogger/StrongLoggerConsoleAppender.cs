using System.Text;

namespace clausndk.stronglogger;

internal class StrongLoggerConsoleAppender : IStrongLoggerAppender
{
    public void Write(LogLevel logLevel, Exception? exception, string logMessage)
    {
        var sb = new StringBuilder($"{DateTimeOffset.UtcNow:O}|{logLevel}|");
        if (exception != null)
            sb.Append($"{exception}|");
        sb.Append(logMessage);
        
        Console.Out.WriteLine(sb);
    }
}