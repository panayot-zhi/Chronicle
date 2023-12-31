// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Aksio.Cratis.Connections;
using Aksio.Cratis.Observation;

namespace Aksio.Cratis.Kernel.Grains.Observation.Clients;

/// <summary>
/// Represents a key used for the <see cref="IObserverMediator"/> to track observer subscriptions.
/// </summary>
/// <param name="ObserverId">Observer the key is for.</param>
/// <param name="ConnectionId">Connection the key is for.</param>
public record ObserverMediatorKey(ObserverId ObserverId, ConnectionId ConnectionId);
