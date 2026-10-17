using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Agents.Governance.Tests.Application;

[TestClass]
public sealed class RepeatGuardAndRequestTests
{
    [TestMethod]
    [DataRow(RepeatMode.Rerun, "x")]
    [DataRow(RepeatMode.Recall, null)]
    [DataRow(RepeatMode.Replay, " ")]
    public void CreateWithInvalidSourceThrowsValidation(RepeatMode mode, string? source)
    {
        var ex = Assert.Throws<RepeatValidationException>(() => RepeatRequest.Create(mode, source));

        Assert.AreEqual(RepeatFailureCategory.Validation, ex.Category);
    }

    [TestMethod]
    public void CreateWithUndefinedModeThrowsValidation()
    {
        Assert.Throws<RepeatValidationException>(() => RepeatRequest.Create((RepeatMode)99, "x"));
    }

    [TestMethod]
    public void GuardsMapToStandardCategories()
    {
        Assert.AreEqual(RepeatFailureCategory.NotFound,
            Assert.Throws<RepeatSourceNotFoundException>(() => RepeatGuard.AgainstSourceNotFound(false, "s")).Category);
        Assert.AreEqual(RepeatFailureCategory.Conflict,
            Assert.Throws<RepeatConflictException>(() => RepeatGuard.AgainstSourceNotCompleted(false, "s")).Category);
        Assert.AreEqual(RepeatFailureCategory.Conflict,
            Assert.Throws<RepeatConflictException>(() => RepeatGuard.AgainstSourceNotReplayCapable(RepeatMode.Replay, false, "s")).Category);
        Assert.AreEqual(RepeatFailureCategory.Forbidden,
            Assert.Throws<RepeatForbiddenException>(() => RepeatGuard.AgainstForbidden(false, "s")).Category);
        RepeatGuard.AgainstSourceNotReplayCapable(RepeatMode.Recall, false, "s");
    }

    [TestMethod]
    public void AddGovernanceRepeatabilityRegistersDefaultsAndHonorsOptions()
    {
        var services = new ServiceCollection();
        services.AddGovernanceRepeatability(o => o.RecallRecords = true);
        using var provider = services.BuildServiceProvider();

        var plan = provider.GetRequiredService<IRepeatPolicy>().Plan(RepeatRequest.Create(RepeatMode.Recall, "s"));

        Assert.IsTrue(plan.Runs(ActionType.Record));
        Assert.IsInstanceOfType<DefaultRepeatRecorder>(provider.GetRequiredService<IRepeatRecorder>());
    }

    [TestMethod]
    public void AddGovernanceRepeatabilityRejectsNullRecallRecordsOnStart()
    {
        var services = new ServiceCollection();
        services.AddGovernanceRepeatability(o => o.RecallRecords = null);
        using var provider = services.BuildServiceProvider();

        Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(
            () => provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RepeatabilityOptions>>().Value);
    }
}
