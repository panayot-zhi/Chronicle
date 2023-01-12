// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Aksio.Cratis.Integration;

/// <summary>
/// Represents an event to be appended.
/// </summary>
/// <param name="Event">The actual event.</param>
/// <param name="ValidFrom">Optionally the associated valid from.</param>
public record EventToAppend(object Event, DateTimeOffset? ValidFrom);
