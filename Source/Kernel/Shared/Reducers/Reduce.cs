// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Aksio.Cratis.Events;

namespace Aksio.Cratis.Reducers;

/// <summary>
/// Represents the payload for a reduce operation.
/// </summary>
/// <param name="Events">Events to reduce from.</param>
/// <param name="InitialState">The initial state.</param>
public record Reduce(IEnumerable<AppendedEvent> Events, object? InitialState);
