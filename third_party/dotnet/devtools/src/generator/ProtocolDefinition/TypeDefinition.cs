namespace OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition
{
    using System.Text.Json.Serialization;
    using OpenQA.Selenium.DevToolsGenerator.Converters;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class TypeDefinition : ProtocolDefinitionItem
    {
        public TypeDefinition()
        {
            Enum = new HashSet<string>();
            Properties = new Collection<TypeDefinition>();
        }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("enum")]
        public ICollection<string> Enum { get; set; }

        [JsonPropertyName("properties")]
        public ICollection<TypeDefinition> Properties { get; set; }

        [JsonPropertyName("items")]
        public TypeDefinition Items { get; set; }

        [JsonPropertyName("minItems")]
        public int MinItems { get; set; }

        [JsonPropertyName("maxItems")]
        public int MaxItems { get; set; }

        [JsonPropertyName("$ref")]
        public string TypeReference { get; set; }

        [JsonPropertyName("optional")]
        [JsonConverter(typeof(BooleanJsonConverter))]
        public bool Optional { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Id))
            {
                return Id;
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            return $"Ref: {TypeReference}";
        }
    }
}
