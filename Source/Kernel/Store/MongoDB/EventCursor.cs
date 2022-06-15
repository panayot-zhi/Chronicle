// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MongoDB.Driver;

namespace Aksio.Cratis.Events.Store.MongoDB;

#pragma warning disable CA1849, MA0042 // Allow this blocking call - we get deadlocks when using MongoDB MoveNextAsync()

/// <summary>
/// Represents an implementation of <see cref="IEventCursor"/> for handling events from event sequence.
/// </summary>
public class EventCursor : IEventCursor
{
    readonly IEventConverter _converter;
    readonly IAsyncCursor<Event>? _innerCursor;

    /// <inheritdoc/>
    public IEnumerable<AppendedEvent> Current { get; private set; } = Array.Empty<AppendedEvent>();

    /// <summary>
    /// Initializes a new instance of the <see cref="EventCursor"/> class.
    /// </summary>
    /// <param name="converter"><see cref="IEventConverter"/> to convert event types.</param>
    /// <param name="innerCursor">The underlying MongoDB cursor.</param>
    public EventCursor(
        IEventConverter converter,
        IAsyncCursor<Event>? innerCursor)
    {
        _converter = converter;
        _innerCursor = innerCursor;
    }

    /// <inheritdoc/>
    public async Task<bool> MoveNext()
    {
        if (_innerCursor is null) return false;

        var result = _innerCursor.MoveNext();
        if (_innerCursor.Current is not null)
        {
            Current = await Task.WhenAll(_innerCursor.Current.Select(@event => _converter.ToAppendedEvent(@event)));
        }
        else
        {
            Current = Array.Empty<AppendedEvent>();
        }
        return result;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _innerCursor?.Dispose();
    }
}
