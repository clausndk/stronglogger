namespace clausndk.stronglogger;

public interface IStrongLogger
{
    void Log(LogLevel logLevel, string message);
    void Log(LogLevel logLevel, Exception? exception, string message);
    IReadOnlyList<IStrongLoggerAppender> Appenders { get; }
}

public enum LogLevel
{
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
    Fatal
}
