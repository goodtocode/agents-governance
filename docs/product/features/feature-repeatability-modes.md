# Feature: Repeatability Modes (Governance)

## Overview
`Goodtocode.Agents.Governance` owns the policy for repeating an execution. It defines what Rerun, Recall and Replay mean, which Collect / Evaluate / Record actions each runs, and how a repeat is recorded and linked to its source. It performs no execution.

## Modes
| Mode | Actions planned | Needs seeds | Result |
|------|----------------|-------------|--------|
| Rerun (default) | Collect, Evaluate, Record | none | Fresh execution against live data |
| Replay | Evaluate, Record | evidence | Re-evaluates the recorded evidence |
| Recall | none (Record only when `RepeatabilityOptions.RecallRecords` is true) | evidence and finding | Re-presents the recorded result |

## Contracts
- `ActionType` (`[Flags]`: Collect, Evaluate, Record): the governance classification of what a step does, and the set of actions a repeat runs
- `RepeatMode`, `RepeatRequest` (mode plus `SourceExecutionId`), `RepeatPlan` (mode plus the `ActionType` set to run)
- `IRepeatPolicy` / `DefaultRepeatPolicy`: turns a request into a action plan
- `IRepeatRecorder` / `DefaultRepeatRecorder`: records a repeat with source lineage

## Consumers
Playbook references this library and runs the planned actions (`PlaybookExecutor`). Applications reference Playbook and get Governance transitively. Governance never references Playbook (guarded by `PackageIndependenceTests`).

## Tests
`RepeatPolicyTests`, `RepeatRecorderTests`, `RepeatRequestTests`, `RepeatActionModelTests`, `RepeatRecorderLineageTests`, `RepeatRegistrationTests`.

## Related
[Replay drift guard](feature-replay-drift-guard.md), [Repeatability hash service](feature-repeatability-hash-service.md)
