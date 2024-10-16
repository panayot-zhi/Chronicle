// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Events;
using Cratis.Chronicle.Reactors;

namespace Cratis.Chronicle.Integration.Orleans.InProcess.for_Observers.when_appending_event;

[DependencyInjection.IgnoreConvention]
public class SomeReactor(TaskCompletionSource tsc) : IReactor
{
    public int HandledEvents;

    public Task OnSomeEvent(SomeEvent evt, EventContext ctx)
    {
        Interlocked.Increment(ref HandledEvents);
        tsc.SetResult();
        return Task.CompletedTask;
    }
}
