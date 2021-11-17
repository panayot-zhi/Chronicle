// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Cratis.Events.Projections;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using IEventStore = Cratis.Extensions.Dolittle.EventStore.IEventStore;
using IEventStream = Cratis.Extensions.Dolittle.EventStore.IEventStream;

namespace Cratis.Extensions.Dolittle.Projections
{
    /// <summary>
    /// Represents an implementation of <see cref="IProjectionEventProvider"/> for the Dolittle event store.
    /// </summary>
    public class ProjectionEventProvider : IProjectionEventProvider
    {
        readonly IEventStream _eventStream;
        readonly ILogger<ProjectionEventProvider> _logger;
        readonly ConcurrentBag<ISubject<Event>> _subjects = new();

        /// <summary>
        /// Initializes a new instance of <see cref="ProjectionEventProvider"/>.
        /// </summary>
        /// <param name="eventStore">The Dolittle <see cref="IEventStore"/>.</param>
        /// <param name="logger"><see cref="ILogger{T}"/> for logging.</param>
        public ProjectionEventProvider(
            IEventStore eventStore,
            ILogger<ProjectionEventProvider> logger)
        {
            _eventStream = eventStore.GetStream(EventStore.EventStreamId.EventLog);
            _logger = logger;
            Task.Run(() => WatchForEvents());
        }

        /// <inheritdoc/>
        public void ProvideFor(IProjection projection, ISubject<Event> subject)
        {
            _logger.ProvidingFor(projection.Identifier);
            _subjects.Add(subject);
        }

        /// <inheritdoc/>
        public async Task<IEventCursor> GetFromPosition(IProjection projection, EventLogSequenceNumber start)
        {
            if (!projection.EventTypes.Any())
            {
                _logger.SkippingProvidingForProjectionDueToNoEventTypes(projection.Identifier);
                return new EventCursor(null);
            }

            var eventTypes = projection.EventTypes.Select(_ => new global::Dolittle.SDK.Events.EventType(Guid.Parse(_.Value))).ToArray();
            var cursor = await _eventStream.GetFromPosition(start, eventTypes);
            return new EventCursor(cursor);
        }

        void WatchForEvents()
        {
            Task.Run(() =>
            {
                var cursor = _eventStream.Watch();
                while (cursor.MoveNext())
                {
                    if (!cursor.Current.Any()) continue;

                    foreach (var subject in _subjects)
                    {
                        foreach (var @event in cursor.Current.Select(_ => _.FullDocument.ToCratis()))
                        {
                            subject.OnNext(@event);
                        }
                    }
                }
            });
        }
    }
}
