// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Concepts.Events;
using Cratis.Chronicle.Concepts.EventTypes;
using MongoDB.Bson.Serialization.Attributes;

namespace Cratis.Events.MongoDB.EventTypes;

/// <summary>
/// Represents the <see cref="EventTypeSchema"/> for MongoDB storage purpose.
/// </summary>
public class EventSchemaMongoDB
{
    /// <summary>
    /// Gets the identifier part of <see cref="EventType"/>.
    /// </summary>
    [BsonId]
    public string EventType { get; init; } = EventTypeId.Unknown.Value;

    /// <summary>
    /// Gets the generation part of the <see cref="EventType"/>>.
    /// </summary>
    public uint Generation { get; init; } = EventTypeGeneration.First.Value;

    /// <summary>
    /// Gets the tombstone part of the <see cref="EventType"/>>.
    /// </summary>
    public bool Tombstone { get; init; }

    /// <summary>
    /// Gets the actual schema as JSON.
    /// </summary>
    public string Schema { get; init; } = string.Empty;
}
