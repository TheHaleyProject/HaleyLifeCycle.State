using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Haley.Models {
    public sealed class DefinitionLoadResult {
        public long DefinitionId { get; set; }
        public int DefinitionVersionId { get; set; }
        public string Environment { get; set; } = "";
        public int VersionCode { get; set; }
        public int StateCount { get; set; }
        public int EventCount { get; set; }
        public int TransitionCount { get; set; }
    }
}
