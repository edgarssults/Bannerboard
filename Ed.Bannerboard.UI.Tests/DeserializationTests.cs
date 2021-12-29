using Ed.Bannerboard.Models;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;

namespace Ed.Bannerboard.UI.Tests
{
    public class DeserializationTests
    {
        [Fact]
        public void DeserializeVersionShouldWork()
        {
            var text = "{\"Type\":\"HandshakeModel\",\"Version\":\"0.3.0\"}";
            var model = JsonConvert.DeserializeObject<HandshakeModel>(text, new VersionConverter());
            model.Version.Minor.Should().Be(3);
        }
    }
}
