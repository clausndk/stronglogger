namespace clausndk.stronglogger.json;

public class JsonFileStrongLoggerAppender : IStrongLoggerAppender
{
    public void Write(DateTimeOffset timestamp, LogLevel logLevel, Exception? exception, string logMessage)
    {
        throw new NotImplementedException();
    }
}
