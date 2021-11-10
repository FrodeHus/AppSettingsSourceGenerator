using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Reodor.AppSettingsSourceGenerator.Tests
{
    public class AppSettingsTests
    {
        [Fact]
        public void It_Can_Extract_Classnames_Based_On_Property_Names()
        {
            var json = @"
{
  ""MySettings"": {
    ""Url"": ""https://api.dockergen.frodehus.dev/""
  }
}
";
            var values = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            var classNames = AppSettingsSourceGenerator.ExtractTopLevelPropertyNames(values);
            classNames.Should().HaveCount(1);
            classNames.Single().Should().Be("MySettings");
        }

        [Theory]
        [InlineData("test", "string")]
        [InlineData(true, "bool")]
        [InlineData(false, "bool")]
        [InlineData(int.MaxValue, "int")]
        [InlineData(double.MaxValue, "double")]
        [InlineData(float.MaxValue, "float")]
        public void It_Can_Determine_Proper_Value_Type(object fieldValue, string expectedType)
        {
            var actual = AppSettingsSourceGenerator.GetValueType(fieldValue);
            actual.Should().Be(expectedType);
        }

        [Fact]
        public void It_Can_Generate_Record_From_Json()
        {
            var json = @"
{
  ""MySettings"": {
    ""Url"": ""http://localhost""
  }
}
";
            var expected = @"
#nullable enable
using System;
namespace Test.AppSettings
{

    public partial class MySettings
    {
        public string Url { get; set; } = default!;
    }
}";
            var values = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            var source = AppSettingsSourceGenerator.GenerateAppSettingSource("MySettings", values, "Test");
            source.Should().Be(expected.Trim());
        }

        [Theory]
        [InlineData("Title", "asdf", "string")]
        [InlineData("Exists", true, "bool")]
        [InlineData("Count", 1, "int")]
        public void It_Can_Generate_Properties(string propertyName, object propertyValue, string valueType)
        {
            var expected = $"public {valueType} {propertyName} {{ get; set; }} = default!;";
            var dict = new Dictionary<string, object>
            {
                {propertyName, propertyValue}
            };
            var actual = AppSettingsSourceGenerator.GeneratePropertiesFromDict(dict).Trim();
            actual.Should().Be(expected);
        }
    }
}