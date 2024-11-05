namespace clausndk.stronglogger.test;

public class StrongLoggerConfigurationBuilderTest
{
    [Theory, AutoFakeItEasyData]
    public void CanInstantiate(StrongLoggerConfigurationBuilder sut)
    {
        Assert.NotNull(sut);
    }
}