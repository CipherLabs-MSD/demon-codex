# Architecture

WORKING DECISION: this repository has a plain C# rules core with logical boundaries,
not a deployed service topology. See [ADR 0001](adr/0001-foundation-boundaries.md).

| Area | Responsibility | Current state |
| --- | --- | --- |
| game | Immutable match/configuration, legal moves, command reducer, ordered events and invariants | Plain C# core; no presentation |
| content | Versioned authoring contracts and approved public content | Proposed schema |
| backend | Future accounts, inventory authority and online systems | Deferred |
| assets | Reviewed art/audio with rights metadata | Empty of media |
| blockchain | Optional exceptional provenance adapter | Deferred; no code |
| tools/tests | NUnit, deterministic simulation and foundation/link checks | Executed headless evidence; Unity attempt blocked by licensing |

WORKING DECISION: the [DC-0002 rules specification](GDD.md) defines a deterministic
local rules core, explicit state transitions and externally supplied die results.
Randomness and UI adapters stay outside the core. Record seed and actions for reproducible
tests. Keep engine-specific types outside rules where practical. Do not add a
distributed backend to solve a local prototype problem.

ACCEPTED: [ADR 0002](adr/0002-unity-6-engine.md) selects Unity 6 for M001 and the
primary mobile client, with iOS/Android direction. M001 remains local/offline.
Unity must not own authoritative future economy/inventory state. The [minimal
project](../game/DemonCodex.Unity/README.md) pins editor 6000.6.3f1 and bundled Test
Framework 1.8.0. The engine choice requires a new ADR to revisit.

DC-0004 keeps runtime rules in an assembly with no Unity references; .NET Standard
2.1 builds link the same C# files. Commands carry revisions; immutable snapshots
and read-only configuration make rejection nonmutating. RNG and deterministic
test policy live only in an editor/test simulation assembly. The [evidence record](product/DC_0004_EVIDENCE.md)
details invariants, API boundaries and test results. No network/save protocol is implied.

## Identity boundaries

PROPOSAL: distinguish entity definition, collectible variant (rarity/edition/skin),
owned instance, account discovery, and provenance certificate. Canonical IDs are
001–666; unnumbered entities have separate opaque IDs and cannot expand that range.
Six ordinary rarity values do not encode identity classification or edition scarcity.
See [content contract](../content/README.md).

## Future integrations, not services to implement

PROPOSAL: MythicEncounterService evaluates authorized encounter policy, emits a
minimal discovery outcome and records witness provenance. Secret predicates stay
in an access-controlled system; clients receive only revealed information.
Forge consumes resources and grants results atomically under future inventory
authority. Commerce, physical redemption and optional provenance use separate
adapters; none controls ordinary match state or blocks ordinary play.

RESTRICTED: narrative policy and discovery conditions are outside ordinary agent
access. Public IDs, telemetry, bundles and error messages must not reveal them.
[Security](SECURITY.md) describes the boundary; a private folder is not that boundary.
