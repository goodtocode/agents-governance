# Pipeline Governance Principles (Static, Product-Wide)

## Purpose

Define a durable, reusable governance lock for pipeline systems with four mandatory principles:

1. Observability
2. Auditability
3. Defensibility
4. Repeatability

## Principle Definitions

- **Observability**: every execution path must be traceable with evidence references.
- **Auditability**: ownership/tenant and tool use must be attributable.
- **Defensibility**: applied policy and justification context must be preserved.
- **Repeatability**: the same inputs and configuration must be replayable and verifiable.

## Repeatability Is Three Distinct Modes, Not One

"Repeat" is ambiguous unless a governed execution declares which of three modes it is
performing. A policy profile that only checks `DeterministicReplaySupported` cannot express
this distinction, so every repeated execution must record an explicit `ReplayMode`:

1. **Recall** — no inference and no re-collection. The pipeline rehydrates a previously
   persisted `EvaluationGovernanceRecord` (and its evidence) for display, dispute review, or
   chronicle re-render. Nothing runs, so nothing can drift. This is the correct mode for
   "show me what happened," report re-render, and cached-result reuse when the runtime can
   prove the cached result belongs to the same run/step/binding/contract.
2. **Replay** — an exact-match verification. The same collected evidence (not re-collected)
   is re-submitted to the same model reference, model version, prompt/workflow content, and
   inputs used originally. `GovernanceReplayGuard.EnsureExactReplay` compares the *input*
   side (policy profile version, model ref/version, prompt hash, input hash) of a baseline
   snapshot against a current snapshot. Input-hash equality only proves the request was
   reproduced faithfully; it does not by itself prove the output is identical unless the
   model/tool is genuinely deterministic (temperature 0 with a provider determinism
   guarantee, or a rule-based/deterministic tool). Replay must therefore also compare an
   `OutputHash` (a hash of the governed structured output) when `DeterministicReplaySupported`
   is claimed true. A model or tool that cannot guarantee identical output for identical
   input must set `DeterministicReplaySupported = false` and use Rerun instead of Replay.
3. **Rerun** — a fresh execution that intentionally re-collects source evidence and/or
   re-invokes a non-deterministic model against the same prompt version, rubric version, and
   temperature/settings, expecting the collected data or the model output to legitimately
   differ. A Rerun is not a governance violation when its inputs differ from the baseline; it
   is a new governed record linked to the prior one via a `SourceExecutionRef` (or
   `ReplayOfRecordRef`) lineage reference. Rerun is the correct mode for "has the source
   system state changed" (Collect) and "is the model still scoring this evidence
   consistently" (Evaluate drift/consistency checks). A Rerun's defensibility record should
   capture an agreement/variance summary against the prior record rather than throwing on
   mismatch — mismatch is the expected signal, not an error.

`RepeatabilityRecord` is extended with `ReplayMode` (`RepeatabilityReplayMode.Recall` | `Replay` |
`Rerun`, default `Rerun`) and `SourceExecutionRef`, and `IRepeatabilityHashStrategy` is extended
with `ComputeOutputHash` so Replay can assert output identity, not only input identity.
`GovernanceReplayGuard.EnsureExactReplay` now also compares `OutputHash` when both baseline and
current snapshots provide one, and `GovernanceReplayGuard.CompareRerun` returns a non-throwing
`GovernanceRerunComparison` drift descriptor for the Rerun case, since a Rerun is allowed to
diverge. `EvaluationGovernanceValidator` requires `SourceExecutionRef` whenever `ReplayMode` is
`Recall` or `Replay`. The equivalent CER-level capability — `PlaybookReplayMode` and
`PlaybookReplayContext<TEvidence, TFinding>` — is implemented in
[Goodtocode.Agents.Playbook](https://github.com/Goodtocode/agents-playbook)'s `PlaybookExecutor`,
independently of this package, so hosts that use the playbook executor without referencing
governance still get Recall/Replay/Rerun stage-skipping behavior. Wiring the four governance
pillars into that playbook execution context (so a playbook-level `Rerun`/`Replay`/`Recall`
automatically produces a governed `EvaluationGovernanceRecord`) is planned as a follow-up patch.

## Durable Lock Model

`GovernanceProfile` is the shared contract used across products:

- `PolicyProfileVersion`
- `ObservabilityRequired`
- `AuditabilityRequired`
- `DefensibilityRequired`
- `RepeatabilityRequired`
- deterministic `GovernanceLockHash` (SHA-256 of the profile)

The lock hash is used to detect drift/tampering and to verify cross-entity governance alignment.

## Governed Evaluation Output Schema (Required)

In addition to entity-level governance locks, evaluate-stage outputs must use a deterministic governed schema:

- `overall_score` (0-100)
- `overall_level`
- `overall_confidence` (0-1)
- `defensibility_summary`
- `criteria[]` with required fields:
  - `name`
  - `score` (0-100)
  - `level`
  - `justification`
  - `evidence`
  - `rubric_reference`
  - `confidence` (0-1)
  - `uncertainty_flag`
  - `defensibility`
- `strengths[]`
- `weaknesses[]`
- `recommendations[]`
- `audit_trace`:
  - `model_version`
  - `rubric_version`
  - `timestamp_utc`
  - `evaluation_id`

This schema is represented by `GovernedEvaluationOutputSchema` and validated at runtime before record-stage persistence.

## Hard-Persisted Fields

The following persisted entities must contain governance lock fields:

- `PipelineEntity`
  - `GovernancePolicyProfileVersion`
  - `GovernanceObservabilityRequired`
  - `GovernanceAuditabilityRequired`
  - `GovernanceDefensibilityRequired`
  - `GovernanceRepeatabilityRequired`
  - `GovernanceLockHash`
- `PlaybookEntity`
  - `GovernancePolicyProfileVersion`
  - `GovernanceObservabilityRequired`
  - `GovernanceAuditabilityRequired`
  - `GovernanceDefensibilityRequired`
  - `GovernanceRepeatabilityRequired`
  - `GovernanceLockHash`

## Non-Bypass Rules

- Pipeline and playbook creation/update must apply governance from authoritative pipeline kit registration.
- Governance flags must remain fully required for all four principles.
- Read and execute paths must validate governance hash integrity.
- Pipeline execution must reject playbooks whose governance profile/hash does not match pipeline governance.

## Reuse Guidance

This model is intentionally product-agnostic:

- Keep `GovernanceProfile` in shared core libraries.
- Allow each product to choose profile versions (for example `*.v1`, `*.v2`) while preserving hash semantics.
- Use the same lock profile contract for:
  - agent-framework quick starts
  - semantic-kernel quick starts
  - any orchestrated multi-step pipeline/runtime.
