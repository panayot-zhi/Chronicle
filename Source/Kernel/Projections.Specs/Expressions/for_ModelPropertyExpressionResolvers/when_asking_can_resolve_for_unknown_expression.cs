// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Projections.Expressions.for_ModelPropertyExpressionResolvers;

public class when_asking_can_resolve_for_unknown_expression : given.model_property_expression_resolvers
{
    bool result;

    void Because() => result = _resolvers.CanResolve(string.Empty, "$randomUnknownExpression");

    [Fact] void should_not_be_able_to_resolve() => result.ShouldBeFalse();
}
