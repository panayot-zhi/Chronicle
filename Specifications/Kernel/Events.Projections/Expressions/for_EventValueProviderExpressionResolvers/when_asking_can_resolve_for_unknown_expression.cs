// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Events.Projections.Expressions.for_EventValueProviderExpressionResolvers
{
    public class when_asking_can_resolve_for_unknown_expression : Specification
    {
        EventValueProviderExpressionResolvers resolvers;
        bool result;

        void Establish() => resolvers = new EventValueProviderExpressionResolvers();

        void Because() => result = resolvers.CanResolve("$randomUnknownExpression");

        [Fact] void should_not_be_able_to_resolve() => result.ShouldBeFalse();
    }

    public class when_trying_to_resolve_unknown_expression : Specification
    {
        EventValueProviderExpressionResolvers resolvers;
        Exception result;

        void Establish() => resolvers = new EventValueProviderExpressionResolvers();

        void Because() => result = Catch.Exception(() => resolvers.Resolve("$randomUnknownExpression"));

        [Fact] void should_throw_unsupported_event_value_expression() => result.ShouldBeOfExactType<UnsupportedEventValueExpression>();
    }
}
