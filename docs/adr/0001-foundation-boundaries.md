# 0001 — Documentation-first foundation and separated identity

- Status: Accepted (within explicitly authorized bootstrap scope)
- Date: 2026-09-30
- Owner: Foundation bootstrap; accountable product owner: repository owner

## Context
The repository began empty. The founder requested a control center without gameplay,
backend, blockchain implementation or bulk demon generation.

## Decision
WORKING DECISION: retain requested directory boundaries, use one shared agent policy,
and add a proposed JSON Schema, contribution guide and dependency-free documentation
checks. Keep canonical identity, variant rarity, special classification and edition
separate. Keep restricted narrative out of ordinary repository content.

## Alternatives
A game scaffold would prematurely select a stack. Separate services would impose
unneeded operational costs. Plain prose alone would leave content boundaries harder
to inspect. Full entity records would risk premature canon.

## Consequences
No playable build exists. The schema is a reviewable authoring proposal, not a frozen
runtime API. At bootstrap, engine and rules required review. The engine question
is now resolved by [ADR 0002](0002-unity-6-engine.md); rules still require review. Added schema/tools/evidence files
are the only meaningful expansion of the requested layout; no system is deployed.

## Evidence
The founder's foundation brief is reflected in [vision](../VISION.md),
[M001 exclusions](../product/MILESTONES.md) and [security boundaries](../SECURITY.md).
Validation evidence is recorded in [foundation review](../product/FOUNDATION_REVIEW.md).
