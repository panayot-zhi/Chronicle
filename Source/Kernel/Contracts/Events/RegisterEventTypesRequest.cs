// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ProtoBuf;

namespace Cratis.Chronicle.Contracts.Events;

/// <summary>
/// Represents a request for registering a collection of event types to an event store.
/// </summary>
[ProtoContract]
public class RegisterEventTypesRequest
{
    /// <summary>
    /// Gets or sets the event store name.
    /// </summary>
    [ProtoMember(1)]
    public string EventStore { get; set; }

    /// <summary>
    /// Gets or sets the collection of types to register.
    /// </summary>
    [ProtoMember(2, IsRequired = true)]
    public IList<EventTypeRegistration> Types { get; set; } = [];
}
