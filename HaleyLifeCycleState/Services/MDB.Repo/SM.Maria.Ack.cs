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
        public Task<IFeedback<long>> Ack_Insert(string messageId, long transitionLogId) =>
                _agw.ScalarAsync<long>(_key, QRY_ACK_LOG.INSERT, (MESSAGE_ID, messageId), (TRANSITION_LOG, transitionLogId));

        public Task<IFeedback<bool>> Ack_MarkReceived(string messageId) =>
            _agw.NonQueryAsync(_key, QRY_ACK_LOG.ACK, (MESSAGE_ID, messageId));

        public Task<IFeedback<List<Dictionary<string, object>>>> Ack_GetPending(int retryAfterMinutes) =>
            _agw.ReadAsync(_key, QRY_ACK_LOG.RETRYQ, (RETRY_AFTER_MIN, retryAfterMinutes));

        public Task<IFeedback<bool>> Ack_Bump(long ackId) =>
            _agw.NonQueryAsync(_key, QRY_ACK_LOG.BUMP, (ID, ackId));
    }
}
