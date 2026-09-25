# Agent Governance - Sprint 0 Context Diagram

## Sprint 0 Outcome
Sprint 0 completed event storming and produced the context diagram for Goodtocode.Agents.Governance in AI inference workflows.

## Context Boundary
The library is an enforcement boundary between caller orchestration and downstream model inference.

## High-Level Context Diagram
```mermaid
flowchart LR
    Caller[Orchestrator / Agent Runtime] --> Request[GovernanceEvaluationRequest]
    Request --> Enforcer[GovernanceEnforcer]
    Enforcer --> Hash[RepeatabilityHashService]
    Enforcer --> Validator[EvaluationGovernanceValidator]
    Validator -->|invalid| ValidationError[GovernanceValidationException]
    Enforcer --> Composer[EvaluationGovernancePromptComposer]
    Extension[IGovernanceDirectiveExtension*] --> Composer
    Composer --> PromptCtx[EvaluationGovernancePromptContext]
    Enforcer --> Result[GovernedEvaluationResult]
    PromptCtx --> Result
    Result --> Inference[Model Inference Runtime]
    Baseline[Persisted Replay Baseline] --> ReplayGuard[GovernanceReplayGuard]
    Result --> Snapshot[GovernanceReplaySnapshot]
    Snapshot --> ReplayGuard
    ReplayGuard -->|mismatch| Drift[Replay Drift Exception]
    Inference --> Output[GovernedEvaluationOutputSchema]
```

## Event Storming Artifacts

### Commands
- EnforceGovernance
- ComputeRepeatabilityHashes
- ValidateGovernanceRecord
- ComposeGovernedPrompt
- ApplyGovernanceExtension
- EnsureExactReplay
- ValidateGovernedOutput

### Domain Events
- GovernanceEvaluationRequested
- RepeatabilityHashComputed
- GovernanceValidationPassed
- GovernanceValidationFailed
- GovernancePromptComposed
- GovernanceExtensionApplied
- GovernanceReplayCompared
- GovernanceReplayDriftDetected
- GovernedEvaluationProduced
- GovernedOutputValidated

### Invariants Captured
- Governance must validate before prompt output.
- Confidence values remain in range 0..1.
- Extension directives cannot bypass governance behavior.
- Replay comparison is exact for policy/model/hash fields.

## Sequence View
```mermaid
sequenceDiagram
    participant Caller as Orchestrator
    participant Enforcer as GovernanceEnforcer
    participant Hash as RepeatabilityHashService
    participant Validator as EvaluationGovernanceValidator
    participant Composer as EvaluationGovernancePromptComposer

    Caller->>Enforcer: EnforceGovernance(request)
    Enforcer->>Hash: Compute hashes (optional)
    Enforcer->>Validator: Validate(governance)
    alt invalid
        Validator-->>Caller: GovernanceValidationException
    else valid
        Enforcer->>Composer: Compose(promptRequest)
        Composer-->>Caller: GovernedEvaluationResult
    end
```

## Sprint 0 Decisions Reflected
1. Enforcement is a mandatory pre-inference gateway.
2. Deterministic hashing + validation are first-class preconditions.
3. Prompt composition is deterministic and extension-safe.
4. Replay guard is explicit and separate from prompt composition.
5. Governed output contract is validated at domain boundary.
