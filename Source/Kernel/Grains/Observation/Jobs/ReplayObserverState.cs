using Cratis.Chronicle.Concepts.Events;
using Cratis.Chronicle.Storage.Jobs;
namespace Cratis.Chronicle.Grains.Observation.Jobs;

/// <summary>
/// Represents the state for a <see cref="ReplayObserver"/> job.
/// </summary>
public class ReplayObserverState : JobState
{
    /// <summary>
    /// Gets or sets the number of new handled events by the job.
    /// </summary>
    public EventCount NewHandledCount { get; set; } = EventCount.Zero;

    /// <summary>
    /// Gets or sets the event sequence number of the last handled event.
    /// </summary>
    public EventSequenceNumber LastHandledEventSequenceNumber { get; set; } = EventSequenceNumber.Unavailable;
}