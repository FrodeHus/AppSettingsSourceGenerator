using FluentAssertions;
using System.Linq;
using Xunit;

namespace Reodor.AppSettingsSourceGenerator.Tests
{
    public class AppSettingsTests
    {
        [Fact]
        public void It_Can_Extract_Classnames_Based_On_Property_Names()
        {
            var source = @"
{
  ""MySettings"": {
    ""ApiEndpoint"": ""https://api.dockergen.frodehus.dev/""
  }
}
";
            var classNames = AppSettingsSourceGenerator.ExtractTopLevelPropertyNames(source);
            classNames.Should().HaveCount(1);
            classNames.Single().Should().Be("MySettings");
        }
    }
}