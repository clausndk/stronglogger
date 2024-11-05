using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;

namespace clausndk.stronglogger.test;

public class AutoFakeItEasyDataAttribute()
    : AutoDataAttribute(() => new Fixture().Customize(new AutoFakeItEasyCustomization()));