// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Concepts.Events;

namespace Cratis.Chronicle.Grains.EventSequences.for_AppendedEventsQueue.when_enqueuing;

public class multiple_events_with_different_partitions_with_multiple_subscribers_that_subscribes_to_one_event_type_each : given.two_subscribers
{
    EventType _firstEventType = new("Some event", 1);
    EventType _secondEventType = new("Some other event", 2);
    AppendedEvent _firstAppendedEvent;
    AppendedEvent _secondAppendedEvent;
    EventSourceId _firstEventSourceId;
    EventSourceId _secondEventSourceId;

    async Task Establish()
    {
        _firstAppendedEvent = AppendedEvent.EmptyWithEventType(_firstEventType);
        _firstEventSourceId = "d5567269-fd83-4956-af21-26a75957cdc1";
        _firstAppendedEvent = _firstAppendedEvent with { Context = EventContext.Empty with { EventSourceId = _firstEventSourceId } };

        _secondAppendedEvent = AppendedEvent.EmptyWithEventType(_secondEventType);
        _secondEventSourceId = "e2924cac-79a6-4781-b9e3-7d277753b09e";
        _secondAppendedEvent = _secondAppendedEvent with { Context = EventContext.Empty with { EventSourceId = _secondEventSourceId } };

        await _queue.Subscribe(_firstObserverKey, [_firstEventType]);
        await _queue.Subscribe(_secondObserverKey, [_secondEventType]);
    }

    async Task Because()
    {
        await _queue.Enqueue([_firstAppendedEvent, _secondAppendedEvent]);
        await _queue.AwaitQueueDepletion();

        // waiting for queue depletion does not guarantee that the event was actually handled
        await Task.Delay(100);
    }

    [Fact] void should_call_handle_on_first_observer_once() => _firstObserverHandledEventsPerPartition.Sum(_ => _.Value.Count).ShouldEqual(1);
    [Fact] void should_call_handle_on_first_observer_with_correct_event_source_id_for_first_event() => _firstObserverHandledEventsPerPartition[_firstEventSourceId][0].Partition.Value.ShouldEqual(_firstEventSourceId.Value);
    [Fact] void should_call_handle_on_first_observer_with_correct_event_for_first_event() => _firstObserverHandledEventsPerPartition[_firstEventSourceId][0].Events.ShouldContainOnly(_firstAppendedEvent);
    [Fact] void should_call_handle_on_second_observer_once() => _secondObserverHandledEventsPerPartition.Sum(_ => _.Value.Count).ShouldEqual(1);
    [Fact] void should_call_handle_on_second_observer_with_correct_event_source_id_for_second_event() => _secondObserverHandledEventsPerPartition[_secondEventSourceId][0].Partition.Value.ShouldEqual(_secondEventSourceId.Value);
    [Fact] void should_call_handle_on_second_observer_with_correct_event_for_second_event() => _secondObserverHandledEventsPerPartition[_secondEventSourceId][0].Events.ShouldContainOnly(_secondAppendedEvent);
}
