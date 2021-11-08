// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Properties;

namespace Cratis.Events.Projections.Expressions
{
    /// <summary>
    /// Represents a <see cref="IPropertyMapperExpressionResolver"/> for resolving value from <see cref="EventSourceId"/> of the <see cref="Event"/>.
    /// </summary>
    public class EventSourceIdExpressionResolver : IPropertyMapperExpressionResolver
    {
        /// <inheritdoc/>
        public bool CanResolve(Property targetProperty, string expression) => expression == "$eventSourceId";

        /// <inheritdoc/>
        public PropertyMapper<Event> Resolve(Property targetProperty, string _) => PropertyMappers.FromEventValueProvider(targetProperty, EventValueProviders.FromEventSourceId);
    }
}
