using Goodtocode.Agents.Governance.Application;

namespace Goodtocode.Agents.Governance.Tests.Application;

[TestClass]
public sealed class GovernanceEnforcerTests
{
    [TestMethod]
    public void EnforceWithValidRequestReturnsGovernedResult()
    {
        // Arrange
        var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer());
        var request = new GovernanceEvaluationRequest
        {
            Governance = TestDataFactory.CreateValidGovernanceRecord(),
            Instruction = AgentInstruction.Create("Use strict governance.")
        };

        // Act
        var result = enforcer.Enforce(request);

        // Assert
        Assert.AreEqual(request.Governance.PolicyProfileVersion, result.Governance.PolicyProfileVersion);
        Assert.IsTrue(result.PromptContext.SystemInstruction.Contains("Use strict governance.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void EnforceWithInvalidRequestThrowsValidationException()
    {
        // Arrange
        var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer());
        var request = new GovernanceEvaluationRequest
        {
            Governance = new EvaluationGovernanceRecord()
        };

        // Act
        Action action = () => enforcer.Enforce(request);

        // Assert
        GovernanceValidationException? exception = null;
        try
        {
            action();
        }
        catch (GovernanceValidationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        Assert.IsNotEmpty(exception.Issues);
    }

    [TestMethod]
    public void EnforceWithRepeatabilitySourcesComputesHashesFromRawValues()
    {
        // Arrange
        var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer());
        var request = new GovernanceEvaluationRequest
        {
            Governance = TestDataFactory.CreateValidGovernanceRecord() with
            {
                Repeatability = TestDataFactory.CreateValidGovernanceRecord().Repeatability with
                {
                    PromptHash = string.Empty,
                    InputHash = string.Empty
                }
            },
            RepeatabilityPromptContent = "workflow-json-content",
            RepeatabilityInputs = new Dictionary<string, object?>
            {
                ["inputA"] = new { value = 10 },
                ["inputB"] = "abc"
            }
        };

        // Act
        var result = enforcer.Enforce(request);

        // Assert
        Assert.IsNotEmpty(result.PromptHash);
        Assert.IsNotEmpty(result.InputHash);
        Assert.AreEqual(result.Governance.Repeatability.PromptHash, result.PromptHash);
        Assert.AreEqual(result.Governance.Repeatability.InputHash, result.InputHash);
    }

    [TestMethod]
    public void EnforceWithSameSemanticInputsDifferentOrderComputesSameInputHash()
    {
        // Arrange
        var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer());
        var baselineGovernance = TestDataFactory.CreateValidGovernanceRecord() with
        {
            Repeatability = TestDataFactory.CreateValidGovernanceRecord().Repeatability with
            {
                PromptHash = string.Empty,
                InputHash = string.Empty
            }
        };

        var first = new GovernanceEvaluationRequest
        {
            Governance = baselineGovernance,
            RepeatabilityPromptContent = "same-prompt",
            RepeatabilityInputs = new Dictionary<string, object?>
            {
                ["b"] = new Dictionary<string, object?> { ["y"] = "two", ["x"] = 2 },
                ["a"] = new Dictionary<string, object?> { ["m"] = 1, ["n"] = "one" }
            }
        };

        var second = new GovernanceEvaluationRequest
        {
            Governance = baselineGovernance,
            RepeatabilityPromptContent = "same-prompt",
            RepeatabilityInputs = new Dictionary<string, object?>
            {
                ["a"] = new Dictionary<string, object?> { ["n"] = "one", ["m"] = 1 },
                ["b"] = new Dictionary<string, object?> { ["x"] = 2, ["y"] = "two" }
            }
        };

        // Act
        var firstResult = enforcer.Enforce(first);
        var secondResult = enforcer.Enforce(second);

        // Assert
        Assert.AreEqual(firstResult.InputHash, secondResult.InputHash);
        Assert.AreEqual(firstResult.PromptHash, secondResult.PromptHash);
    }

    [TestMethod]
    public void EnforceWithEmptyRawInputsStillComputesNonEmptyInputHash()
    {
        // Arrange
        // Regression test: a workflow with zero inputs (e.g. an input-less playbook) must not be
        // treated the same as "no raw source provided". Hashing must always run unconditionally.
        var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer());
        var request = new GovernanceEvaluationRequest
        {
            Governance = TestDataFactory.CreateValidGovernanceRecord() with
            {
                Repeatability = TestDataFactory.CreateValidGovernanceRecord().Repeatability with
                {
                    PromptHash = string.Empty,
                    InputHash = string.Empty
                }
            },
            RepeatabilityPromptContent = "workflow-json-content",
            RepeatabilityInputs = new Dictionary<string, object?>()
        };

        // Act
        var result = enforcer.Enforce(request);

        // Assert
        Assert.IsNotEmpty(result.InputHash);
        Assert.IsNotEmpty(result.PromptHash);
    }

    [TestMethod]
    public void EnforceWithEmptyRawPromptContentStillComputesNonEmptyPromptHash()
    {
        // Arrange
        var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer());
        var request = new GovernanceEvaluationRequest
        {
            Governance = TestDataFactory.CreateValidGovernanceRecord() with
            {
                Repeatability = TestDataFactory.CreateValidGovernanceRecord().Repeatability with
                {
                    PromptHash = string.Empty,
                    InputHash = string.Empty
                }
            },
            RepeatabilityPromptContent = string.Empty,
            RepeatabilityInputs = new Dictionary<string, object?> { ["a"] = 1 }
        };

        // Act
        var result = enforcer.Enforce(request);

        // Assert
        Assert.IsNotEmpty(result.PromptHash);
        Assert.IsNotEmpty(result.InputHash);
    }

    [TestMethod]
    public void EnforceWithNullRawPromptContentAndDefaultInputsStillProducesValidHashes()
    {
        // Arrange
        // Regression test mirroring the exact Cannery scenario: RepeatabilityPromptContent left null
        // and RepeatabilityInputs left at its default empty dictionary (no raw sources supplied at all).
        var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer());
        var request = new GovernanceEvaluationRequest
        {
            Governance = TestDataFactory.CreateValidGovernanceRecord() with
            {
                Repeatability = TestDataFactory.CreateValidGovernanceRecord().Repeatability with
                {
                    PromptHash = string.Empty,
                    InputHash = string.Empty
                }
            }
        };

        // Act
        var result = enforcer.Enforce(request);

        // Assert
        Assert.IsNotEmpty(result.PromptHash);
        Assert.IsNotEmpty(result.InputHash);
    }

    [TestMethod]
    public void EnforceWithCustomHashStrategyUsesSuppliedStrategy()
    {
        // Arrange
        var strategy = new StubHashStrategy();
        var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer(), strategy);
        var request = new GovernanceEvaluationRequest
        {
            Governance = TestDataFactory.CreateValidGovernanceRecord() with
            {
                Repeatability = TestDataFactory.CreateValidGovernanceRecord().Repeatability with
                {
                    PromptHash = string.Empty,
                    InputHash = string.Empty
                }
            },
            RepeatabilityPromptContent = "custom-strategy-prompt",
            RepeatabilityInputs = new Dictionary<string, object?> { ["a"] = 1 }
        };

        // Act
        var result = enforcer.Enforce(request);

        // Assert
        Assert.AreEqual("stub-prompt-hash", result.PromptHash);
        Assert.AreEqual("stub-input-hash", result.InputHash);
    }

    private sealed class StubHashStrategy : IRepeatabilityHashStrategy
    {
        public string ComputePromptHash(string promptContent) => "stub-prompt-hash";

        public string ComputeInputHash(IReadOnlyDictionary<string, object?> inputs) => "stub-input-hash";
    }
}
