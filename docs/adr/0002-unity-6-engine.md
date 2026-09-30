# 0002 — Unity 6 for M001 and the primary mobile client

- Status: Accepted
- Date: 2026-09-30
- Owner / reviewer: Product owner (explicit human architecture decision)

## Context
Foundation left engine selection open under DC-0003. The product owner has now
selected Unity 6 for M001 and the primary Demon Codex mobile game client.
This record resolves that question in ADR 0001; its other boundaries remain in force.

## Decision
Use **Unity 6**. The initial target is local development with mobile-oriented
architecture; the primary platform direction is iOS and Android. M001 remains
local/offline. Keep deterministic core rules as independent of presentation and
as testable as practical, with controllable randomness and minimal Unity coupling.

Unity must not own authoritative future economy or inventory state. Future client
views and requests must remain separate from trusted authority, whose design is
deferred. This decision introduces no production backend, multiplayer or blockchain.
Revisiting the engine choice requires a new ADR; do not silently replace this decision.

## Reasons for selection
The product owner's selection rationale is:

- Mature mobile tooling.
- Broad platform support.
- Strong C# testing ecosystem.
- Suitability for 2D/3D board-game presentation.
- Compatibility with agent-assisted development workflows.

## Alternatives
Continuing to defer selection would leave M001 tooling direction unresolved.
Other engines or a custom client stack are not selected. No comparative benchmark
or exhaustive engine evaluation is claimed by this record.

## Consequences and tradeoffs
Future client work will use Unity and C#, with editor/project tooling and mobile
build integration to maintain. Editor-dependent tests can cost more to run than
isolated rules tests, reinforcing the need for a small, independently testable core.
Engine coupling and migration costs must be managed through clear boundaries.
Mobile build prerequisites, licensing suitability and device performance still need
review during setup; this decision supplies no budget or performance guarantees.

UNKNOWN: exact Unity 6 editor release, package versions, rendering approach and
device baseline. These are setup details, not an open engine-selection task.
At engine acceptance, detailed board rules remained UNKNOWN. The subsequent
[DC-0002 specification](../GDD.md) records supplied mechanics and remaining
review gaps without changing this engine decision. No project scaffold or game
implementation is authorized by this documentation update.

## Evidence
Explicit product-owner instruction on PR #1: “Demon Codex will use Unity 6 as the
game engine for M001 and the primary mobile game client.” This is a human decision,
not a conclusion from implementation tests. It completes the engine decision in
[DC-0003](../product/MASTER_BACKLOG.md); unrelated tasks retain their status.
