using Haley.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Haley.Models;

namespace Haley.Abstractions {

    public interface ILifeCycleStateMachine {
        Task<LifeCycleInstance?> GetInstanceAsync(string externalRefType, Guid externalRefId);
        Task<LifeCycleInstance?> GetInstanceAsync<TEntity>(Guid externalRefId);

        Task InitializeAsync(string externalRefType, Guid externalRefId, int definitionVersion);
        Task InitializeAsync<TEntity>(Guid externalRefId, int definitionVersion);

        Task<bool> TriggerAsync(string externalRefType, Guid externalRefId, Guid toStateId, string comment = null,object ? context = null);
        Task<bool> TriggerAsync<TEntity>(Guid externalRefId, Guid toStateId, string? comment = null, object? context = null);

        Task<bool> ValidateTransitionAsync(Guid fromStateId, Guid toStateId);

        Task<LifeCycleState> GetCurrentStateAsync(string externalRefType, Guid externalRefId);
        Task<LifeCycleState> GetCurrentStateAsync<TEntity>(Guid externalRefId);

        Task<IReadOnlyList<LifeCycleTransitionLog?>> GetTransitionHistoryAsync(string externalRefType, Guid externalRefId);
        Task<IReadOnlyList<LifeCycleTransitionLog?>> GetTransitionHistoryAsync<TEntity>(Guid externalRefId);

        Task ForceUpdateStateAsync(string externalRefType, Guid externalRefId, Guid newStateId, LifeCycleTransitionLogFlag flags = LifeCycleTransitionLogFlag.System);
        Task ForceUpdateStateAsync<TEntity>(Guid externalRefId, Guid newStateId, LifeCycleTransitionLogFlag flags = LifeCycleTransitionLogFlag.System);

        Task<bool> IsFinalStateAsync(Guid stateId);
        Task<bool> IsInitialStateAsync(Guid stateId);

        Task<IFeedback<DefinitionLoadResult>> ImportDefinitionFromFileAsync(string filePath);
        Task<IFeedback<DefinitionLoadResult>> ImportDefinitionFromJsonAsync(string json);
    }
}