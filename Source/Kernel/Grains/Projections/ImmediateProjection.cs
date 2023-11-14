// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Dynamic;
using Aksio.Cratis.Changes;
using Aksio.Cratis.Dynamic;
using Aksio.Cratis.Events;
using Aksio.Cratis.EventSequences;
using Aksio.Cratis.Json;
using Aksio.Cratis.Kernel.Engines.Projections;
using Aksio.Cratis.Kernel.Keys;
using Aksio.Cratis.Kernel.Schemas;
using Aksio.Cratis.Projections;
using Aksio.Cratis.Properties;
using Aksio.DependencyInversion;
using EngineProjection = Aksio.Cratis.Kernel.Engines.Projections.IProjection;

namespace Aksio.Cratis.Kernel.Grains.Projections;

/// <summary>
/// Represents an implementation of <see cref="IImmediateProjection"/>.
/// </summary>
public class ImmediateProjection : Grain, IImmediateProjection
{
    readonly ProviderFor<IProjectionManager> _projectionManagerProvider;
    readonly ProviderFor<ISchemaStore> _schemaStoreProvider;
    readonly IObjectComparer _objectComparer;
    readonly IEventSequenceStorage _eventProvider;
    readonly IExpandoObjectConverter _expandoObjectConverter;
    readonly IExecutionContextManager _executionContextManager;
    ImmediateProjectionKey? _projectionKey;
    EventSequenceNumber _lastHandledEventSequenceNumber;
    ExpandoObject? _initialState;
    ProjectionId _projectionId = ProjectionId.NotSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmediateProjection"/> class.
    /// </summary>
    /// <param name="projectionManagerProvider"><see cref="IProjectionManager"/> for working with engine projections.</param>
    /// <param name="schemaStoreProvider">Provider for <see cref="ISchemaStore"/> for event schemas.</param>
    /// <param name="objectComparer"><see cref="IObjectComparer"/> to compare objects with.</param>
    /// <param name="eventProvider"><see cref="IEventSequenceStorage"/> for getting events from storage.</param>
    /// <param name="expandoObjectConverter"><see cref="IExpandoObjectConverter"/> to convert between JSON and ExpandoObject.</param>
    /// <param name="executionContextManager">The <see cref="IExecutionContextManager"/>.</param>
    public ImmediateProjection(
        ProviderFor<IProjectionManager> projectionManagerProvider,
        ProviderFor<ISchemaStore> schemaStoreProvider,
        IObjectComparer objectComparer,
        IEventSequenceStorage eventProvider,
        IExpandoObjectConverter expandoObjectConverter,
        IExecutionContextManager executionContextManager)
    {
        _projectionManagerProvider = projectionManagerProvider;
        _schemaStoreProvider = schemaStoreProvider;
        _objectComparer = objectComparer;
        _eventProvider = eventProvider;
        _expandoObjectConverter = expandoObjectConverter;
        _executionContextManager = executionContextManager;
        _lastHandledEventSequenceNumber = EventSequenceNumber.Unavailable;
    }

    /// <inheritdoc/>
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _projectionId = this.GetPrimaryKey(out var keyAsString);
        _projectionKey = ImmediateProjectionKey.Parse(keyAsString);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<ImmediateProjectionResult> GetModelInstance()
    {
        // TODO: This is a temporary work-around till we fix #264 & #265
        _executionContextManager.Establish(_projectionKey!.TenantId, _executionContextManager.Current.CorrelationId, _projectionKey.MicroserviceId);

        var projection = _projectionManagerProvider().Get(_projectionId);

        if (!projection.EventTypes.Any())
        {
            return ImmediateProjectionResult.Empty;
        }

        var affectedProperties = new HashSet<PropertyPath>();

        var modelKey = _projectionKey.ModelKey.IsSpecified ? (EventSourceId)_projectionKey.ModelKey.Value : null!;

        var fromSequenceNumber = _lastHandledEventSequenceNumber == EventSequenceNumber.Unavailable ? EventSequenceNumber.First : _lastHandledEventSequenceNumber.Next();
        var cursor = await _eventProvider.GetFromSequenceNumber(EventSequenceId.Log, fromSequenceNumber, modelKey, projection.EventTypes);
        var projectedEventsCount = 0;
        var state = _initialState ?? new ExpandoObject();
        while (await cursor.MoveNext())
        {
            if (!cursor.Current.Any())
            {
                break;
            }

            var events = cursor.Current.ToArray();
            var result = await HandleEvents(projection, affectedProperties, state, events);
            projectedEventsCount += result.ProjectedEventsCount;
            state = result.State;

            _lastHandledEventSequenceNumber = events[^1].Metadata.SequenceNumber;
        }

        _initialState = state;
        var jsonObject = _expandoObjectConverter.ToJsonObject(state, projection.Model.Schema);
        return new(jsonObject, affectedProperties, projectedEventsCount);
    }

    /// <inheritdoc/>
    public async Task<ImmediateProjectionResult> GetCurrentModelInstanceWithAdditionalEventsApplied(IEnumerable<EventToApply> events)
    {
        // TODO: This is a temporary work-around till we fix #264 & #265
        _executionContextManager.Establish(_projectionKey!.TenantId, _executionContextManager.Current.CorrelationId, _projectionKey.MicroserviceId);

        var projection = _projectionManagerProvider().Get(_projectionId);
        var affectedProperties = new HashSet<PropertyPath>();

        var schemaStoreProvider = _schemaStoreProvider();
        var eventsToApplyTasks = events.Select(async _ =>
        {
            var eventSchema = await schemaStoreProvider.GetFor(_.EventType.Id, _.EventType.Generation);
            return AppendedEvent.EmptyWithEventType(_.EventType) with
            {
                Content = _expandoObjectConverter.ToExpandoObject(_.Content, eventSchema.Schema)
            };
        }).ToArray();
        var eventsToApply = await Task.WhenAll(eventsToApplyTasks);
        var initialState = _initialState ?? new ExpandoObject();
        var result = await HandleEvents(projection, affectedProperties, initialState, eventsToApply);
        var jsonObject = _expandoObjectConverter.ToJsonObject(result.State, projection.Model.Schema);
        return new(jsonObject, affectedProperties, result.ProjectedEventsCount);
    }

    /// <inheritdoc/>
    public Task Dehydrate()
    {
        DeactivateOnIdle();
        return Task.CompletedTask;
    }

    async Task<(int ProjectedEventsCount, ExpandoObject State)> HandleEvents(EngineProjection projection, HashSet<PropertyPath> affectedProperties, ExpandoObject initialState, AppendedEvent[] events)
    {
        var projectedEventsCount = 0;
        var state = initialState;

        foreach (var @event in events)
        {
            var changeset = new Changeset<AppendedEvent, ExpandoObject>(_objectComparer, @event, initialState);
            var keyResolver = projection.GetKeyResolverFor(@event.Metadata.Type);
            var key = await keyResolver(_eventProvider!, @event);
            var context = new ProjectionEventContext(key, @event, changeset);

            await HandleEventFor(projection!, context);

            projectedEventsCount++;

            state = ApplyActualChanges(key, changeset.Changes, changeset.InitialState, affectedProperties);
        }

        return (projectedEventsCount, state);
    }

    async Task HandleEventFor(EngineProjection projection, ProjectionEventContext context)
    {
        if (projection.Accepts(context.Event.Metadata.Type))
        {
            projection.OnNext(context);
        }

        foreach (var child in projection.ChildProjections)
        {
            await HandleEventFor(child, context);
        }
    }

    ExpandoObject ApplyActualChanges(Key key, IEnumerable<Change> changes, ExpandoObject state, HashSet<PropertyPath> affectedProperties)
    {
        foreach (var change in changes)
        {
            switch (change)
            {
                case PropertiesChanged<ExpandoObject> propertiesChanged:
                    foreach (var difference in propertiesChanged.Differences)
                    {
                        affectedProperties.Add(difference.PropertyPath);
                    }
                    state = state.MergeWith((change.State as ExpandoObject)!);
                    break;

                case ChildAdded childAdded:
                    var items = state.EnsureCollection<object>(childAdded.ChildrenProperty, key.ArrayIndexers);
                    items.Add(childAdded.Child);
                    break;

                case Joined joined:
                    state = ApplyActualChanges(key, joined.Changes, state, affectedProperties);
                    break;

                case ResolvedJoin resolvedJoin:
                    state = ApplyActualChanges(key, resolvedJoin.Changes, state, affectedProperties);
                    break;
            }
        }

        return state;
    }
}
