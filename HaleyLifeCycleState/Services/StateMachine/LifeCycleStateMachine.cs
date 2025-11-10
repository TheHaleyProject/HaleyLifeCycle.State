using Haley.Abstractions;
using Haley.Enums;
using Haley.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Haley.Services {
    public partial class LifeCycleStateMachine : ILifeCycleStateMachine {
        private readonly ILifeCycleStateRepository _repo;
        // dictionary: transition_name (or event_name) -> guard delegate
        private readonly Dictionary<string, Func<object, Task<bool>>> _guards;
        public event Func<TransitionEventArgs, Task>? OnBeforeTransition;
        public event Func<TransitionEventArgs, Task>? OnAfterTransition;
        public event Func<TransitionEventArgs, Task>? OnTransitionFailed;


        public LifeCycleStateMachine(ILifeCycleStateRepository repo) { 
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _guards = new Dictionary<string, Func<object, Task<bool>>>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
