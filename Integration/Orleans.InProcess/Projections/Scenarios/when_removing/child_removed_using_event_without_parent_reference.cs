// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Events;
using Cratis.Chronicle.Integration.Base;
using Cratis.Chronicle.Integration.Orleans.InProcess.Projections.Events;
using Cratis.Chronicle.Integration.Orleans.InProcess.Projections.Scenarios.Models;
using context = Cratis.Chronicle.Integration.Orleans.InProcess.Projections.Scenarios.when_removing.child_removed_using_event_without_parent_reference.context;

namespace Cratis.Chronicle.Integration.Orleans.InProcess.Projections.Scenarios.when_removing;

[Collection(GlobalCollection.Name)]
public class child_removed_using_event_without_parent_reference(context context) : Given<context>(context)
{
    public class context(GlobalFixture globalFixture) : given.a_projection_and_events_appended_to_it<UserProjectionWithExternalRemovedWith, User>(globalFixture)
    {
        public EventSourceId UserId;
        public EventSourceId FirstGroupId;
        public EventSourceId SecondGroupId;
        public override IEnumerable<Type> EventTypes => [typeof(GroupCreated), typeof(UserCreated), typeof(UserAddedToGroup), typeof(GroupRemoved)];

        void Establish()
        {
            var userId = (EventSourceId)Guid.NewGuid();
            FirstGroupId = "449670b7-120c-4978-ba2e-8fbb12ff4fbc";
            SecondGroupId = "be7f5b19-8df3-4049-bb9c-78fb2fdf5cce";
            EventSourceId = "162784c6-6b64-4e0a-8710-191bc5a57788";
            ModelId = userId;

            EventsWithEventSourceIdToAppend.Add(new(FirstGroupId, new GroupCreated("SomeGroup")));
            EventsWithEventSourceIdToAppend.Add(new(SecondGroupId, new GroupCreated("SomeOtherGroup")));
            EventsWithEventSourceIdToAppend.Add(new(userId, new UserCreated("Someone")));
            EventsWithEventSourceIdToAppend.Add(new(FirstGroupId, new UserAddedToGroup(userId)));
            EventsWithEventSourceIdToAppend.Add(new(SecondGroupId, new UserAddedToGroup(userId)));
            EventsWithEventSourceIdToAppend.Add(new(FirstGroupId, new GroupRemoved()));
        }
    }

    [Fact] void should_return_model() => Context.Result.ShouldNotBeNull();
    [Fact] void should_only_have_one_child() => Context.Result.Groups.Count().ShouldEqual(1);
    [Fact] void should_have_the_correct_group_left() => Context.Result.Groups.First().GroupId.ShouldEqual(Context.SecondGroupId);
    [Fact] void should_not_have_the_removed_group() => Context.Result.Groups.Any(_ => _.GroupId == Context.FirstGroupId).ShouldBeFalse();
}
