namespace clausndk.stronglogger;

internal class StrongLogger(StrongLoggerBuilder builder) : IStrongLogger
{
    public IReadOnlyList<IStrongLoggerAppender> Appenders { get; } = builder.Appenders;
    public LogLevel LogLevel { get; } = builder.LogLevel;

    public void Log(LogLevel logLevel, string message)
    {
        Log(logLevel, null, message);
    }

    public void Log(LogLevel logLevel, Exception? exception, string message)
    {
        if (LogLevel > logLevel)
            return;

        var ts = DateTimeOffset.UtcNow;

        Parallel.ForEach(Appenders, appender =>
        {
            appender.Write(ts, logLevel, exception, message);
        });
    }
}