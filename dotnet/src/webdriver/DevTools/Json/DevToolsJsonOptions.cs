using System.Text.Json;

#nullable enable

namespace OpenQA.Selenium.DevTools.Json;

internal static class DevToolsJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new JsonSerializerOptions()
    {
        Converters =
        {
            new StringConverter(),
        }
    };
}
