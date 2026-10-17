using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Agents.Governance.Tests.Application;

[TestClass]
public sealed class RepeatRequestTests
{
    [TestMethod]
    public void CreateTrimsSource()
    {
        var request = RepeatRequest.Create(RepeatMode.Replay, "  run-1  ");

        Assert.AreEqual("run-1", request.SourceExecutionId);
    }

    [TestMethod]
    public void CreateWithGuidFormatsSourceAsD()
    {
        var id = Guid.NewGuid();

        var request = RepeatRequest.Create(RepeatMode.Recall, id);

        Assert.AreEqual(id.ToString("D"), request.SourceExecutionId);
    }

    [TestMethod]
    public void CreateRerunWithNullGuidHasNoSource()
    {
        var request = RepeatRequest.Create(RepeatMode.Rerun, (Guid?)null);

        Assert.AreEqual(RepeatMode.Rerun, request.Mode);
        Assert.IsNull(request.SourceExecutionId);
    }

    [TestMethod]
    public void RerunFactoryMatchesCreateRerun()
    {
        Assert.AreEqual(RepeatRequest.Create(RepeatMode.Rerun), RepeatRequest.Rerun());
    }

    [TestMethod]
    public void ModeOrdinalsAreStable()
    {
        Assert.AreEqual(0, (int)RepeatMode.Rerun);
        Assert.AreEqual(1, (int)RepeatMode.Recall);
        Assert.AreEqual(2, (int)RepeatMode.Replay);
    }
}

[TestClass]
public sealed class RepeatActionModelTests
{
    [TestMethod]
    public void RunsReportsOnlyPlannedActions()
    {
        var plan = new RepeatPlan(RepeatMode.Replay, ActionType.Evaluate | ActionType.Record);

        Assert.IsFalse(plan.Runs(ActionType.Collect));
        Assert.IsTrue(plan.Runs(ActionType.Evaluate));
        Assert.IsTrue(plan.Runs(ActionType.Record));
        Assert.IsTrue(plan.Runs(ActionType.Evaluate | ActionType.Record));
        Assert.IsFalse(plan.Runs(ActionType.All));
    }

    [TestMethod]
    public void AllAndNoneAreOpposites()
    {
        var singleBitFlags = Enum.GetValues<ActionType>()
            .Where(flag => flag != ActionType.None && flag != ActionType.All)
            .Where(flag =>
            {
                var value = (int)flag;
                return (value & (value - 1)) == 0;
            })
            .ToArray();

        var combinedSingleBitFlags = singleBitFlags.Aggregate(ActionType.None, (current, next) => current | next);

        Assert.AreEqual(ActionType.All, combinedSingleBitFlags);
        Assert.IsTrue(singleBitFlags.All(flag => !ActionType.None.HasFlag(flag)));
    }

    [TestMethod]
    public void RecallPlanRecordsOnlyWhenOptedIn()
    {
        var request = RepeatRequest.Create(RepeatMode.Recall, "s");

        Assert.AreEqual(ActionType.None, new DefaultRepeatPolicy().Plan(request).Run);
        Assert.AreEqual(ActionType.Record,
            new DefaultRepeatPolicy(new RepeatabilityOptions { RecallRecords = true }).Plan(request).Run);
    }

    [TestMethod]
    public void PolicyRejectsNullRequest()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultRepeatPolicy().Plan(null!));
    }
}

[TestClass]
public sealed class RepeatRecorderLineageTests
{
    [TestMethod]
    public void RecordingTwiceKeepsTheLatestRepeat()
    {
        var recorder = new DefaultRepeatRecorder();
        var first = recorder.Record(new EvaluationGovernanceRecord(), RepeatRequest.Create(RepeatMode.Replay, "a"), ActionType.All);

        var second = recorder.Record(first, RepeatRequest.Create(RepeatMode.Recall, "b"), ActionType.None);

        Assert.AreEqual(RepeatMode.Recall, second.Repeatability.ReplayMode);
        Assert.AreEqual("b", second.Repeatability.SourceExecutionRef?.Value);
        Assert.AreEqual(ActionType.None, second.Repeatability.ActionsRun);
    }

    [TestMethod]
    public void RecordDoesNotMutateTheInput()
    {
        var input = new EvaluationGovernanceRecord();

        new DefaultRepeatRecorder().Record(input, RepeatRequest.Create(RepeatMode.Replay, "a"), ActionType.All);

        Assert.AreEqual(RepeatMode.Rerun, input.Repeatability.ReplayMode);
        Assert.IsNull(input.Repeatability.SourceExecutionRef);
    }

    [TestMethod]
    public void RecorderRejectsNulls()
    {
        var recorder = new DefaultRepeatRecorder();

        Assert.Throws<ArgumentNullException>(() => recorder.Record(null!, RepeatRequest.Rerun(), ActionType.All));
        Assert.Throws<ArgumentNullException>(() => recorder.Record(new EvaluationGovernanceRecord(), null!, ActionType.All));
    }
}

[TestClass]
public sealed class RepeatRegistrationTests
{
    [TestMethod]
    public void AppRegisteredPolicyIsNotOverridden()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRepeatPolicy, StrictPolicy>();
        services.AddGovernanceRepeatability();
        using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<StrictPolicy>(provider.GetRequiredService<IRepeatPolicy>());
    }

    private sealed class StrictPolicy : IRepeatPolicy
    {
        public RepeatPlan Plan(RepeatRequest request) => new(request.Mode, ActionType.All);
    }
}
