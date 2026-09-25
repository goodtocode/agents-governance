using Goodtocode.Agents.Governance.Application;

namespace Goodtocode.Agents.Governance.Tests.Application;

[TestClass]
public sealed class GovernanceReplayGuardTests
{
    [TestMethod]
    public void EnsureExactReplayWithMatchingSnapshotsDoesNotThrow()
    {
        // Arrange
        var baseline = new GovernanceReplaySnapshot(
            PolicyProfileVersion: "ai-assurance.v1",
            ModelRef: "model://gpt/governed",
            ModelVersion: "1.0.0",
            PromptHash: "PROMPT-HASH-001",
            InputHash: "INPUT-HASH-001");

        var current = baseline with { };

        // Act
        Action action = () => GovernanceReplayGuard.EnsureExactReplay(baseline, current);

        // Assert
        action();
    }

    [TestMethod]
    public void EnsureExactReplayWithPromptHashMismatchThrowsInvalidOperationException()
    {
        // Arrange
        var baseline = new GovernanceReplaySnapshot(
            PolicyProfileVersion: "ai-assurance.v1",
            ModelRef: "model://gpt/governed",
            ModelVersion: "1.0.0",
            PromptHash: "PROMPT-HASH-001",
            InputHash: "INPUT-HASH-001");

        var current = baseline with { PromptHash = "PROMPT-HASH-999" };

        // Act
        Action action = () => GovernanceReplayGuard.EnsureExactReplay(baseline, current);

        // Assert
        InvalidOperationException? exception = null;
        try
        {
            action();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void EnsureExactReplayWithOutputHashMismatchThrowsInvalidOperationException()
    {
        // Arrange
        var baseline = new GovernanceReplaySnapshot(
            PolicyProfileVersion: "ai-assurance.v1",
            ModelRef: "model://gpt/governed",
            ModelVersion: "1.0.0",
            PromptHash: "PROMPT-HASH-001",
            InputHash: "INPUT-HASH-001",
            OutputHash: "OUTPUT-HASH-001");

        var current = baseline with { OutputHash = "OUTPUT-HASH-999" };

        // Act
        Action action = () => GovernanceReplayGuard.EnsureExactReplay(baseline, current);

        // Assert
        InvalidOperationException? exception = null;
        try
        {
            action();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void EnsureExactReplayWithEmptyOutputHashesDoesNotThrow()
    {
        // Arrange
        var baseline = new GovernanceReplaySnapshot(
            PolicyProfileVersion: "ai-assurance.v1",
            ModelRef: "model://gpt/governed",
            ModelVersion: "1.0.0",
            PromptHash: "PROMPT-HASH-001",
            InputHash: "INPUT-HASH-001");

        var current = baseline with { };

        // Act
        Action action = () => GovernanceReplayGuard.EnsureExactReplay(baseline, current);

        // Assert
        action();
    }

    [TestMethod]
    public void CompareRerunReportsDivergenceWithoutThrowing()
    {
        // Arrange
        var baseline = new GovernanceReplaySnapshot(
            PolicyProfileVersion: "ai-assurance.v1",
            ModelRef: "model://gpt/governed",
            ModelVersion: "1.0.0",
            PromptHash: "PROMPT-HASH-001",
            InputHash: "INPUT-HASH-001",
            OutputHash: "OUTPUT-HASH-001");

        var current = baseline with { InputHash = "INPUT-HASH-002", OutputHash = "OUTPUT-HASH-002" };

        // Act
        var comparison = GovernanceReplayGuard.CompareRerun(baseline, current);

        // Assert
        Assert.IsFalse(comparison.IsIdenticalToBaseline);
        Assert.IsTrue(comparison.InputChanged);
        Assert.IsTrue(comparison.OutputChanged);
        Assert.IsFalse(comparison.ModelReferenceChanged);
        Assert.IsFalse(comparison.ModelVersionChanged);
        Assert.IsFalse(comparison.PromptChanged);
    }

    [TestMethod]
    public void CompareRerunReportsIdenticalBaselineWhenNothingChanged()
    {
        // Arrange
        var baseline = new GovernanceReplaySnapshot(
            PolicyProfileVersion: "ai-assurance.v1",
            ModelRef: "model://gpt/governed",
            ModelVersion: "1.0.0",
            PromptHash: "PROMPT-HASH-001",
            InputHash: "INPUT-HASH-001",
            OutputHash: "OUTPUT-HASH-001");

        var current = baseline with { };

        // Act
        var comparison = GovernanceReplayGuard.CompareRerun(baseline, current);

        // Assert
        Assert.IsTrue(comparison.IsIdenticalToBaseline);
    }
}
