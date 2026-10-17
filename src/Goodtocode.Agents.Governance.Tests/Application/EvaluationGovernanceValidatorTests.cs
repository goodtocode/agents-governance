using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Tests.Application;

[TestClass]
public sealed class EvaluationGovernanceValidatorTests
{
    [TestMethod]
    public void ValidateWithCompleteGovernanceRecordReturnsValidResult()
    {
        // Arrange
        var record = TestDataFactory.CreateValidGovernanceRecord();

        // Act
        var result = EvaluationGovernanceValidator.Validate(record);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Issues);
    }

    [TestMethod]
    public void ValidateWithIncompleteGovernanceRecordReturnsIssues()
    {
        // Arrange
        var record = new EvaluationGovernanceRecord();

        // Act
        var result = EvaluationGovernanceValidator.Validate(record);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotEmpty(result.Issues);
        Assert.IsTrue(result.Issues.Any(x => x.Field == nameof(EvaluationGovernanceRecord.PolicyProfileVersion)));
    }

    [TestMethod]
    public void ValidateWithRerunReplayModeDoesNotRequireSourceExecutionRef()
    {
        // Arrange
        var record = TestDataFactory.CreateValidGovernanceRecord();

        // Act
        var result = EvaluationGovernanceValidator.Validate(record);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    [DataRow(RepeatMode.Recall)]
    [DataRow(RepeatMode.Replay)]
    public void ValidateWithRecallOrReplayModeRequiresSourceExecutionRef(RepeatMode mode)
    {
        // Arrange
        var record = TestDataFactory.CreateValidGovernanceRecord() with
        {
            Repeatability = TestDataFactory.CreateValidGovernanceRecord().Repeatability with
            {
                ReplayMode = mode,
                SourceExecutionRef = null
            }
        };

        // Act
        var result = EvaluationGovernanceValidator.Validate(record);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Issues.Any(x => x.Field == nameof(RepeatabilityRecord.SourceExecutionRef)));
    }

    [TestMethod]
    [DataRow(RepeatMode.Recall)]
    [DataRow(RepeatMode.Replay)]
    public void ValidateWithRecallOrReplayModeAndSourceExecutionRefIsValid(RepeatMode mode)
    {
        // Arrange
        var record = TestDataFactory.CreateValidGovernanceRecord() with
        {
            Repeatability = TestDataFactory.CreateValidGovernanceRecord().Repeatability with
            {
                ReplayMode = mode,
                SourceExecutionRef = GovernanceReference.Parse("execution://prior/001")
            }
        };

        // Act
        var result = EvaluationGovernanceValidator.Validate(record);

        // Assert
        Assert.IsTrue(result.IsValid);
    }
}
