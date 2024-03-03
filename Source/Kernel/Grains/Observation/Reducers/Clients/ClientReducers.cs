// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Connections;
using Cratis.Observation;
using Cratis.Observation.Reducers;
using Microsoft.Extensions.Logging;

namespace Cratis.Kernel.Grains.Observation.Reducers.Clients;

/// <summary>
/// Represents an implementation of <see cref="IClientReducers"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ClientReducers"/> class.
/// </remarks>
/// <param name="kernel"><see cref="IKernel"/> for accessing global artifacts.</param>
/// <param name="logger"><see cref="ILogger"/> for logging.</param>
public class ClientReducers(
    IKernel kernel,
    ILogger<ClientReducers> logger) : Grain, IClientReducers
{
    /// <inheritdoc/>
    public async Task Register(
        ConnectionId connectionId,
        IEnumerable<ReducerDefinition> definitions,
        IEnumerable<TenantId> tenants)
    {
        logger.RegisterReducers();

        var microserviceId = (MicroserviceId)this.GetPrimaryKey();

        var eventStore = kernel.GetEventStore((string)microserviceId);

        foreach (var definition in definitions)
        {
            await eventStore.ReducerPipelineDefinitions.Register(definition);
            foreach (var tenantId in tenants)
            {
                logger.RegisterReducer(
                    definition.ReducerId,
                    definition.Name,
                    definition.EventSequenceId);

                var key = new ObserverKey(microserviceId, tenantId, definition.EventSequenceId);
                var reducer = GrainFactory.GetGrain<IClientReducer>(definition.ReducerId, key);
                await reducer.Start(definition.Name, connectionId, definition.EventTypes);
            }
        }
    }
}
