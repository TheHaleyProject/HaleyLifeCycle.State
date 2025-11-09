using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haley.Models {
    public class TransitionEventArgs : EventArgs {
        public LifeCycleTransitionLog? Log { get; }
        public Exception? Exception { get; }
        public string? Context { get; }

        public TransitionEventArgs(LifeCycleTransitionLog? log, Exception? ex = null, string? ctx = null) {
            Log = log;
            Exception = ex;
            Context = ctx;
        }
    }

}
