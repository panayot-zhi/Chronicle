// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Metrics;

namespace Cratis.Chronicle.Grains.Workers;

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1402 // File may only contain a single type

internal static partial class LimitedConcurrencyLevelTaskSchedulerMetrics
{
    [Gauge<int>("chronicle-running-tasks", "Number of running tasks")]
    internal static partial void RunningTasks(this IMeter<LimitedConcurrencyLevelTaskScheduler> meter, int numberOfTasks);

    [Gauge<int>("chronicle-queued-tasks", "Number of queued tasks")]
    internal static partial void QueuedTasks(this IMeter<LimitedConcurrencyLevelTaskScheduler> meter, int numberOfTasks);
}
