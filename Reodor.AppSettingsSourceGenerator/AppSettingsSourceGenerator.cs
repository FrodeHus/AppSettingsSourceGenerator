using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Reodor.AppSettingsSourceGenerator
{

    [Generator]
    public class AppSettingsSourceGenerator : ISourceGenerator
    {

        internal static string GenerateAppSettingSource(string className, Dictionary<string, object> values, string nameSpace)
        {
            var builder = new StringBuilder();
            builder.Append(@$"
#nullable enable
using System;
namespace {nameSpace}.AppSettings
{{

");
            builder
                .Append("    public partial class ")
                .AppendLine(className)
                .AppendLine("    {");
            if (values[className] == null)
            {
                throw new ArgumentException($"No values found in given dictionary for {className}", nameof(values));
            }

            if (values[className] is not JsonElement propValues)
            {
                return null;
            }

            switch (propValues.ValueKind)
            {
                case JsonValueKind.Object:
                    builder.Append(GeneratePropertiesFromDict(propValues.Deserialize<Dictionary<string, object>>(), 8));
                    break;
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString().Trim();
        }

        internal static string GeneratePropertiesFromDict(Dictionary<string, object> values, int indentationLevel = 0)
        {
            var builder = new StringBuilder();
            foreach (var kvp in values)
            {
                var valueType = GetValueType(kvp.Value);
                var propertyName = StringToValidTypeName(kvp.Key);
                var indent = Enumerable.Repeat(' ', indentationLevel);
                builder.Append(indent.ToArray())
                    .Append("public ")
                    .Append(valueType)
                    .Append(' ')
                    .Append(propertyName)
                    .AppendLine(" { get; set; } = default!;");
            }
            return builder.ToString();
        }

        internal static List<string> ExtractTopLevelPropertyNames(Dictionary<string, object> values)
        {
            if (values == null)
            {
                return new List<string>();
            }

            return values.Select(x => StringToValidTypeName(x.Key.Trim())).ToList();
        }

        internal static string GetValueType(object fieldValue) => fieldValue switch
        {
            bool _ => "bool",
            int _ => "int",
            double _ => "double",
            float _ => "float",
            _ => "string"
        };


        public void Execute(GeneratorExecutionContext context)
        {
            var appsettingFiles = context.AdditionalFiles.Where(f => Path.GetFileNameWithoutExtension(f.Path).StartsWith("appsettings", StringComparison.OrdinalIgnoreCase));
            var values = MergeAppSettings(appsettingFiles);
            foreach (var className in ExtractTopLevelPropertyNames(values))
            {
                var source = GenerateAppSettingSource(className, values, context.Compilation.AssemblyName);
                if (source == null)
                {
                    continue;
                }

                context.AddSource(className, SourceText.From(source, Encoding.UTF8));
            }
        }

        private Dictionary<string, object> MergeAppSettings(IEnumerable<AdditionalText> appsettingFiles)
        {
            var mergedAppsettings = new Dictionary<string, object>();
            foreach (var file in appsettingFiles)
            {
                var json = file.GetText()?.ToString();
                var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                foreach (var kvp in values)
                {
                    var value = kvp.Value;
                    var propertyName = kvp.Key;
                    if (value.ValueKind != JsonValueKind.Object)
                    {
                        propertyName = "SimpleSettings";
                        var simpleProperties = new Dictionary<string, object>();
                        if (mergedAppsettings.ContainsKey(propertyName))
                        {
                            var serialized = (JsonElement)mergedAppsettings[propertyName];
                            simpleProperties = serialized.Deserialize<Dictionary<string, object>>();
                        }
                        simpleProperties[kvp.Key] = value;
                        var serializedDict = JsonSerializer.SerializeToElement(simpleProperties);
                        mergedAppsettings[propertyName] = serializedDict;

                    }
                    else
                    {
                        mergedAppsettings[propertyName] = value;
                    }
                }
            }
            return mergedAppsettings;
        }

        internal static string StringToValidTypeName(string s)
        {
            s = s.Trim();
            s = char.IsLetter(s[0]) ? char.ToUpper(s[0]) + s.Substring(1) : s;
            s = char.IsDigit(s.Trim()[0]) ? "_" + s : s;
            s = new string(s.Select(ch => char.IsDigit(ch) || char.IsLetter(ch) ? ch : '_').ToArray());
            return s;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}