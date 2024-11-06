namespace clausndk.stronglogger.test;

public class StrongLoggerBuilderTest
{
    [Theory, AutoFakeItEasyData]
    internal void CanInstantiate(StrongLoggerBuilder sut)
    {
        Assert.NotNull(sut);
    }

    [Theory, AutoFakeItEasyData]
    internal void LogLevel_DefaultsTo_Verbose(StrongLoggerBuilder sut)
    {
        Assert.Equal(LogLevel.Verbose, sut.LogLevel);
    }

    [Theory, AutoFakeItEasyData]
    internal void SetMinimumLogLevel_SetsMinimumLogLevel_ReturnsBuilder(StrongLoggerBuilder sut)
    {
        var result = sut.SetMinimumLogLevel(LogLevel.Warning);

        Assert.Equal(LogLevel.Warning, result.LogLevel);
        Assert.Equal(sut, result);
    }

    [Theory, AutoFakeItEasyData]
    internal void AddAppender_AddNull_ThrowsArgumentNullException(
        StrongLoggerBuilder sut)
    {
        Assert.Throws<ArgumentNullException>(() => sut.AddAppender(null!));
    }

    [Theory, AutoFakeItEasyData]
    internal void AddAppender_AddsAppender_ReturnsBuilder(
        StrongLoggerConsoleAppender consoleAppender,
        StrongLoggerBuilder sut)
    {
        var result = sut.AddAppender(consoleAppender);

        Assert.Equal(sut, result);
        Assert.Contains(result.Appenders, a => a == consoleAppender);
    }

    [Theory, AutoFakeItEasyData]
    internal void Build_CreatesLogger_ReturnsLogger(StrongLoggerBuilder sut)
    {
        var result = sut.Build();

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IStrongLogger>(result);
    }
}