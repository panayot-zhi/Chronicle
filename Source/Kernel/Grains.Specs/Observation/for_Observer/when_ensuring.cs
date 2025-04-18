// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Grains.Observation.for_Observer;

public class when_ensuring : given.an_observer
{
    static Exception error;
    async Task Because() => error = await Catch.Exception(_observer.Ensure);

    [Fact] void should_not_fail() => error.ShouldBeNull();
    [Fact] void should_not_write_state() => _storageStats.Writes.ShouldEqual(0);
}