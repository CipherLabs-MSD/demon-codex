# Architecture

WORKING DECISION: this is a documentation-first repository with logical boundaries,
not a deployed service topology. See [ADR 0001](adr/0001-foundation-boundaries.md).

| Area | Responsibility | Current state |
| --- | --- | --- |
| game | Rules, turns, bot choices, presentation adapters | No implementation |
| content | Versioned authoring contracts and approved public content | Proposed schema |
| backend | Future accounts, inventory authority and online systems | Deferred |
| assets | Reviewed art/audio with rights metadata | Empty of media |
| blockchain | Optional exceptional provenance adapter | Deferred; no code |
| tools/tests | Foundation checks and future evidence | Documentation checks only |

WORKING DECISION: the [DC-0002 rules specification](GDD.md) defines a deterministic
local rules core, explicit state transitions and externally supplied die results.
Randomness and UI adapters stay outside the core. Record seed and actions for reproducible
tests. Keep engine-specific types outside rules where practical. Do not add a
distributed backend to solve a local prototype problem.

ACCEPTED: [ADR 0002](adr/0002-unity-6-engine.md) selects Unity 6 for M001 and the
primary mobile client, with iOS/Android direction. M001 remains local/offline.
Unity must not own authoritative future economy/inventory state. Exact editor and
package versions remain setup unknowns; the engine choice requires a new ADR to revisit.

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
