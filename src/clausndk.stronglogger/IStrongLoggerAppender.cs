namespace clausndk.stronglogger;

public interface IStrongLoggerAppender
{
    void Write(LogLevel logLevel, Exception? exception, string logMessage);
}