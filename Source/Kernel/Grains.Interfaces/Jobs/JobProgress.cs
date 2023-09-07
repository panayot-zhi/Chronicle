// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Aksio.Cratis.Kernel.Grains.Jobs;

/// <summary>
/// Represents progress of a job.
/// </summary>
public class JobProgress
{
    /// <summary>
    /// Gets or sets the total number of steps.
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// Gets or sets the completed number of steps.
    /// </summary>
    public int CompletedSteps { get; set; }

    /// <summary>
    /// Gets or sets the failed number of steps.
    /// </summary>
    public int FailedSteps { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="JobProgressMessage"/> associated with the progress.
    /// </summary>
    public JobProgressMessage Message { get; set; } = JobProgressMessage.None;
}
