// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Execution;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Providers.Streams.Common;
using Orleans.Streams;

namespace Cratis.Events.Store.Grains
{
    /// <summary>
    /// Represents an implementation of <see cref="IEventLog"/>.
    /// </summary>
    [StorageProvider(ProviderName = EventLogState.StorageProvider)]
    public class EventLog : Grain<EventLogState>, IEventLog
    {
        public const string StreamProvider = "event-log";
        readonly ILogger<EventLog> _logger;
        EventLogId _eventLogId = EventLogId.Unspecified;
        TenantId _tenantId = TenantId.NotSet;
        IAsyncStream<AppendedEvent>? _stream;

        /// <summary>
        /// Initializes a new instance of <see cref="EventLog"/>.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{T}"/> for logging.</param>
        public EventLog(ILogger<EventLog> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override async Task OnActivateAsync()
        {
            _eventLogId = this.GetPrimaryKey(out var tenantId);
            _tenantId = tenantId;

            var streamProvider = GetStreamProvider(StreamProvider);
            _stream = streamProvider.GetStream<AppendedEvent>(_eventLogId, _tenantId.ToString());

            await base.OnActivateAsync();
        }

        /// <inheritdoc/>
        public async Task WarmUp()
        {
            var appendedEvent = new AppendedEvent(
                new EventMetadata(0, new EventType(Guid.Empty, EventGeneration.First)),
                new EventContext(string.Empty, DateTimeOffset.UtcNow),
                "{}"
            );

            await _stream!.OnNextAsync(appendedEvent, new EventSequenceToken(-1));
            await WriteStateAsync();
        }

        /// <inheritdoc/>
        public async Task Append(EventSourceId eventSourceId, EventType eventType, string content)
        {
            _logger.Appending(eventType, eventSourceId, State.SequenceNumber, _eventLogId);

            var appendedEvent = new AppendedEvent(
                new EventMetadata(State.SequenceNumber, eventType),
                new EventContext(eventSourceId, DateTimeOffset.UtcNow),
                content
            );

            var updateSequenceNumber = true;

            try
            {
                await _stream!.OnNextAsync(appendedEvent, new EventSequenceToken((long)State.SequenceNumber.Value));
            }
            catch (UnableToAppendToEventLog ex)
            {
                _logger.FailedAppending(
                    ex.StreamId,
                    ex.SequenceNumber,
                    ex.TenantId,
                    ex.EventSourceId,
                    ex
                );

                updateSequenceNumber = false;
            }
            catch { }

            if (updateSequenceNumber)
            {
                State.SequenceNumber++;
                await WriteStateAsync();
            }
        }

        /// <inheritdoc/>
        public Task Compensate(EventLogSequenceNumber sequenceNumber, EventType eventType, string content, DateTimeOffset? validFrom = default)
        {
            _logger.Compensating(eventType, sequenceNumber, _eventLogId);

            return Task.CompletedTask;
        }
    }
}
