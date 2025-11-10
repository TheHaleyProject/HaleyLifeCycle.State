using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Haley.Models {
    public sealed class DefinitionSpec {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("version")]
        public string? Version { get; set; }
        [JsonPropertyName("version_code")]
        public int? VersionCode { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
