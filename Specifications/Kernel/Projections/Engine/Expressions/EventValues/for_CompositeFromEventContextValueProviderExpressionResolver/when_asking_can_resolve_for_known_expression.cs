// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Aksio.Cratis.Events.Projections.Expressions.EventValues.for_CompositeFromEventContextValueProviderExpressionResolver;

public class when_asking_can_resolve_for_regular_property : Specification
{
    CompositeValueProviderExpressionResolver resolvers;
    bool result;

    void Establish() => resolvers = new();

    void Because() => result = resolvers.CanResolve("$compositeFromContext(occurred.ear, occurred.month, occurred.day)");

    [Fact] void should_be_able_to_resolve() => result.ShouldBeTrue();
}
