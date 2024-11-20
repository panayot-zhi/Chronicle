// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Concepts.Auditing;
using Cratis.Chronicle.Concepts.Identities;
using Cratis.Chronicle.Storage;

namespace Cratis.Chronicle.Concepts.for_Try.with_value;

public class when_error : Specification
{
    static Try<TheErrorType> result;
    static TheErrorType error;

    void Establish() => error = TheErrorType.SomeType;

    void Because() => result = Try<TheErrorType>.Failed(error);

    [Fact] void should_not_be_success() => result.IsSuccess.ShouldBeFalse();
    [Fact] void should_try_get_error() => result.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_have_the_error() => result.Match<object>(_ => _, errorType => error).ShouldEqual(error);
}
