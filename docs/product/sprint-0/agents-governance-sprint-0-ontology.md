# Agent Governance - Sprint 0 Ontology

## Sprint 0 Outcome
Sprint 0 completed ontology discovery and normalization for the Goodtocode.Agents.Governance domain. The team aligned on shared language mapped to the current library API surface.

## Purpose
Define canonical terms for governance enforcement across AI inference workflows to reduce ambiguity in code, docs, tests, and product planning.

## Canonical Concepts
- Governance Aggregate: EvaluationGovernanceRecord with PolicyProfileVersion + four pillars.
- Observability: TraceId, CorrelationId, EvidenceRefs.
- Repeatability: ModelRef, ModelVersion, PromptHash, InputHash, DeterministicReplaySupported, Seed.
- Auditability: OwnerId, TenantId, PrincipalDisplay, ToolRefs.
- Defensibility: PoliciesApplied, JustificationRefs, ReasoningSummary, ConfidenceScore.
- Enforcement/Validation: GovernanceEvaluationRequest, GovernedEvaluationResult, EvaluationGovernanceValidator, GovernanceValidationIssue, GovernanceValidationException.
- Prompt Composition: EvaluationGovernancePromptRequest, EvaluationGovernancePromptContext, EvaluationGovernancePromptComposer.
- Extension Model: IGovernanceDirectiveExtension, GovernanceDirectiveContribution with ordering, uniqueness, and safety checks.
- Determinism/Replay: RepeatabilityHashService, GovernanceReplaySnapshot, GovernanceReplayGuard.
- Policy and References: GovernancePillarsProfile, GovernanceReference.
- Governed Output Contract: GovernedEvaluationOutputSchema + criterion and audit trace types.

## Ubiquitous Language (Approved)
Governance, Policy Profile, Directive, Evidence Reference, Justification Reference, Replay Drift, Governed Output.

## Synonyms Rejected
- Guardrail payload -> Governance Record
- Prompt policy blob -> Prompt Context
- Hash token -> PromptHash/InputHash
- Audit actor -> OwnerId/TenantId/PrincipalDisplay

## Sprint 0 Decisions
1. Four-pillar governance is the canonical abstraction.
2. Enforcement is validation-first and deterministic.
3. Prompt composition is extensible but safety-constrained.
4. Replay protection requires exact snapshot matching.
5. Governed output validation is part of the product contract.

## Traceability
Canonical terms map to src/Goodtocode.Agents.Governance/Application/* and src/Goodtocode.Agents.Governance/Domain/*.
This document is the ontology baseline for Sprint 1+.
