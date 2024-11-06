namespace clausndk.stronglogger.test;
public class StrongLoggerConsoleAppenderTest
{
    [Theory, AutoFakeItEasyData]
    internal void CanInstantiate(StrongLoggerConsoleAppender sut)
    {
        Assert.NotNull(sut);
    }
}
