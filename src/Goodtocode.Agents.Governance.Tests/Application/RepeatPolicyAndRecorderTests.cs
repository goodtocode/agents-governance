using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Tests.Application;

[TestClass]
public sealed class RepeatPolicyTests
{
    [TestMethod]
    [DataRow(RepeatMode.Rerun, false, true, true, true)]
    [DataRow(RepeatMode.Replay, false, false, true, true)]
    [DataRow(RepeatMode.Recall, false, false, false, false)]
    [DataRow(RepeatMode.Recall, true, false, false, true)]
    public void PlanRunsExpectedActions(RepeatMode mode, bool recallRecords, bool collect, bool evaluate, bool record)
    {
        var policy = new DefaultRepeatPolicy(new RepeatabilityOptions { RecallRecords = recallRecords });
        var request = mode == RepeatMode.Rerun ? RepeatRequest.Rerun() : RepeatRequest.Create(mode, "source");

        var plan = policy.Plan(request);

        Assert.AreEqual(new RepeatPlan(mode, (collect ? ActionType.Collect : 0) | (evaluate ? ActionType.Evaluate : 0) | (record ? ActionType.Record : 0)), plan);
    }
}

[TestClass]
public sealed class RepeatRecorderTests
{
    private readonly DefaultRepeatRecorder _recorder = new();

    [TestMethod]
    public void RecordsModeSourceAndActionsRun()
    {
        var actions = ActionType.Evaluate | ActionType.Record;

        var result = _recorder.Record(new EvaluationGovernanceRecord(), RepeatRequest.Create(RepeatMode.Replay, "source-1"), actions);

        Assert.AreEqual(RepeatMode.Replay, result.Repeatability.ReplayMode);
        Assert.AreEqual("source-1", result.Repeatability.SourceExecutionRef?.Value);
        Assert.AreEqual(actions, result.Repeatability.ActionsRun);
    }

    [TestMethod]
    public void RerunRecordsNoSource()
    {
        var result = _recorder.Record(new EvaluationGovernanceRecord(), RepeatRequest.Rerun(), ActionType.All);

        Assert.AreEqual(RepeatMode.Rerun, result.Repeatability.ReplayMode);
        Assert.IsNull(result.Repeatability.SourceExecutionRef);
    }

    [TestMethod]
    public void PreservesOtherRecordData()
    {
        var input = new EvaluationGovernanceRecord { PolicyProfileVersion = "v1" };

        var result = _recorder.Record(input, RepeatRequest.Rerun(), ActionType.All);

        Assert.AreEqual("v1", result.PolicyProfileVersion);
    }
}
