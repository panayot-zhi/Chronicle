// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Events.Constraints;

/// <summary>
/// Represents a definition of a unique event type and property constraint.
/// </summary>
/// <param name="EventTypeId">The <see cref="EventTypeId"/>.</param>
/// <param name="Properties">The properties on the event type.</param>
public record UniqueConstraintEventDefinition(EventTypeId EventTypeId, IEnumerable<string> Properties);
