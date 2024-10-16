// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.Models;
using Cratis.Chronicle.Contracts.Primitives;
using Cratis.Chronicle.Contracts.Sinks;
using ProtoBuf;

namespace Cratis.Chronicle.Contracts.Projections;

/// <summary>
/// Represents the definition of a projection.
/// </summary>
[ProtoContract]
public class ProjectionDefinition
{
    /// <summary>
    /// Gets or sets the event sequence identifier the projection projects from.
    /// </summary>
    public string EventSequenceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the projection.
    /// </summary>
    [ProtoMember(1)]
    public string Identifier { get; set; }

    /// <summary>
    /// Gets or sets the target <see cref="ModelDefinition"/>.
    /// </summary>
    [ProtoMember(2)]
    public ModelDefinition Model { get; set; }

    /// <summary>
    /// Gets or sets whether or not the projection is an actively observing projection.
    /// </summary>
    [ProtoMember(3)]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether or not the projection is rewindable.
    /// </summary>
    [ProtoMember(4)]
    public bool IsRewindable { get; set; }

    /// <summary>
    /// Gets or sets the initial state to use for new instances of the model.
    /// </summary>
    [ProtoMember(5)]
    public string InitialModelState { get; set; }

    /// <summary>
    /// Gets or sets all the <see cref="FromDefinition"/> for <see cref="EventType">event types</see>.
    /// </summary>
    [ProtoMember(6, IsRequired = true)]
    public IDictionary<EventType, FromDefinition> From { get; set; } = new Dictionary<EventType, FromDefinition>();

    /// <summary>
    /// Gets or sets all the <see cref="JoinDefinition"/> for <see cref="EventType">event types</see>.
    /// </summary>
    [ProtoMember(7, IsRequired = true)]
    public IDictionary<EventType, JoinDefinition> Join { get; set; } = new Dictionary<EventType, JoinDefinition>();

    /// <summary>
    /// Gets or sets all the <see cref="ChildrenDefinition"/> for properties on model.
    /// </summary>
    [ProtoMember(8, IsRequired = true)]
    public IDictionary<string, ChildrenDefinition> Children { get; set; } = new Dictionary<string, ChildrenDefinition>();

    /// <summary>
    /// Gets or sets all the <see cref="FromDerivativesDefinition"/> for <see cref="EventType">event types</see>.
    /// </summary>
    [ProtoMember(9, IsRequired = true)]
    public IList<FromDerivativesDefinition> FromEvery { get; set; } = [];

    /// <summary>
    /// Gets or sets the full <see cref="FromEveryDefinition"/>.
    /// </summary>
    [ProtoMember(10)]
    public FromEveryDefinition All { get; set; }

    /// <summary>
    /// Gets or sets the optional <see cref="FromEventPropertyDefinition"/> definition.
    /// </summary>
    [ProtoMember(11)]
    public FromEventPropertyDefinition? FromEventProperty { get; set; }

    /// <summary>
    /// Gets or sets the definition of what removes a child, if any.
    /// </summary>
    [ProtoMember(12, IsRequired = true)]
    public IDictionary<EventType, RemovedWithDefinition> RemovedWith { get; set; } = new Dictionary<EventType, RemovedWithDefinition>();

    /// <summary>
    /// Gets or sets the definition of what removes a child through joining, if any.
    /// </summary>
    [ProtoMember(13, IsRequired = true)]
    public IDictionary<EventType, RemovedWithJoinDefinition> RemovedWithJoin { get; set; } = new Dictionary<EventType, RemovedWithJoinDefinition>();

    /// <summary>
    /// Gets or sets the last time the projection definition was updated.
    /// </summary>
    [ProtoMember(14)]
    public SerializableDateTimeOffset? LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the projection sink definition.
    /// </summary>
    [ProtoMember(15)]
    public SinkDefinition Sink { get; set; }
}
