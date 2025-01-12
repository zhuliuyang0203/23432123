namespace OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition
{
    using System.Text.Json.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class ProtocolDefinition : IDefinition
    {
        public ProtocolDefinition()
        {
            Domains = new Collection<DomainDefinition>();
        }

        [JsonPropertyName("browserVersion")]
        [JsonRequired]
        public ProtocolVersionDefinition BrowserVersion { get; set; }

        [JsonPropertyName("version")]
        [JsonRequired]
        public Version Version { get; set; }

        [JsonPropertyName("domains")]
        [JsonRequired]
        public ICollection<DomainDefinition> Domains { get; set; }
    }
}
