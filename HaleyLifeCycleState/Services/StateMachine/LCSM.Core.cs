using Haley.Abstractions;
using Haley.Enums;
using Haley.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Haley.Services {
    public partial class LifeCycleStateMachine {
        public async Task<bool> ReceiveAckAsync(string messageId) {
            var fb = await _repo.Ack_MarkReceived(messageId);
            return fb != null && fb.Status;
        }

        public async Task RetryUnackedAsync(int retryAfterMinutes = 2) {
            var fb = await _repo.Ack_GetPending(retryAfterMinutes);
            if (fb?.Result == null) return;

            foreach (var row in fb.Result) {
                var ackId = Convert.ToInt64(row["id"]);
                var transitionLogId = Convert.ToInt64(row["transition_log"]);
                var messageId = row["message_id"]?.ToString();

                // Re-publish the same notification (idempotent)
                //await _notifier.PublishTransition(messageId, transitionLogId);

                await _repo.Ack_Bump(ackId);
            }
        }

        public void RegisterGuard(string transitionKey, Func<object, Task<bool>> guardFunc) {
            _guards[transitionKey] = guardFunc;
        }

        #region Instance Retrieval

        public async Task<LifeCycleInstance?> GetInstanceAsync(string externalRefType, Guid externalRefId) {
            var fb = await _repo.GetInstancesByRef(externalRefId.ToString());
            if (fb == null || !fb.Status || fb.Result == null || fb.Result.Count == 0) return null;
            var match = fb.Result.Find(x => string.Equals(ToStr(x["external_type"]), externalRefType, StringComparison.OrdinalIgnoreCase));
            return match != null ? MapInstance(match) : null;
        }

        public Task<LifeCycleInstance?> GetInstanceAsync<TEntity>(Guid externalRefId) => GetInstanceAsync(GetRefType<TEntity>(), externalRefId);

        #endregion

        #region Initialization

        public async Task InitializeAsync(string externalRefType, Guid externalRefId, int definitionVersion) {
            var initFb = await _repo.GetInitialState(definitionVersion);
            await ThrowIfFailed(initFb, "GetInitialState");
            if (initFb.Result == null) throw new InvalidOperationException($"No initial state for def_version {definitionVersion}");

            int initStateId = ToInt(initFb.Result["id"]);
            var regFb = await _repo.RegisterInstance(definitionVersion, initStateId, 0, externalRefId.ToString(), LifeCycleInstanceFlag.Active);
            await ThrowIfFailed(regFb, "RegisterInstance");
        }

        public Task InitializeAsync<TEntity>(Guid externalRefId, int definitionVersion) => InitializeAsync(GetRefType<TEntity>(), externalRefId, definitionVersion);

        #endregion

        #region Trigger

        public async Task<bool> TriggerAsync(string externalRefType, Guid externalRefId, Guid toStateId, string comment = null, object? context = null) {
            LifeCycleTransitionLog? log = null;
            try {
                var instance = await GetInstanceAsync(externalRefType, externalRefId);
                if (instance == null) throw new InvalidOperationException("Instance not found.");

                var fromState = instance.CurrentState;
                var transitionFb = await _repo.GetOutgoingTransitions(fromState, instance.DefinitionVersion);
                var transition = transitionFb.Result?.Find(x => Convert.ToInt32(x["to_state"]) == Convert.ToInt32(toStateId));
                if (transition == null) throw new InvalidOperationException($"Invalid transition {fromState} → {toStateId}");

                var transitionName = transition["event"]?.ToString() ?? $"T_{fromState}_{toStateId}";

                // Guard lookup
                if (_guards.TryGetValue(transitionName, out var guardFunc)) {
                    bool allowed = await guardFunc(context);
                    if (!allowed) throw new InvalidOperationException($"Guard condition failed for transition {transitionName}");
                }

                // Prepare transition log (in-memory model)
                log = new LifeCycleTransitionLog {
                    InstanceId = instance.Id,
                    FromState = fromState,
                    ToState = Convert.ToInt32(toStateId),
                    Event = Convert.ToInt32(transition["event"]),
                    Actor = "system",
                    Flags = LifeCycleTransitionLogFlag.Manual,
                    Metadata = comment,
                    Created = DateTime.UtcNow
                };

                // Before hook (unchanged)
                await RaiseAsync(OnBeforeTransition, log);

                // Persist transition + update instance
                var logIdFb = await _repo.LogTransition(instance.Id, fromState, log.ToState, log.Event, log.Actor, log.Flags, log.Metadata);
                await ThrowIfFailed(logIdFb, "LogTransition"); // INSERT ...; SELECT LAST_INSERT_ID(); returns id
                var updFb = await _repo.UpdateInstanceState(instance.Id, log.ToState, log.Event, instance.Flags);
                await ThrowIfFailed(updFb, "UpdateInstanceState");

                // --- ACK: mark SENT for this transition (minimal blue-tick record) ---
                // messageId lets the client send back an acknowledgement later
                var messageId = Guid.NewGuid().ToString();
                // NOTE: requires repo method: Ack_Insert(messageId, transitionLogId)
                // ack_log table already exists in schema
                await _repo.Ack_Insert(messageId, logIdFb.Result);

                // After hook (unchanged). If you want the client to see messageId via events,
                // you can pass it inside log.Metadata (append) or extend TransitionEventArgs later.
                await RaiseAsync(OnAfterTransition, log);

                return true;
            } catch (Exception ex) {
                await RaiseAsync(OnTransitionFailed, log, ex);
                return false;
            }
        }


        public Task<bool> TriggerAsync<TEntity>(Guid externalRefId, Guid toStateId, string? comment = null, object? context = null) => TriggerAsync(GetRefType<TEntity>(), externalRefId, toStateId, comment,context);

        #endregion
    }
}
