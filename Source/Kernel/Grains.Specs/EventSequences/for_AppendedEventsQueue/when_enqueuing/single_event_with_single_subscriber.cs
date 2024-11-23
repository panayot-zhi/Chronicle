// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Concepts.Events;
using Cratis.Chronicle.Concepts.Observation;
using Cratis.Chronicle.Grains.Observation;

namespace Cratis.Chronicle.Grains.EventSequences.for_AppendedEventsQueue.when_enqueuing;

public class single_event_with_single_subscriber : given.a_single_subscriber
{
    EventType _eventType = new("Some event", 1);
    AppendedEvent _appendedEvent;

    EventSourceId _eventSourceId;

    void Establish()
    {
        _appendedEvent = AppendedEvent.EmptyWithEventType(_eventType);
        _eventSourceId = Guid.NewGuid();
        _appendedEvent = _appendedEvent with { Context = EventContext.Empty with { EventSourceId = _eventSourceId } };
    }

    protected override IEnumerable<EventType> EventTypes => [_eventType];

    async Task Because()
    {
        await _queue.Enqueue([_appendedEvent]);
        await _queue.AwaitQueueDepletion();
    }

    [Fact] void should_call_handle_on_observer_once() => _handledEvents.Count.ShouldEqual(1);
    [Fact] void should_call_handle_on_observer_with_correct_event_source_id() => _handledEvents[0].Partition.Value.ShouldEqual(_eventSourceId.Value);
    [Fact] void should_call_handle_on_observer_with_correct_event() => _handledEvents[0].Events.ShouldContainOnly(_appendedEvent);
}
