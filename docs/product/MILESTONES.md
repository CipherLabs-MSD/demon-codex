# Milestones

## F001 — Foundation v0.1
WORKING DECISION: docs, instructions, initial backlog, schema proposal, ADR process,
CI and reviewable commits. Evidence: [foundation review](FOUNDATION_REVIEW.md).

## M001 — First Playable (v0.01)
WORKING DECISION: supports O001 / KR1–KR5. Status: in progress — DC-0004 rules core is human-approved DONE; DC-0005 local Unity prototype is human-accepted DONE (2026-10-01). DC-0006 is DONE. **M001 is closed (2026-10-01) with the product-owner decision ITERATE**; see the [M001 review](M001_REVIEW.md). KR2–KR5 PASS; KR1 remains PARTIAL (no continuous human match to victory).

Acceptance: START → ROLL → SELECT → MOVE → KNOCK OUT → REACH HOME → WIN → RESTART.
One human can complete a match against three bots with four pieces per player,
one board and one die. Legal moves, turn transitions, safe/home handling and
knockout behavior conform to reviewed rules. Invalid actions cannot corrupt state.
Restart resets match state. Automated and manual evidence satisfy the OKR.

IN: local board rules, minimal readable UI, bot turns, victory, restart, rule tests,
seeded simulations, clean setup and human playtest record.

OUT: blockchain, NFTs, marketplace, physical redemption, real-money economy,
production backend, live multiplayer, 666 generated demons, elaborate LiveOps,
collection economy, Forge and monetization. Art may be clearly labeled placeholders.

Rules approval gate satisfied: the human owner accepted the [DC-0002 rules
specification](../GDD.md), including G1–G6. Board values must be explicit validated
configuration before a playable match; no final dimensions are selected here. The engine decision is satisfied:
[ADR 0002](../adr/0002-unity-6-engine.md) accepts Unity 6 for local/offline M001
and the primary mobile client (iOS/Android direction). Exit gate: reviewer confirms all acceptance evidence and
product owner records whether to iterate or proceed. No dates or budget promised.

## M001.1 — Pacing and fun playtest
WORKING DECISION: started 2026-10-01 at the owner's request after the ITERATE
decision. Status: REVIEW / TEST — the [experiment contract](M001_1_PACING_FUN_EXPERIMENT.md)
and Editor-only A/B tooling are ready; no human session is recorded yet
([DC-0037](MASTER_BACKLOG.md)). CONTROL A is the unchanged M001 game; VARIANT B
changes only the bot presentation delay (0.65 s → 0.25 s). It asks whether the core
match is enjoyable at its length, and if not why, before any scope expands.
Targets are provisional and need owner approval.
