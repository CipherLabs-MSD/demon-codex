# Proposed content contract

PROPOSAL: [entity.schema.json](entity.schema.json) is JSON Schema draft 2020-12 for
public authoring review. It is not the runtime model and contains no demon records.
No engine or JSON Schema validator dependency is selected by this contract.

Canonical definitions use codexId 001–666; anomalous definitions omit it and use an
independent entityId. Normal rarity, special classification and editions are separate.
Nested sections cover identity, lore, visual, media, gameplay and commerce policy refs.
Optional fields allow incomplete approved briefs without fabricated details.

Cross-record constraints require future catalog validation: unique entityId and codexId,
references resolving to approved records, and eventually exactly 666 approved canonical
definitions. The schema alone cannot enforce catalog completeness or access control.
Rarity variant arrays do not mean every demon must exist at every rarity.

Owned instance, player discovery and certificate records are future separate contracts.
Never store bearer data, secret policy bodies or discovery predicates here. Public
policy refs must resolve only to safe published descriptors, not secret identifiers.
Migration proposal: version schema; validate changes; document migration before any
approved records rely on it. Foundation checks parse/check contract structure only;
full standards-based instance validation remains future work.
