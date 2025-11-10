using Haley.Abstractions;
using Haley.Enums;
using Haley.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Haley.Services {
    public partial class LifeCycleStateMachine {

        #region Validation

        public async Task<bool> ValidateTransitionAsync(Guid fromStateId, Guid toStateId) {
            var fb = await _repo.GetTransition(ToInt(fromStateId), 0, 0);
            return fb != null && fb.Status && fb.Result != null && ToInt(fb.Result["to_state"]) == ToInt(toStateId);
        }

        #endregion

        #region Current State

        public async Task<LifeCycleState> GetCurrentStateAsync(string externalRefType, Guid externalRefId) {
            var instance = await GetInstanceAsync(externalRefType, externalRefId) ?? throw new InvalidOperationException($"Instance not found for {externalRefType}:{externalRefId}");
            var fb = await _repo.GetStateByName(instance.DefinitionVersion, instance.CurrentState.ToString());
            await ThrowIfFailed(fb, "GetStateByName");
            return fb.Result != null ? MapState(fb.Result) : throw new InvalidOperationException($"State not found for {instance.CurrentState}");
        }

        public Task<LifeCycleState> GetCurrentStateAsync<TEntity>(Guid externalRefId) => GetCurrentStateAsync(GetRefType<TEntity>(), externalRefId);

        #endregion

        #region Transition History

        public async Task<IReadOnlyList<LifeCycleTransitionLog?>> GetTransitionHistoryAsync(string externalRefType, Guid externalRefId) {
            var instance = await GetInstanceAsync(externalRefType, externalRefId);
            if (instance == null) return Array.Empty<LifeCycleTransitionLog>();

            var fb = await _repo.GetLogsByInstance(instance.Id);
            if (fb == null || !fb.Status || fb.Result == null) return Array.Empty<LifeCycleTransitionLog>();

            var list = new List<LifeCycleTransitionLog>();
            foreach (var r in fb.Result) {
                list.Add(new LifeCycleTransitionLog {
                    Id = ToLong(r["id"]),
                    InstanceId = ToLong(r["instance_id"]),
                    FromState = ToInt(r["from_state"]),
                    ToState = ToInt(r["to_state"]),
                    Event = ToInt(r["event"]),
                    Actor = ToStr(r["actor"]),
                    Metadata = ToStr(r["metadata"]),
                    Flags = (LifeCycleTransitionLogFlag)ToInt(r["flags"]),
                    Created = Convert.ToDateTime(r["created"])
                });
            }
            return list;
        }

        public Task<IReadOnlyList<LifeCycleTransitionLog?>> GetTransitionHistoryAsync<TEntity>(Guid externalRefId) => GetTransitionHistoryAsync(GetRefType<TEntity>(), externalRefId);

        #endregion

        #region Force Update

        public async Task ForceUpdateStateAsync(string externalRefType, Guid externalRefId, Guid newStateId, LifeCycleTransitionLogFlag flags = LifeCycleTransitionLogFlag.System) {
            var instance = await GetInstanceAsync(externalRefType, externalRefId) ?? throw new InvalidOperationException($"Instance not found for {externalRefType}:{externalRefId}");
            var logFb = await _repo.LogTransition(instance.Id, instance.CurrentState, ToInt(newStateId), 0, "system", flags, "Force update");
            await ThrowIfFailed(logFb, "LogTransition");
            var updFb = await _repo.UpdateInstanceState(instance.Id, ToInt(newStateId), 0, instance.Flags);
            await ThrowIfFailed(updFb, "UpdateInstanceState");
        }

        public Task ForceUpdateStateAsync<TEntity>(Guid externalRefId, Guid newStateId, LifeCycleTransitionLogFlag flags = LifeCycleTransitionLogFlag.System) => ForceUpdateStateAsync(GetRefType<TEntity>(), externalRefId, newStateId, flags);

        #endregion

        #region State Checks

        public async Task<bool> IsFinalStateAsync(Guid stateId) {
            var fb = await _repo.GetStateByName(0, stateId.ToString());
            return fb != null && fb.Status && fb.Result != null && ((LifeCycleStateFlag)ToInt(fb.Result["flags"])).HasFlag(LifeCycleStateFlag.IsFinal);
        }

        public async Task<bool> IsInitialStateAsync(Guid stateId) {
            var fb = await _repo.GetStateByName(0, stateId.ToString());
            return fb != null && fb.Status && fb.Result != null && ((LifeCycleStateFlag)ToInt(fb.Result["flags"])).HasFlag(LifeCycleStateFlag.IsInitial);
        }

        #endregion
    }
}
