// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Cratis.Chronicle.Concepts;
using Cratis.Chronicle.Concepts.Jobs;
using Cratis.Chronicle.Grains.Jobs;
using Cratis.Chronicle.Integration.Base;
using Cratis.Chronicle.Integration.Orleans.InProcess.for_JobsManager.given;
using Cratis.Chronicle.Storage.Jobs;
using context = Cratis.Chronicle.Integration.Orleans.InProcess.for_JobsManager.when_starting.job_with_single_step.and_then_resumed.context;

namespace Cratis.Chronicle.Integration.Orleans.InProcess.for_JobsManager.when_starting.job_with_single_step;

[Collection(GlobalCollection.Name)]
public class and_then_resumed(context context) : Given<context>(context)
{
    public class context(GlobalFixture globalFixture) : given.a_jobs_manager(globalFixture)
    {
        public Result<JobId, StartJobError> StartJobResult;
        public JobState CompletedJobState;
        public JobId JobId;
        public IImmutableList<JobStepState> JobStepStates;

        async Task Because()
        {
            var taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TheJobStepProcessor.SetStartTask(taskCompletionSource.Task);
            StartJobResult = await JobsManager.Start<IJobWithSingleStep, JobWithSingleStepRequest>(new() { KeepAfterCompleted = true });
            JobId = StartJobResult.AsT0;
            await TheJobStepProcessor.WaitForAllPreparedStepsToBeStarted();
            await JobsManager.Resume(JobId);
            taskCompletionSource.SetResult();
            CompletedJobState = await JobStorage.WaitTillJobMeetsPredicate<JobWithSingleStepState>(JobId, state => state.Status == JobStatus.CompletedSuccessfully);
            var getJobStepState = await JobStepStorage.GetForJob(JobId);
            JobStepStates = getJobStepState.AsT0;
        }
    }

    [Fact]
    public void should_start_job() => Context.StartJobResult.IsSuccess.ShouldBeTrue();

    [Fact]
    public void should_have_correct_job_type() => Context.CompletedJobState.Type.Value.ShouldEqual(nameof(JobWithSingleStep));

    [Fact]
    public void should_not_keep_any_job_step_states_after_completed() => Context.JobStepStates.ShouldBeEmpty();

    [Fact]
    public void should_have_completed_job_successfully() => Context.CompletedJobState.Status.ShouldEqual(JobStatus.CompletedSuccessfully);

    [Fact]
    public void should_have_completed_job_progress() => Context.CompletedJobState.Progress.IsCompleted.ShouldBeTrue();

    [Fact]
    public void should_have_job_progress_with_one_successful_step() => Context.CompletedJobState.Progress.SuccessfulSteps.ShouldEqual(1);

    [Fact]
    public void should_perform_work_for_job_step_only_once() => Context.TheJobStepProcessor.ShouldHavePerformedJobStepCalls(Context.JobId, 1);

    [Fact]
    public void should_have_completed_work_successfully_for_one_job_step() => Context.TheJobStepProcessor.ShouldHaveCompletedJobSteps(Context.JobId, JobStepStatus.CompletedSuccessfully, 1);
}
