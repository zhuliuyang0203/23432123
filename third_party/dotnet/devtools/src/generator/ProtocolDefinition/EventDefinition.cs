namespace OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition
{
    using System.Text.Json.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class EventDefinition : ProtocolDefinitionItem
    {
        public EventDefinition()
        {
            Parameters = new Collection<TypeDefinition>();
        }

        [JsonPropertyName("parameters")]
        public ICollection<TypeDefinition> Parameters { get; set; }
    }
}
