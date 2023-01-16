// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aksio.Cratis.Changes;
using Aksio.Cratis.DependencyInversion;
using Aksio.Cratis.Dynamic;
using Aksio.Cratis.Execution;
using Aksio.Cratis.Kernel.Engines.Projections;
using Aksio.Cratis.Kernel.Engines.Projections.Definitions;
using Aksio.Cratis.Kernel.Engines.Projections.Pipelines;
using Aksio.Cratis.Kernel.Grains.Observation;
using Aksio.Cratis.Events;
using Aksio.Cratis.EventSequences;
using Aksio.Cratis.Observation;
using Aksio.Cratis.Projections;
using Aksio.Cratis.Projections.Definitions;
using Orleans;
using EngineProjection = Aksio.Cratis.Kernel.Engines.Projections.IProjection;
using Aksio.Cratis.Kernel.Orleans.Observers;
using Microsoft.Extensions.Logging;

namespace Aksio.Cratis.Kernel.Grains.Projections;

/// <summary>
/// Represents an implementation of <see cref="IProjection"/>.
/// </summary>
public class Projection : Grain, IProjection
{
    readonly ProviderFor<IProjectionDefinitions> _projectionDefinitionsProvider;
    readonly ProviderFor<IProjectionPipelineDefinitions> _projectionPipelineDefinitionsProvider;
    readonly IProjectionFactory _projectionFactory;
    readonly IProjectionPipelineFactory _projectionPipelineFactory;
    readonly IObjectsComparer _objectsComparer;
    readonly IEventSequenceStorageProvider _eventProvider;
    readonly IExecutionContextManager _executionContextManager;
    readonly ObserverManager<INotifyProjectionDefinitionsChanged> _definitionObservers;
    EngineProjection? _projection;
    IProjectionPipeline? _pipeline;
    IObserver? _observer;
    ProjectionId _projectionId;
    ProjectionDefinition? _definition;
    ProjectionPipelineDefinition? _pipelineDefinition;
    TenantId? _tenantId;
    MicroserviceId? _microserviceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="Projection"/> class.
    /// </summary>
    /// <param name="projectionDefinitionsProvider"><see cref="IProjectionDefinitions"/>.</param>
    /// <param name="projectionPipelineDefinitionsProvider"><see cref="IProjectionPipelineDefinitions"/> for working with pipelines.</param>
    /// <param name="projectionFactory"><see cref="IProjectionFactory"/> for creating engine projections.</param>
    /// <param name="projectionPipelineFactory"><see cref="IProjectionPipelineFactory"/> for creating the pipeline for the projection.</param>
    /// <param name="objectsComparer"><see cref="IObjectsComparer"/> to compare objects with.</param>
    /// <param name="eventProvider"><see cref="IEventSequenceStorageProvider"/> for getting events from storage.</param>
    /// <param name="executionContextManager">The <see cref="IExecutionContextManager"/>.</param>
    /// <param name="logger">Logger for logging.</param>
    public Projection(
        ProviderFor<IProjectionDefinitions> projectionDefinitionsProvider,
        ProviderFor<IProjectionPipelineDefinitions> projectionPipelineDefinitionsProvider,
        IProjectionFactory projectionFactory,
        IProjectionPipelineFactory projectionPipelineFactory,
        IObjectsComparer objectsComparer,
        IEventSequenceStorageProvider eventProvider,
        IExecutionContextManager executionContextManager,
        ILogger<Projection> logger)
    {
        _projectionDefinitionsProvider = projectionDefinitionsProvider;
        _projectionPipelineDefinitionsProvider = projectionPipelineDefinitionsProvider;
        _projectionFactory = projectionFactory;
        _projectionPipelineFactory = projectionPipelineFactory;
        _objectsComparer = objectsComparer;
        _eventProvider = eventProvider;
        _executionContextManager = executionContextManager;
        _projectionId = ProjectionId.NotSet;
        _definitionObservers = new(TimeSpan.FromMinutes(1), logger, "ProjectionDefinitionObservers");
    }

    /// <inheritdoc/>
    public override async Task OnActivateAsync()
    {
        _projectionId = this.GetPrimaryKey(out var keyAsString);
        var key = ProjectionKey.Parse(keyAsString);
        _microserviceId = key.MicroserviceId;
        _tenantId = key.TenantId;

        // TODO: This is a temporary work-around till we fix #264 & #265
        _executionContextManager.Establish(key.TenantId, CorrelationId.New(), key.MicroserviceId);

        await RefreshDefinition();

        _observer = GrainFactory.GetGrain<IObserver>(_projectionId, new ObserverKey(key.MicroserviceId, key.TenantId, key.EventSequenceId));

        await _observer.SetMetadata(_definition!.Name.Value, ObserverType.Projection);
        await _observer.Subscribe<IProjectionObserverSubscriber>(_projection!.EventTypes);
    }

    /// <inheritdoc/>
    public Task SubscribeDefinitionsChanged(INotifyProjectionDefinitionsChanged subscriber)
    {
        _definitionObservers.Subscribe(subscriber, subscriber);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task RefreshDefinition()
    {
        // TODO: This is a temporary work-around till we fix #264 & #265
        _executionContextManager.Establish(_tenantId!, CorrelationId.New(), _microserviceId);

        _definition = await _projectionDefinitionsProvider().GetFor(_projectionId);
        _pipelineDefinition = await _projectionPipelineDefinitionsProvider().GetFor(_projectionId);
        _projection = await _projectionFactory.CreateFrom(_definition);
        _pipeline = _projectionPipelineFactory.CreateFrom(_projection, _pipelineDefinition);
        _definitionObservers.Notify(_ => _.OnProjectionDefinitionsChanged());
    }

    /// <inheritdoc/>
    public Task Ensure() => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task<JsonObject> GetModelInstanceById(EventSourceId eventSourceId)
    {
        if (_projection is null)
        {
            return new JsonObject();
        }
        var cursor = await _eventProvider.GetFromSequenceNumber(EventSequenceId.Log, EventSequenceNumber.First, eventSourceId, _projection.EventTypes);
        var state = new ExpandoObject();
        while (await cursor.MoveNext())
        {
            if (!cursor.Current.Any())
            {
                break;
            }

            foreach (var @event in cursor.Current)
            {
                var changeset = new Changeset<AppendedEvent, ExpandoObject>(_objectsComparer, @event, state);
                var keyResolver = _projection.GetKeyResolverFor(@event.Metadata.Type);
                var key = await keyResolver(_eventProvider!, @event);
                var context = new ProjectionEventContext(key, @event, changeset);

                await HandleEventFor(_projection!, context);

                foreach (var change in changeset.Changes)
                {
                    switch (change)
                    {
                        case PropertiesChanged<ExpandoObject> propertiesChanged:
                            state = state.MergeWith((change.State as ExpandoObject)!);
                            break;

                        case ChildAdded childAdded:
                            var items = state.EnsureCollection<object>(childAdded.ChildrenProperty, key.ArrayIndexers);
                            items.Add(childAdded.Child);
                            break;
                    }
                }
            }
        }

        // TODO: Conversion from ExpandoObject to JsonObject can be improved - they're effectively both just Dictionary<string, object>
        var json = JsonSerializer.Serialize(state);
        var jsonObject = JsonNode.Parse(json)!;
        return (jsonObject as JsonObject)!;
    }

    /// <inheritdoc/>
    public Task Rewind()
    {
        _observer?.Rewind();
        return Task.CompletedTask;
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

}
