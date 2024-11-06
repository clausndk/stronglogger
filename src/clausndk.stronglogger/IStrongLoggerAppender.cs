namespace clausndk.stronglogger;

public interface IStrongLoggerAppender
{
    void Write(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception, string logMessage);
}