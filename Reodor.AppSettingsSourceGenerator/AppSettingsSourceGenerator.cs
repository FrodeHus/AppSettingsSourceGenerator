using Microsoft.CodeAnalysis;
using System.Text;

namespace Reodor.AppSettingsSourceGenerator;

[Generator]
public class AppSettingsSourceGenerator : ISourceGenerator
{

    internal string GenerateClassFile(string className)
    {
        var builder = new StringBuilder();
        builder.Append(@"
#nullable enable
namespace 
");

        return builder.ToString();
    }

    internal static List<string> ExtractTopLevelPropertyNames(string appsettingsJson)
    {
        var values = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(appsettingsJson);

        if (values == null)
        {
            return new List<string>();
        }

        return values.Select(x => StringToValidClassName(x.Key)).ToList();
    }
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var file in context.AdditionalFiles)
        {
            if (Path.GetExtension(file.Path).Equals(".json", StringComparison.OrdinalIgnoreCase)
                && file.Path.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase))
            {

            }
        }
    }

    internal static string StringToValidClassName(string s)
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
