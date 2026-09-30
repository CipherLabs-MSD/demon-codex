# Agent operating instructions

Build Demon Codex incrementally: a play-to-collect universe with 666 canonical
demons. Foundation contains no game. Read README, the task, relevant domain docs,
M001 and accepted ADRs before changing files.

## Authority and decisions

Current explicit human instructions govern task scope. Product CANON and accepted
ADRs govern existing decisions; WORKING DECISION is revisable, PROPOSAL is unapproved,
UNKNOWN must remain visible and RESTRICTED must remain inaccessible. If sources
conflict, surface the conflict; never silently promote proposals to canon.
Backlog tasks organize work but do not authorize spending or publishing.

## Implementation discipline

Prefer small, modular, testable changes. Keep rules separate from presentation and
randomness controllable. Avoid speculative abstractions and dependencies. Unity 6
is the accepted engine for M001 and the primary mobile client; follow
[ADR 0002](docs/adr/0002-unity-6-engine.md). No premature blockchain, multiplayer
or production backend. Ordinary
game state stays off-chain. Keep canonical numbering separate from anomaly identity,
rarity, edition and owned instances. Consult architecture and schema docs.

## Safety and escalation

Human approval is required for large-blast-radius architecture changes, money,
publication, production deployments, material economy changes, chain deployments,
destructive operations, restricted reveals and legal/compliance decisions.
Raise assumptions that materially change product architecture with options and
evidence. Routine reversible work within an authorized task may proceed.
Never read, infer, invent or commit restricted lore. A hidden local folder is not
security. Do not place secrets in code, issues, tests, logs or PR descriptions.

## Workflow and evidence

Trace objective → KR → milestone → epic → task → implementation → test → evidence
→ review. Use stable task IDs. Move READY → IN PROGRESS → REVIEW / TEST → DONE;
use BLOCKED with a reason and unblock action. DONE requires acceptance evidence
and the applicable review. Do not report unrun tests as passing.

Run `python tools/check_foundation.py` for foundation edits; add meaningful rule
tests, seeded simulations and manual playtest evidence when gameplay exists.
Update affected docs and decision labels in the same change. Record unknowns.

Inspect status and preserve other contributors' edits. Use a feature branch,
coherent imperative commits and a PR to main. Never force-push shared branches or
rewrite published history. Review diffs for credentials and restricted material.
Do not merge, release or deploy without authorization. CLAUDE.md points here so
policy does not drift between agents.
