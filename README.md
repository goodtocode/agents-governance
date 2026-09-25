# Goodtocode.Agents.Governance Quick Start

[![NuGet CI/CD](https://github.com/goodtocode/agents-governance/actions/workflows/agents-governance-nuget.yml/badge.svg)](https://github.com/goodtocode/agents-governance/actions/workflows/agents-governance-nuget.yml)

[![NuGet](https://img.shields.io/nuget/v/Goodtocode.Agents.Governance.svg?label=NuGet)](https://www.nuget.org/packages/Goodtocode.Agents.Governance)

Use this package in any inference-driven workflow (Microsoft.Extensions.AI, Microsoft Agent Framework, or Semantic Kernel) to add consistent governance for:

- Observability (trace and evidence visibility)
- Auditability (actor and tool accountability)
- Repeatability (stable replay baselines)
- Defensibility (justified and explainable outcomes)

---

## Why the Four Pillars Matter

Generative AI inference is inherently non-deterministic: the same prompt, sent twice, can produce
two different outputs, and a model can hallucinate plausible-sounding but false or unsupported
claims. Traditional software correctness practices (unit tests, fixed I/O contracts) do not fully
apply when the "function" under test is a probabilistic model. Governance is how this package
closes that gap — not by making the model deterministic, but by making every inference **provable,
traceable, and replay-checkable** regardless of what the model actually returns.

Each pillar addresses a distinct failure mode of AI inference:

### Observability — "Can we see what happened?"

**Value & intent:** Every inference action must leave a traceable record: trace/correlation
identifiers, timestamps, and evidence references. Without this, a hallucinated or wrong answer is
just an isolated bad output with no way to reconstruct the run that produced it.

**Why it matters for hallucination mitigation:** Observability turns "the model said something
wrong" into "here is the exact prompt, input, evidence, and context that led to that output" —
which is a prerequisite for diagnosing *why* a hallucination occurred and whether it's systemic.

### Auditability — "Who did it, and with what?"

**Value & intent:** Every action must be attributable to a principal (user, service, or agent),
a tenant/owner, and the tools/connectors it used. This package enforces `OwnerId`, `TenantId`,
and `ToolRefs` as first-class governance fields, not optional metadata.

**Why it matters:** AI agents increasingly act autonomously and invoke tools on a user's behalf.
If an agent's tool call or decision can't be tied back to an accountable actor, the organization
has no way to assign responsibility, revoke trust, or investigate misuse — hallucinations included.

### Repeatability — "Would we get the same result again?"

**Value & intent:** This package computes and persists deterministic `PromptHash` and `InputHash`
values for every inference call, plus model/version and seed metadata, so a later "replay" can be
compared against the original run via `GovernanceReplayGuard`.

**Why it matters most for inference variability:** Because LLM inference is not guaranteed to
repeat the same result twice (even at temperature 0, across model versions, or under load-balanced
routing), repeatability governance doesn't promise identical output — it promises **detectable
drift**. If a replay diverges from its baseline, that divergence is itself a governed, auditable
signal, rather than a silent surprise.

### Defensibility — "Can we justify the result?"

**Value & intent:** Every governed output must carry the policy applied, the justification
references, a reasoning summary, and a confidence score. This is the pillar most directly aimed
at hallucination: it forces the system to record *why* a result was accepted, not just what the
result was.

**Why it matters:** A hallucinated output that passes through ungoverned is indistinguishable from
a correct one until a human notices. Requiring policy/evidence/confidence at the point of
inference means low-confidence or policy-violating outputs can be flagged, rejected, or routed to
human review before they propagate downstream.

### The pillars together

No single pillar is sufficient on its own:
- Observability without defensibility tells you *what happened* but not *whether it was justified*.
- Defensibility without repeatability lets you justify one run but gives no way to detect drift on the next.
- Repeatability without auditability tells you *that* something changed but not *who/what* is accountable.

Together, the four pillars make hallucination and inference variability **manageable risks with
evidence trails**, instead of untraceable, unexplainable, one-off failures. See
[pipeline-governance-principles.md](../crucible-web/docs/governance/pipeline-governance-principles.md)
in the Crucible product for the full durable governance-lock model (`GovernanceProfile`,
`GovernanceLockHash`, and the collect/evaluate/record evidence chain) that this package's runtime
enforcement is designed to interoperate with.

## How This Differs From LangChain / LangGraph-Style Frameworks

Popular Python agent frameworks in the "Lang\*" family (LangChain, LangGraph, and similar) provide
strong orchestration primitives — chains, graphs, tool-calling, memory — and some offer tracing or
callback hooks that touch on observability. However, they are built around an **AI-agent-first**
execution model: the graph/chain is the unit of control, and deterministic steps are typically just
another node type inside an inherently agentic, LLM-driven flow.

`Goodtocode.Agents.Governance` takes a different position:

- **Enforcement precedes execution.** `GovernanceEnforcer.Enforce(...)` is a mandatory gate that
  must succeed *before* any model or tool call runs — it is not an optional callback or trace
  sink attached after the fact.
- **Governance is closed for modification, open for extension.** Core directives
  (observability/auditability/defensibility/repeatability) cannot be weakened or removed by
  extensions (`IGovernanceDirectiveExtension`); only additive, domain-specific directives are
  allowed.
- **True hybrid deterministic + AI workflows.** Because governance is enforced at the boundary
  of *any* inference call rather than baked into an agent-graph runtime, this package supports
  pipelines that freely mix fully deterministic, rule-based steps with AI-agent steps in the same
  governed execution — each inference step is individually gated, hashed, and made replay-checkable,
  while deterministic steps run without needing to be reshaped into agent/graph nodes to get the
  same audit trail. Lang\*-style frameworks generally assume the AI-agent graph *is* the workflow,
  which makes injecting non-agentic, deterministic business logic with equivalent governance
  coverage awkward or impossible without significant custom scaffolding.

The result: teams can adopt an agentic workflow where it adds value, keep deterministic logic
where it's more reliable and cheaper, and get the same observability/auditability/defensibility/
repeatability guarantees across both — which is the governance model Crucible's pipeline principles
document requires end-to-end (Collect → Evaluate → Record).

---

## 1) Install package
```powershell
dotnet add package Goodtocode.Agents.Governance
```

Or pin a version:

```powershell
dotnet add package Goodtocode.Agents.Governance --version <latest-version>
```
---

## 2) Add using statements
These namespaces contain the core contracts (`EvaluationGovernanceRecord`) and execution entrypoint (`GovernanceEnforcer`).

```csharp
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
```

---

## 3) Instantiate and optionally reuse one enforcer

**Intent**  
Make governance enforcement mandatory before every inference action (single prompt, tool call, chain step, or workflow step).

**Why**  
A reused enforcer gives one stable pre-inference control point.  
You can call it inline directly, or wrap it in your own Gate class if you want custom exception mapping.

The package keeps mandatory core governance directives locked in place (closed for modification).  
Extension is opt-in only (open for extension): you can append extra directives, but you cannot remove or weaken core directives.

**Code (instantiate once)**

```csharp
var enforcer = new GovernanceEnforcer(new EvaluationGovernancePromptComposer());
```

**Optional extension (only if you need app-specific directives)**

```csharp
public sealed class MyGovernanceExtension : IGovernanceDirectiveExtension
{
    public string ExtensionId => "my-governance";
    public string ExtensionVersion => "1.0.0";
    public int Order => 100;

    public GovernanceDirectiveContribution Build(EvaluationGovernancePromptRequest request)
    {
        _ = request;
        return new GovernanceDirectiveContribution
        {
            Directives =
            [
                "Include domain-specific evidence tags in each scored decision."
            ],
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["profile"] = "domain-x"
            }
        };
    }
}

var enforcerWithExtension = new GovernanceEnforcer(
    new EvaluationGovernancePromptComposer(
    [
        new MyGovernanceExtension()
    ]));
```

**Code (call inline every time before inference)**

```csharp
GovernedEvaluationResult governed;
try
{
    governed = enforcer.Enforce(request);
}
catch (GovernanceValidationException ex)
{
    // Map to your app's validation/error contract and stop execution.
    throw;
}

// Only call your AI runtime after governance succeeds:
// - use governed.PromptContext.SystemInstruction
// - use governed.PromptContext.Metadata
// - persist governed.PromptHash and governed.InputHash
```

**Optional pattern: Gate wrapper**

If your stack prefers a Gate abstraction, it can call `enforcer.Enforce(request)` internally and translate `GovernanceValidationException` into your local error type.

**Expected behavior**  
`enforcer.Enforce(request)` is the enforcement boundary:
- validates governance input
- computes repeatability hashes from raw prompt/inputs
- returns governed prompt context for runtime execution
- throws `GovernanceValidationException` when governance is invalid
- appends optional extension directives in deterministic order when extensions are provided

If enforcement fails, inference should not run.

---

## 4) Build a governance request from your inference inputs

**Intent**  
Capture the complete governance envelope for one inference operation.

**Why**  
Governance only works when context is explicit: what was run, by whom, with what evidence, under which policy, and against which model.

**Code**

```csharp
var correlationId = Guid.NewGuid();
var promptText = "Evaluate these inputs with strict policy and evidence traceability.";

var request = new GovernanceEvaluationRequest
{
    Governance = new EvaluationGovernanceRecord
    {
        PolicyProfileVersion = "v1",
        Observability = new ObservabilityRecord
        {
            TraceId = correlationId.ToString("N"),
            CorrelationId = correlationId,
            EvidenceRefs =
            [
                GovernanceReference.Parse("evidence://records/42")
            ]
        },
        Repeatability = new RepeatabilityRecord
        {
            ModelRef = "model://azure-openai/gpt",
            ModelVersion = "2026-01-01",
            DeterministicReplaySupported = true,
            Seed = 1234
        },
        Auditability = new AuditabilityRecord
        {
            OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            PrincipalDisplay = "service:inference-runner",
            ToolRefs =
            [
                GovernanceReference.Parse("tool://inference/evaluator")
            ]
        },
        Defensibility = new DefensibilityRecord
        {
            PoliciesApplied =
            [
                GovernanceReference.Parse("policy://governance/v1")
            ],
            JustificationRefs =
            [
                GovernanceReference.Parse("justification://ruleset/primary")
            ],
            ReasoningSummary = "Decision must be policy-aligned and evidence-backed.",
            ConfidenceScore = 0.95
        }
    },
    ExistingSystemInstruction = "You are a governed inference system.",

    // Raw values: package computes hashes internally.
    RepeatabilityPromptContent = promptText,
    RepeatabilityInputs = new Dictionary<string, object?>
    {
        ["customerProfile"] = new { segment = "enterprise", region = "us" },
        ["metrics"] = new { risk = 0.2, quality = 0.91 }
    }
};
```

**Expected behavior**  
- Observability fields define trace/correlation/evidence.
- Auditability fields define ownership and acting tool context.
- Defensibility fields define policy and reasoning support.
- Repeatability raw sources are accepted; hashes are generated internally by the package.

---

## 5) Enforce governance before inference

**Intent**  
Run governance checks and prompt composition before any model/tool invocation.

**Why**  
This makes governance a required precondition rather than optional afterthought.

**Code**

```csharp
var governed = enforcer.Enforce(request);
```

**Expected behavior**  
`governed` includes:

- `governed.PromptContext.SystemInstruction` for governance guardrails
- `governed.PromptContext.Metadata` for normalized, persistable governance metadata
- `governed.PromptHash` as read-only computed repeatability prompt hash
- `governed.InputHash` as read-only computed repeatability input hash

`PromptHash` and `InputHash` are **always** computed from `RepeatabilityPromptContent` and
`RepeatabilityInputs`, unconditionally, on every call to `Enforce` — including when those raw values
are empty (for example, a workflow with zero inputs). You never set these hashes yourself, and the
enforcer never skips hashing based on emptiness. This keeps hashing closed for modification.

The hashing *algorithm* itself is open for extension. `GovernanceEnforcer` accepts an optional
`IRepeatabilityHashStrategy`; if you don't supply one, the built-in SHA-256/canonical-JSON
`DefaultRepeatabilityHashStrategy` is used automatically:

```csharp
var enforcer = new GovernanceEnforcer(
    new EvaluationGovernancePromptComposer(),
    hashStrategy: new MyCustomHashStrategy()); // optional — omit to use the default
```

---

## 6) Send governed prompt context to your AI runtime

**Intent**  
Use enforced context as the runtime input to your model/agent stack.

**Why**  
Governance has effect only if the governed instruction/metadata are actually used during execution and persisted with run records.

**Example flow (minimal snippets)**

```csharp
var systemInstruction = governed.PromptContext.SystemInstruction;
var metadata = governed.PromptContext.Metadata;
```

Microsoft.Extensions.AI (MEI):

```csharp
var messages = new List<ChatMessage> { new(ChatRole.System, systemInstruction), new(ChatRole.User, userPrompt) };
var response = await chatClient.GetResponseAsync(messages, cancellationToken: ct);
```

Microsoft Agent Framework:

```csharp
var prompt = $"{systemInstruction}\n\n{userPrompt}";
var response = await agent.RunAsync(prompt, cancellationToken: ct);
```

**Expected behavior**  
Your inference call is constrained by governance guardrails, and your run log contains all data required for investigation and replay analysis.

---

## 7) Replay and drift control

**Intent**  
Detect when a "replay" is no longer equivalent to the original governed run.

**Why**  
Inference drift can occur from changed prompts, inputs, model versions, or runtime settings.

**Guidance**

- Reuse persisted governance metadata and hashes.
- Reuse same model and version when possible.
- Treat hash mismatches as drift; fail fast or require explicit override.
- Use `GovernanceReplayGuard` to compare baseline vs current replay conditions.

**Expected behavior**  
You can quickly distinguish true replays from drifted reruns.

---

## 8) Failure behavior

**Intent**  
Handle invalid governance deterministically.

**Why**  
Governance errors should be explicit and actionable, not silent.

**Behavior**

- Invalid governance input throws `GovernanceValidationException`.
- Catch at your boundary and return your platform's validation contract.

**Expected behavior**  
Bad governance payloads are rejected before inference execution starts.

---

## 9) Minimal checklist

**Intent**  
Give a fast implementation completion signal.

**Why**  
This helps teams verify they implemented the governance path end-to-end.

- [ ] Enforce governance before every inference call
- [ ] Persist `PromptHash` and `InputHash`
- [ ] Persist trace/correlation/owner/tenant/tool refs
- [ ] Persist defensibility references and reasoning summary
- [ ] Use replay guard for deterministic reruns

**Expected behavior**  
If all boxes are checked, you have a practical baseline for observable, auditable, repeatable, and defensible inference workflows.

---

## 10) Extending with `IGovernanceDirectiveExtension`

**Intent**  
Add domain- or product-specific governance directives without modifying core package directives.

**Why**  
Core governance stays mandatory and unchanged, while extension logic remains opt-in for advanced use cases.

**Code**

```csharp
using Goodtocode.Agents.Governance.Application;

public sealed class FinanceGovernanceExtension : IGovernanceDirectiveExtension
{
    public string ExtensionId => "finance-governance";
    public string ExtensionVersion => "1.0.0";
    public int Order => 200;

    public GovernanceDirectiveContribution Build(EvaluationGovernancePromptRequest request)
    {
        _ = request;
        return new GovernanceDirectiveContribution
        {
            Directives =
            [
                "For financial decisions, include evidence tags for risk, threshold, and source timestamp."
            ],
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["domain"] = "finance",
                ["requiresRiskEvidence"] = "true"
            }
        };
    }
}

var composer = new EvaluationGovernancePromptComposer(
[
    new FinanceGovernanceExtension()
]);

var enforcer = new GovernanceEnforcer(composer);
```

**Expected behavior**  
- Core governance directives are always included.
- Extension directives are appended in deterministic order (`Order`, then `ExtensionId`).
- Extension provenance is included in metadata (for example `governance.extensions.applied`).
- If an extension tries to weaken governance, enforcement fails with `GovernanceValidationException`.

---

## 11) Extending with `IRepeatabilityHashStrategy`

**Intent**  
Swap the repeatability hashing algorithm without changing when or whether hashing happens.

**Why**  
Most consumers never touch this — raw prompt/input values are always hashed automatically by the
built-in SHA-256/canonical-JSON strategy. Supply a custom strategy only if you need a different
algorithm (for example, to match an existing hash format already stored in your system).

**Code**

```csharp
using Goodtocode.Agents.Governance.Application;

public sealed class MyCustomHashStrategy : IRepeatabilityHashStrategy
{
    public string ComputePromptHash(string promptContent) =>
        Convert.ToHexString(System.Security.Cryptography.SHA512.HashData(
            System.Text.Encoding.UTF8.GetBytes(promptContent)));

    public string ComputeInputHash(IReadOnlyDictionary<string, object?> inputs) =>
        Convert.ToHexString(System.Security.Cryptography.SHA512.HashData(
            System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(inputs))));
}

var enforcer = new GovernanceEnforcer(
    new EvaluationGovernancePromptComposer(),
    hashStrategy: new MyCustomHashStrategy());
```

**Expected behavior**  
- Hashing still runs unconditionally on every `Enforce` call; only the algorithm changes.
- `PromptHash`/`InputHash` are never settable directly by consumers, regardless of strategy.
- Omitting `hashStrategy` uses `DefaultRepeatabilityHashStrategy` automatically — no setup required.
