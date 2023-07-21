// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Aksio.Commands;
using Aksio.Cratis.Connections;
using Aksio.Cratis.Events;
using Aksio.Cratis.EventSequences;
using Aksio.Cratis.Kernel.Grains.Clients;
using Aksio.Cratis.Observation;
using Aksio.Cratis.Reducers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Aksio.Cratis.Kernel.Grains.Observation.Reducers.Clients;

/// <summary>
/// Represents an implementation of <see cref="IClientReducerSubscriber"/>.
/// </summary>
public class ClientReducerSubscriber : Grain, IClientReducerSubscriber
{
    readonly ILogger<ClientReducerSubscriber> _logger;
    readonly IHttpClientFactory _httpClientFactory;
    readonly JsonSerializerOptions _jsonSerializerOptions;
    MicroserviceId _microserviceId = MicroserviceId.Unspecified;
    ObserverId _observerId = ObserverId.Unspecified;
    TenantId _tenantId = TenantId.NotSet;
    EventSequenceId _eventSequenceId = EventSequenceId.Unspecified;
    IConnectedClients? _connectedClients;
    IConnectedClients ConnectedClientsGrain => _connectedClients ??= GrainFactory.GetGrain<IConnectedClients>(_microserviceId);

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientReducerSubscriber"/> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> for logging.</param>
    /// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> for connecting to the client.</param>
    /// <param name="jsonSerializerOptions"><see cref="JsonSerializerOptions"/> for serialization.</param>
    public ClientReducerSubscriber(
        ILogger<ClientReducerSubscriber> logger,
        IHttpClientFactory httpClientFactory,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    /// <inheritdoc/>
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var id = this.GetPrimaryKey(out var keyAsString);
        var key = ObserverSubscriberKey.Parse(keyAsString);
        _microserviceId = key.MicroserviceId;
        _observerId = id;
        _tenantId = key.TenantId;
        _eventSequenceId = key.EventSequenceId;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<ObserverSubscriberResult> OnNext(AppendedEvent @event, ObserverSubscriberContext context)
    {
        _logger.EventReceived(
            _observerId.Value,
            _microserviceId,
            _tenantId,
            @event.Metadata.Type.Id,
            _eventSequenceId,
            @event.Context.SequenceNumber);

        if (context.Metadata is JsonElement connectedClientJsonObject)
        {
            var connectedClient = connectedClientJsonObject.Deserialize<ConnectedClient>(_jsonSerializerOptions);
            if (connectedClient is not null)
            {
                using var httpClient = _httpClientFactory.CreateClient(ConnectedClients.ConnectedClientsHttpClient);
                httpClient.BaseAddress = connectedClient.ClientUri;

                // Get the current state from the sink
                var reduce = new Reduce(new[] { @event }, null);

                using var jsonContent = JsonContent.Create(reduce, options: _jsonSerializerOptions);
                httpClient.DefaultRequestHeaders.Add(ExecutionContextAppBuilderExtensions.TenantIdHeader, _tenantId.ToString());
                var response = await httpClient.PostAsync($"/.cratis/reducers/{_observerId}", jsonContent);
                var commandResult = (await response.Content.ReadFromJsonAsync<CommandResult>(_jsonSerializerOptions))!;
                var state = ObserverSubscriberState.Ok;

                // Compare existing to new state and create a change set
                // On OK, apply changes to sink

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    await ConnectedClientsGrain.OnClientDisconnected(connectedClient.ConnectionId, "Client not found");
                    state = ObserverSubscriberState.Disconnected;
                }
                else if (response.StatusCode != HttpStatusCode.OK || !commandResult.IsSuccess)
                {
                    state = ObserverSubscriberState.Failed;
                }

                return new ObserverSubscriberResult(state, commandResult.ExceptionMessages, commandResult.ExceptionStackTrace);
            }
        }

        return new ObserverSubscriberResult(ObserverSubscriberState.Disconnected, Enumerable.Empty<string>(), string.Empty);
    }
}
