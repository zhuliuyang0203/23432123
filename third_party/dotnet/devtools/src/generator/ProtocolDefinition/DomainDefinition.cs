namespace OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition
{
    using System.Text.Json.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class DomainDefinition : ProtocolDefinitionItem
    {
        public DomainDefinition()
        {
            Dependencies = new HashSet<string>();

            Types = new Collection<TypeDefinition>();
            Events = new Collection<EventDefinition>();
            Commands = new Collection<CommandDefinition>();
        }

        [JsonPropertyName("domain")]
        public override string Name { get; set; }

        [JsonPropertyName("types")]
        public ICollection<TypeDefinition> Types { get; set; }

        [JsonPropertyName("commands")]
        public ICollection<CommandDefinition> Commands { get; set; }

        [JsonPropertyName("events")]
        public ICollection<EventDefinition> Events { get; set; }

        [JsonPropertyName("dependencies")]
        public ICollection<string> Dependencies { get; set; }
    }
}
