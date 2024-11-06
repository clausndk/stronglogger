using FakeItEasy;

namespace clausndk.stronglogger.test;

public class StrongLoggerTest
{
    [Theory, AutoFakeItEasyData]
    internal void CanInstantiate(StrongLogger sut)
    {
        Assert.NotNull(sut);
    }

    [Theory, AutoFakeItEasyData]
    internal void Appenders_Reflect_Builder(
        StrongLoggerBuilder builder,
        StrongLoggerConsoleAppender consoleAppender)
    {
        builder.AddAppender(consoleAppender);

        var sut = new StrongLogger(builder);

        Assert.Contains(sut.Appenders, a => a == consoleAppender);
    }

    [Theory, AutoFakeItEasyData]
    internal void LogLevel_Reflect_Builder(
        StrongLoggerBuilder builder)
    {
        var sut = new StrongLogger(builder);

        Assert.Equal(LogLevel.Verbose, sut.LogLevel);
    }

    [Theory, AutoFakeItEasyData]
    internal void Log_Logs_ToAppenders(
        IStrongLoggerAppender fakeAppender,
        LogLevel logLevel,
        string logMessage)
    {
        var sut = new StrongLoggerBuilder()
            .AddAppender(fakeAppender)
            .Build();

        sut.Log(logLevel, logMessage);

        A.CallTo(() =>
                fakeAppender.Write(logLevel, null, logMessage))
            .MustHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    internal void Log_MinimumLogLevelNotMet_NoForwarding(
        IStrongLoggerAppender fakeAppender,
        string logMessage)
    {
        var sut = new StrongLoggerBuilder()
            .AddAppender(fakeAppender)
            .SetMinimumLogLevel(LogLevel.Debug)
            .Build();

        sut.Log(LogLevel.Verbose, logMessage);

        A.CallTo(() =>
                fakeAppender.Write(LogLevel.Verbose, null, logMessage))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    internal void LogWithException_Logs_ToAppenders(
        IStrongLoggerAppender fakeAppender,
        LogLevel logLevel,
        Exception e,
        string logMessage)
    {
        var sut = new StrongLoggerBuilder()
            .AddAppender(fakeAppender)
            .Build();

        sut.Log(logLevel, e, logMessage);

        A.CallTo(() =>
                fakeAppender.Write(logLevel, e, logMessage))
            .MustHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    internal void LogWithException_MinimumLogLevelNotMet_NoForwarding(
        IStrongLoggerAppender fakeAppender,
        Exception e,
        string logMessage)
    {
        var sut = new StrongLoggerBuilder()
            .AddAppender(fakeAppender)
            .SetMinimumLogLevel(LogLevel.Debug)
            .Build();

        sut.Log(LogLevel.Verbose, e, logMessage);

        A.CallTo(() =>
                fakeAppender.Write(LogLevel.Verbose, e, logMessage))
            .MustNotHaveHappened();
    }
}