using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Haley.Models {
    public sealed class StateSpec {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("is_initial")]
        public bool IsInitial { get; set; }
        [JsonPropertyName("is_final")]
        public bool IsFinal { get; set; }
        [JsonPropertyName("category")]
        public string? Category { get; set; }
        [JsonPropertyName("flags")]
        public List<string>? Flags { get; set; }
    }
}
