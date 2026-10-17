# Copilot Instructions for Goodtocode.Agents.Governance

## Project Overview
Goodtocode.Agents.Governance is a .NET governance library for AI inference workflows. It provides deterministic governance enforcement across observability, repeatability, auditability, and defensibility.

## Repository Shape
- Solution: Goodtocode.Agents.Governance.slnx
- Library: src/Goodtocode.Agents.Governance
- Tests: src/Goodtocode.Agents.Governance.Tests
- Product docs: docs/product/**
- Governance docs: docs/governance/**
- Automation: .github/workflows/** and .github/scripts/**

## Coding Expectations
- Follow existing patterns in the touched folder before introducing new patterns.
- Keep public contracts stable unless requested.
- Prefer deterministic behavior and explicit validation.
- Use Microsoft/.NET built-in capabilities unless a new dependency is necessary.
- Keep XML docs and markdown docs aligned with behavior changes.

## Testing Expectations
- Build the solution after code changes.
- Run relevant tests in Goodtocode.Agents.Governance.Tests.
- For workflow/script updates, validate referenced paths and commands against this repo layout.

## Documentation Expectations
When modifying docs, keep terms aligned with Sprint 0 artifacts:
- docs/product/sprint-0/agent-governance-sprint-0-ontology.md
- docs/product/sprint-0/agent-governance-sprint-0-context-diagram.md

## CI/CD Expectations
- Ensure workflow commands match installed SDK and command semantics.
- Keep package CI focused on restore/build/test/pack for this solution and project names.
- Avoid environment assumptions that do not exist in this repository.

## Source Control Workflow
- Leave all changes local and uncommitted. Do not run `git commit`, `git push`, or open pull requests; the maintainer reviews the uncommitted diff first and then commits, pushes, and opens the PR.
- Do not create or switch branches unless asked. Summarize what changed so the diff can be reviewed.

## NuGet Versioning
- Do not alter `<Version>`, `<FileVersion>`, `<AssemblyVersion>`, `<InformationalVersion>`, or pin temporary versions (for example `9.9.0-local`) in any `.csproj`, and do not force version bumps.
- Versioning is owned by `.github/scripts/ci/Set-Version.ps1` and `Get-Version.ps1`, driven by `VERSION_MAJOR`/`VERSION_MINOR` in the workflow. Only the maintainer changes those, and only when asked.
- For local cross-repo testing, do not edit project files; ask the maintainer how to consume a local build.
