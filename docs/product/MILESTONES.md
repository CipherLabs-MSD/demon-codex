# Milestones

## F001 — Foundation v0.1
WORKING DECISION: docs, instructions, initial backlog, schema proposal, ADR process,
CI and reviewable commits. Evidence: [foundation review](FOUNDATION_REVIEW.md).

## M001 — First Playable (v0.01)
WORKING DECISION: supports O001 / KR1–KR5. Status: not started.

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
