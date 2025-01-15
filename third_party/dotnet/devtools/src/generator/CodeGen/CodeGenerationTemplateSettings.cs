using System.Text.Json.Serialization;

namespace OpenQA.Selenium.DevToolsGenerator.CodeGen
{
    /// <summary>
    /// Defines settings around templates
    /// </summary>
    public class CodeGenerationTemplateSettings
    {
        [JsonPropertyName("templatePath")]
        public string TemplatePath { get; set; }

        [JsonPropertyName("outputPath")]
        public string OutputPath { get; set; }
    }
}
