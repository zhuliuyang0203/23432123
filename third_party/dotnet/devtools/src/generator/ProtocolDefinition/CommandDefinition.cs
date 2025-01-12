namespace OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition
{
    using System.Text.Json.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class CommandDefinition : ProtocolDefinitionItem
    {
        public CommandDefinition()
        {
            Handlers = new HashSet<string>();

            Parameters = new Collection<TypeDefinition>();
            Returns = new Collection<TypeDefinition>();
        }

        [JsonPropertyName("handlers")]
        public ICollection<string> Handlers { get; set; }

        [JsonPropertyName("parameters")]
        public ICollection<TypeDefinition> Parameters { get; set; }

        [JsonPropertyName("returns")]
        public ICollection<TypeDefinition> Returns { get; set; }

        [JsonPropertyName("redirect")]
        public string Redirect { get; set; }

        [JsonIgnore]
        public bool NoParameters => Parameters == null || Parameters.Count == 0;
    }
}
