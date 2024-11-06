namespace clausndk.stronglogger;

public class StrongLoggerBuilder
{
    private readonly List<IStrongLoggerAppender> _appenders = [];

    public LogLevel LogLevel { get; private set; } = LogLevel.Verbose;
    public IReadOnlyList<IStrongLoggerAppender> Appenders => _appenders;

    public StrongLoggerBuilder SetMinimumLogLevel(LogLevel level)
    {
        LogLevel = level;
        return this;
    }

    public IStrongLogger Build()
    {
        return new StrongLogger(this);
    }

    public StrongLoggerBuilder AddAppender(IStrongLoggerAppender strongLoggingAppender)
    {
        ArgumentNullException.ThrowIfNull(strongLoggingAppender);

        _appenders.Add(strongLoggingAppender);

        return this;
    }

    public StrongLoggerBuilder AddConsoleAppender()
    {
        _appenders.Add(new StrongLoggerConsoleAppender());

        return this;
    }
}