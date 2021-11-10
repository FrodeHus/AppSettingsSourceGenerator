using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;


namespace Reodor.AppSettingsSourceGenerator
{

    [Generator]
    public class AppSettingsSourceGenerator : ISourceGenerator
    {

        internal static string GenerateRecordSource(string recordName, Dictionary<string, object> values)
        {
            var builder = new StringBuilder();
            builder.Append(@"
using System;
namespace AppSettings
{

");
            builder
                .Append("    public record ")
                .AppendLine(recordName)
                .Append('{')
                .AppendLine();
            if (values[recordName] == null)
            {
                throw new ArgumentException($"No values found in given dictionary for {recordName}", nameof(values));
            }

            if (values[recordName] is not JObject propValues)
            {
                return null;
            }

            switch (propValues.Type)
            {
                case JTokenType.Object:
                    builder.Append(GeneratePropertiesFromDict(propValues.ToObject<Dictionary<string, object>>()));
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
                builder.Append("        public ").Append(declaration).AppendLine(" { get; init; }");
            }
            return builder.ToString();
        }

        internal static List<string> ExtractTopLevelPropertyNames(Dictionary<string, object> values)
        {

            if (values == null)
            {
                return new List<string>();
            }

            return values.Select(x => StringToValidRecordName(x.Key.Trim())).ToList();
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
            foreach (var file in context.AdditionalFiles)
            {
                if (!Path.GetExtension(file.Path).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var json = file.GetText()?.ToString();
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (values == null)
                {
                    continue;
                }

                foreach (var recordName in ExtractTopLevelPropertyNames(values))
                {
                    var source = GenerateRecordSource(recordName, values);
                    if (source == null)
                    {
                        continue;
                    }

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
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }
    }
}