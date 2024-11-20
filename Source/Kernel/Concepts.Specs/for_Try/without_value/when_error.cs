// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Concepts.for_Try.without_value;

public class when_error : Specification
{
    static SafeTry result;
    static Exception error;

    void Establish() => error = new Exception();

    void Because() => result = SafeTry.Failed(error);

    [Fact] void should_not_be_success() => result.IsSuccess.ShouldBeFalse();
    [Fact] void should_try_get_error() => result.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_have_the_error() => result.Match<object>(_ => _, errorType => error).ShouldEqual(error);
}
