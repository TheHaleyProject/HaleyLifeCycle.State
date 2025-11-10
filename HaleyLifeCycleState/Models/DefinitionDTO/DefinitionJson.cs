using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Haley.Models {

    public sealed class DefinitionJson {
        [JsonPropertyName("environment")]
        public string? Environment { get; set; }
        [JsonPropertyName("definition")]
        public DefinitionSpec Definition { get; set; } = new();
        [JsonPropertyName("states")] 
        public List<StateSpec> States { get; set; } = new();
        [JsonPropertyName("events")]
        public List<string>? Events { get; set; }
        [JsonPropertyName("transitions")]
        public List<TransitionSpec> Transitions { get; set; } = new();
    }
}
