namespace OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition
{
    using System.Text.Json.Serialization;
    using System;

    /// <summary>
    /// Indicates the version of the Protocol Definition.
    /// </summary>
    public sealed class Version : IComparable<Version>
    {
        [JsonPropertyName("major")]
        public string Major { get; set; }

        [JsonPropertyName("minor")]
        public string Minor { get; set; }

        public int CompareTo(Version other)
        {
            if (other == null)
            {
                return -1;
            }

            return ToString().CompareTo(other.ToString());
        }

        public override bool Equals(object obj)
        {
            var other = obj as Version;

            if (other == null)
            {
                return false;
            }

            return ToString().Equals(other.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }
    }
}
