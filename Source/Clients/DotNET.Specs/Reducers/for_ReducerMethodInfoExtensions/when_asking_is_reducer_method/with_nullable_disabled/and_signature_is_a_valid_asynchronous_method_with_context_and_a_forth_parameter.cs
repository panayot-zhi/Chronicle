// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using Cratis.Chronicle.Events;

#nullable disable

namespace Cratis.Chronicle.Reducers.for_ReducerMethodInfoExtensions.when_asking_is_reducer_method.with_nullable_disabled;

public class and_signature_is_a_valid_asynchronous_method_with_context_and_a_forth_parameter : Specification
{
    class MyReducer : IReducerFor<ReadModel>
    {
        public ReducerId Id => "55753433-5bbd-4a79-91b6-7b6231c52183";

        public Task<ReadModel> Something(ValidEvent @event, ReadModel current, EventContext eventContext, string something) => Task.FromResult(current);
    }

    bool result;

    void Because() => result = typeof(MyReducer).GetMethod(nameof(MyReducer.Something)).IsReducerMethod(typeof(ReadModel), []);

    [Fact] void should_not_be_considered_a_reducer_method() => result.ShouldBeFalse();
}
