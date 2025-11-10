using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Haley.Models {
    public sealed class TransitionSpec {
        [JsonPropertyName("from")]
        public string From { get; set; } = "";
        [JsonPropertyName("event")]
        public string? Event { get; set; }
        [JsonPropertyName("to")]
        public string To { get; set; } = "";
        [JsonPropertyName("guard")]
        public string? Guard { get; set; }
        [JsonPropertyName("flags")]
        public List<string>? Flags { get; set; }
    }
}
