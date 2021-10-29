using Ed.Bannerboard.UI.Logic;
using FluentAssertions;
using System;
using Xunit;

namespace Ed.Bannerboard.UI.Tests
{
    public class WidgetBaseTests
    {
        private readonly WidgetBase _target;

        public WidgetBaseTests()
        {
            _target = new WidgetBase();
        }

        [Theory]
        [InlineData("0.2.0", "0.2.0", true)]
        [InlineData("0.2.1", "0.2.0", true)]
        [InlineData("0.3.0", "0.2.0", true)]
        [InlineData("0.1.0", "0.2.0", false)]
        public void IsCompatibleShouldWork(string version, string minimumVersion, bool result)
        {
            _target
                .IsCompatible(new Version(version), new Version(minimumVersion))
                .Should()
                .Be(result);
        }

        [Fact]
        public void IsCompatibleShouldHandleNull()
        {
            _target
                .IsCompatible(null, new Version("0.2.0"))
                .Should()
                .BeFalse();
        }
    }
}
