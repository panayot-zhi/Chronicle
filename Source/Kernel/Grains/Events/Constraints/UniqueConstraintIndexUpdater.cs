// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Concepts.Events;
using Cratis.Chronicle.Concepts.Events.Constraints;
using Cratis.Chronicle.Storage.Events.Constraints;

namespace Cratis.Chronicle.Grains.Events.Constraints;

/// <summary>
/// Represents an implementation of <see cref="IUpdateConstraintIndex"/> for unique constraints.
/// </summary>
/// <param name="definition"><see cref="UniqueConstraintDefinition"/> for the updater to use.</param>
/// <param name="context">In which <see cref="ConstraintValidationContext"/> it is working with.</param>
/// <param name="storage"><see cref="IUniqueConstraintsStorage"/> to use for storage.</param>
public class UniqueConstraintIndexUpdater(
    UniqueConstraintDefinition definition,
    ConstraintValidationContext context,
    IUniqueConstraintsStorage storage) : IUpdateConstraintIndex
{
    /// <inheritdoc/>
    public async Task Update(EventSequenceNumber eventSequenceNumber)
    {
        if (context.EventType.Id == definition.RemovedWith)
        {
            await storage.Remove(context.EventSourceId, definition.Name);
        }
        else
        {
            var (_, value) = definition.GetPropertyAndValue(context);
            if (value is not null)
            {
                await storage.Save(context.EventSourceId, definition.Name, eventSequenceNumber, value);
            }
        }
    }
}
