using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.Json;

namespace Reodor.AppSettingsSourceGenerator;

[Generator]
public class AppSettingsSourceGenerator : ISourceGenerator
{

    internal static string GenerateRecordSource(string recordName, Dictionary<string, object> values)
    {
        var builder = new StringBuilder();
        builder.Append(@"
#nullable enable
using System;
namespace AppSettings;

");
        builder
            .Append("public record ")
            .AppendLine(recordName)
            .Append('{')
            .AppendLine();
        if (values[recordName] == null || values[recordName] is not JsonElement)
        {
            throw new ArgumentException($"No values found in given dictionary for {recordName}", nameof(values));
        }

        var propValues = (JsonElement)values[recordName];
        var properties = propValues.ValueKind switch
        {
            JsonValueKind.Object => GeneratePropertiesFromDict(propValues.Deserialize<Dictionary<string, object>>() ?? new Dictionary<string, object>()),
            _ => null
        };

        if (properties != null)
        {
            builder.Append(properties);
        }

        builder.AppendLine("}");
        return builder.ToString().Trim();
    }

    internal static string GeneratePropertiesFromDict(Dictionary<string, object> values)
    {
        var builder = new StringBuilder();
        foreach (var kvp in values)
        {
            var declaration = GetPropertyTypeAndName(kvp.Key, kvp.Value);
            builder.Append("    public ").Append(declaration).AppendLine(" { get; init; }");
        }
        return builder.ToString();
    }

    internal static List<string> ExtractTopLevelPropertyNames(Dictionary<string, object> values)
    {

        if (values == null)
        {
            return new List<string>();
        }

        return values.Select(x => StringToValidRecordName(x.Key)).ToList();
    }


    internal static string GetPropertyTypeAndName(string fieldName, object fieldValue)
    {
        var typeName = fieldValue switch
        {
            true => "bool",
            false => "bool",
            int _ => "int",
            double _ => "double",
            float _ => "float",
            _ => "string"
        };
        return $"{typeName} {fieldName}";
    }

    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var file in context.AdditionalFiles)
        {
            if (!Path.GetExtension(file.Path).Equals(".json", StringComparison.OrdinalIgnoreCase)
                || !file.Path.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var json = File.ReadAllText(file.Path);
            var values = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (values == null)
            {
                continue;
            }

            foreach (var recordName in ExtractTopLevelPropertyNames(values))
            {
                var source = GenerateRecordSource(recordName, values);
                context.AddSource(recordName, SourceText.From(source, Encoding.UTF8));
            }
        }
    }

    internal static string StringToValidRecordName(string s)
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
