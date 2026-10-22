# Feature: Agentic Intent Policy, Instruction and Preamble

## Summary
Governance has four assurance pillars and a policy layer. **Agentic Intent** is the first policy. It is a pillar-governed artifact, not a fifth pillar.

## Contracts (Phase A)
- `IntentPolicy`: `PolicyId`, `Version`, ordered `IntentPrinciple` list. `ComputeLockHash()` returns an uppercase SHA-256 over length-prefixed fields.
- `IntentPrinciple`: `PrincipleId`, `Statement`, `Kind` (`Boundary`, `Value`, `Conduct`), `Priority`.
- `IIntentPolicyProvider` / `IntentPolicyRequest(TenantId, Jurisdiction)`: host supplies the policy per tenant and jurisdiction. `StaticIntentPolicyProvider` is the simple default. No legal text lives in this package.
- `IGovernancePreambleComposer` / `GovernancePreambleComposer`: deterministic per-turn text.
- `GovernedTurnKind` (Chat, Collect, Evaluate, Record), `GovernancePreambleMode` (Compact, Full).
- `EvaluationGovernanceRecord.Intent` (`IntentRecord`: `Required`, `PolicyId`, `PolicyVersion`, `PolicyHash`).
- `GovernancePillarsProfile.IntentRequired` (default false).

## Instruction vs Preamble
| | Instruction | Preamble |
|---|---|---|
| Cadence | Once per session, playbook run or instantiation | Before every chat message and every LLM action |
| Produced by | `GovernanceEnforcer` + `EvaluationGovernancePromptComposer` | `GovernancePreambleComposer` |
| Content | Persona, core directives, intent policy (outranks persona) | Policy id/version/hash, principle ids (Compact) or text (Full), precedence |

## Precedence
`Boundary > Value > Conduct > persona > user request`. The composer places intent after the persona and states it outranks it. Persona, extension and preamble text pass the shared prohibited-fragment guard (`GovernanceDirectiveGuard`).

## Pillar mapping
- Observability: `governance.intent.*` and `governance.preamble.*` metadata.
- Repeatability: the policy hash is folded into `PromptHash` only when a policy is supplied, so existing hashes are unchanged. A policy change is `PromptChanged` drift in `GovernanceReplayGuard`.
- Auditability: `Intent` record persists id, version, hash. The enforcer derives them from the policy; callers cannot spoof them.
- Defensibility: the Instruction requires citing principle ids with a short justification, never chain-of-thought.

## Fail-closed
When `Intent.Required` is true, the validator requires id, version and hash, and the composer throws without a policy or when the hash does not match.

## Typed Instruction
`AgentInstruction.Create(text, InstructionScope)` (`Session`, `Playbook`, `Instantiation`) rejects empty text and prohibited fragments. `ComputeHash()` is folded into `PromptHash` as `[instruction:hash]` only when present. `EvaluationGovernancePromptRequest.Instruction` and `GovernanceEvaluationRequest.Instruction` replace the former string property (breaking change).

## Turn identity
`GovernedTurnRecord(Turn, PolicyId, PolicyVersion, PolicyHash, InstructionApplied, PreambleApplied, InstructionHash)` is reported to `IGovernedTurnObserver`. `GovernedTurnLedger` is an observer with `Drain()`, registered per run or session, so hosts persist turn identity in step or chat metadata.

## Phase D: output verification
- `IntentOutput` carries `IntentCitation`s (principle id plus short justification).
- `IIntentOutputVerifier` / `IntentOutputVerifier` with `IntentOutputVerifierOptions` (`CitationsRequired` default true, `MaxJustificationLength` default 500).
- Violations are `IntentViolation` with an `IntentViolationCode` (`MissingCitation`, `UnknownPrinciple`, `MissingJustification`, `JustificationTooLong`, `RuleViolation`).
- `IIntentOutputRule` is the pluggable hook for host rules or classifiers.
- `IntentVerificationResult.Passed` and `EnsurePassed()` (throws `GovernanceValidationException`).
- Pillars: Defensibility (citations), Auditability (violations are recorded), Observability (violation codes as metadata).

## Roadmap
Phases A to D are implemented. The chat decorator lives in `Goodtocode.Agents.Playbook.Prompting` because governance has no `Microsoft.Extensions.AI` dependency.
