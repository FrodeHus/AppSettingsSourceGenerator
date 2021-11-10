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
                .Append("    {")
                .AppendLine();
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
                    builder.Append(GeneratePropertiesFromDict(JsonSerializer.Deserialize<Dictionary<string, object>>(propValues)));
                    break;
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString().Trim();
        }

        internal static string GeneratePropertiesFromDict(Dictionary<string, object> values)
        {
            var builder = new StringBuilder();
            foreach (var kvp in values)
            {
                var declaration = GetPropertyTypeAndName(kvp.Key, kvp.Value);
                builder.Append("        public ").Append(declaration).AppendLine(" { get; set; } = default!;");
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


        internal static string GetPropertyTypeAndName(string fieldName, object fieldValue)
        {
            var typeName = "string";
            switch (fieldValue)
            {
                case Boolean _:
                    typeName = "bool";
                    break;
                case int _:
                    typeName = "int";
                    break;
                case double _:
                    typeName = "double";
                    break;
                case float _:
                    typeName = "float";
                    break;
            };
            return $"{typeName} {fieldName}";
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var addedSources = new List<string>();
            foreach (var file in context.AdditionalFiles)
            {
                if (!Path.GetExtension(file.Path).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var json = file.GetText()?.ToString();
                var values = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (values == null)
                {
                    continue;
                }


                foreach (var className in ExtractTopLevelPropertyNames(values))
                {
                    if (addedSources.Contains(className)) continue;
                    var source = GenerateAppSettingSource(className, values, context.Compilation.AssemblyName);
                    if (source == null)
                    {
                        continue;
                    }
                    context.AddSource(className, SourceText.From(source, Encoding.UTF8));
                    addedSources.Add(className);
                }
            }
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
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }
    }
}