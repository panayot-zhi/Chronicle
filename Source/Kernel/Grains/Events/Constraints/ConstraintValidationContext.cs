// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Dynamic;
using Cratis.Chronicle.Concepts.Events;

namespace Cratis.Chronicle.Grains.Events.Constraints;

/// <summary>
/// Represents the context for constraint validation.
/// </summary>
public record ConstraintValidationContext
{
    readonly IEnumerable<IUpdateConstraintIndex> _updaters;
    readonly IEnumerable<IConstraintValidator> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintValidationContext"/> class.
    /// </summary>
    /// <param name="validators">The <see cref="IConstraintValidator">validators</see> involved in the context.</param>
    /// <param name="eventSourceId">The <see cref="Concepts.Events.EventSourceId"/> to validate for.</param>
    /// <param name="eventType">The <see cref="EventType"/> to validate for.</param>
    /// <param name="content">The content of the event.</param>
    public ConstraintValidationContext(
        IEnumerable<IConstraintValidator> validators,
        EventSourceId eventSourceId,
        EventType eventType,
        ExpandoObject content)
    {
        _updaters = validators.OfType<IHaveUpdateConstraintIndex>().Select(v => v.GetUpdateFor(this));
        _validators = validators.Where(_ => _.CanValidate(this));
        EventSourceId = eventSourceId;
        EventType = eventType;
        Content = content;
    }

    /// <summary>
    /// Gets the <see cref="EventSourceId"/> to validate for.
    /// </summary>
    public EventSourceId EventSourceId { get; }

    /// <summary>
    /// Gets the <see cref="EventType"/> to validate for.
    /// </summary>
    public EventType EventType { get; }

    /// <summary>
    /// Gets the content of the event.
    /// </summary>
    public ExpandoObject Content { get; }

    /// <summary>
    /// Perform validation on a <see cref="EventToValidateForConstraints"/>.
    /// </summary>
    /// <returns><see cref="ConstraintValidationResult"/> holding the result of validation.</returns>
    public async Task<ConstraintValidationResult> Validate()
    {
        var results = await Task.WhenAll(_validators.Select(v => v.Validate(this)));
        var violations = results.Where(r => !r.IsValid).SelectMany(r => r.Violations);
        return new()
        {
            Violations = violations.ToImmutableList()
        };
    }

    /// <summary>
    /// Update constraints with information from the <see cref="EventToValidateForConstraints"/> and <see cref="EventSequenceNumber"/>.
    /// </summary>
    /// <param name="eventSequenceNumber">The <see cref="EventSequenceNumber"/> of the event that might affect a constraint.</param>
    /// <returns>Awaitable task.</returns>
    /// <remarks>
    /// The purpose of this method is to update any constraints that might be affected by typically appending an event.
    /// Some constraints use this information to keep track of the sequence number that holds information the constraint will use when violated.
    /// </remarks>
    public async Task Update(EventSequenceNumber eventSequenceNumber)
    {
        await Task.WhenAll(_updaters.Select(v => v.Update(eventSequenceNumber)));
    }
}
