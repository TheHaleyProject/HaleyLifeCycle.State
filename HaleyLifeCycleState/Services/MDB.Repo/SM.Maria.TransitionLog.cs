using Haley.Abstractions;
using Haley.Enums;
using Haley.Internal;
using Haley.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Haley.Internal.QueryFields;

namespace Haley.Services {
    public partial class LifeCycleStateMariaDB {

        public Task<IFeedback<long>> LogTransition(long instanceId, int fromState, int toState, int eventId, string actor, LifeCycleTransitionLogFlag flags, string metadata = null) =>
            _agw.ScalarAsync<long>(_key, QRY_TRANSITION_LOG.INSERT, (INSTANCE_ID, instanceId), (FROM_STATE, fromState), (TO_STATE, toState), (EVENT, eventId), (ACTOR, actor ?? string.Empty), (FLAGS, (int)flags), (METADATA, metadata ?? string.Empty));

        public Task<IFeedback<List<Dictionary<string, object>>>> GetLogsByInstance(long instanceId) =>
            _agw.ReadAsync(_key, QRY_TRANSITION_LOG.GET_BY_INSTANCE, (INSTANCE_ID, instanceId));

        public Task<IFeedback<List<Dictionary<string, object>>>> GetLogsByStateChange(int fromState, int toState) =>
            _agw.ReadAsync(_key, QRY_TRANSITION_LOG.GET_BY_STATE_CHANGE, (FROM_STATE, fromState), (TO_STATE, toState));

        public Task<IFeedback<List<Dictionary<string, object>>>> GetLogsByDateRange(System.DateTime from, System.DateTime to) =>
            _agw.ReadAsync(_key, QRY_TRANSITION_LOG.GET_BY_DATE_RANGE, (CREATED, from), (MODIFIED, to));

        public Task<IFeedback<Dictionary<string, object>>> GetLatestLogForInstance(long instanceId) =>
            _agw.ReadSingleAsync(_key, QRY_TRANSITION_LOG.GET_LATEST_FOR_INSTANCE, (INSTANCE_ID, instanceId));
    }
}
