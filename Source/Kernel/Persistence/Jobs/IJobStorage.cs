// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Aksio.Cratis.Kernel.Grains.Jobs;

namespace Aksio.Cratis.Kernel.Persistence.Jobs;

/// <summary>
/// Defines a system that can store job state.
/// </summary>
/// <typeparam name="TJobState">Type of <see cref="JobState{T}"/> to store.</typeparam>
public interface IJobStorage<TJobState>
{
    /// <summary>
    /// Read the state of a job.
    /// </summary>
    /// <param name="jobId">The <see cref="JobId"/> of the job to read.</param>
    /// <returns>Job state instance or null if it doesn't exist.</returns>
    Task<TJobState?> Read(JobId jobId);

    /// <summary>
    /// Save the state of a job.
    /// </summary>
    /// <param name="jobId">The <see cref="JobId"/> of the job to save.</param>
    /// <param name="state"><see cref="JobState{T}"/> to save.</param>
    /// <returns>Awaitable task.</returns>
    Task Save(JobId jobId, TJobState state);

    /// <summary>
    /// Remove a job.
    /// </summary>
    /// <param name="jobId">The <see cref="JobId"/> of the job to remove.</param>
    /// <returns>Awaitable task.</returns>
    Task Remove(JobId jobId);

    /// <summary>
    /// Get all jobs of a given type with a given status.
    /// </summary>
    /// <typeparam name="TJobType">Type of job to get for.</typeparam>
    /// <param name="statuses">Optional params of <see cref="JobStatus"/> to filter on.</param>
    /// <returns>A collection of job state objects.</returns>
    /// <remarks>
    /// If no job statuses are specified, all jobs of the given type will be returned.
    /// </remarks>
    Task<IImmutableList<TJobState>> GetJobs<TJobType>(params JobStatus[] statuses);
}
