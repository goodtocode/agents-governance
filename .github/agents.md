# AI Agent Operating Guide

## Purpose
This file defines repository-specific operating rules for AI agents working in goodtocode/agent-governance.

## Repository Scope
- Primary deliverable: reusable .NET library package Goodtocode.Agents.Governance.
- Current solution: Goodtocode.Agents.Governance.slnx.
- Main projects:
  - src/Goodtocode.Agents.Governance/ (library)
  - src/Goodtocode.Agents.Governance.Tests/ (tests)

## Required Reading Order
1. .github/copilot-instructions.md
2. README.md
3. docs/governance/architecture.md
4. docs/governance/coding-standards.md
5. docs/governance/development-workflow.md
6. docs/governance/sprint-0-step-1-ontology.md
7. docs/governance/sprint-0-step-2-event-storming.md
8. docs/product/sprint-0/agent-governance-sprint-0-ontology.md
9. docs/product/sprint-0/agent-governance-sprint-0-context-diagram.md
10. docs/product/features/*.md

## Agent Workflow
1. Confirm target behavior from product/governance docs.
2. Keep changes minimal and inside the requested scope.
3. Preserve library API intent (governance enforcement, validation, repeatability, defensibility).
4. Update or add tests when behavior changes.
5. Verify with build/test before completion.

## Constraints
AI agents must not:
- Introduce repo structure assumptions from other solutions.
- Break existing package contracts without explicit request.
- Add unrelated dependencies or architecture layers.
- Edit unrelated workflows/docs outside task scope.

## Delivery Expectations
- Changes compile.
- Tests pass for affected areas.
- Docs remain consistent with code.
- File paths and naming match this repository.

## Source Control Workflow
- Leave all changes local and uncommitted. Do not run `git commit`, `git push`, or open pull requests; the maintainer reviews the uncommitted diff first and then commits, pushes, and opens the PR.
- Do not create or switch branches unless asked. Summarize what changed so the diff can be reviewed.
