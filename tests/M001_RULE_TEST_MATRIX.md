# M001 rule-to-test matrix — DC-0004 input

WORKING DECISION: this matrix now maps to executable NUnit tests in
[RulesTests.cs](../game/DemonCodex.Unity/Assets/DemonCodex/Tests/EditMode/RulesTests.cs).
Source of truth: [GDD](../docs/GDD.md). T01–T25 passed in the .NET run (48 total
cases including parameterizations and extra diagnostics); see [actual results](evidence/dc-0004/unit-tests.json).
Unity EditMode tests were authored but **NOT EXECUTED** because the editor exited
198 for missing license entitlement. [Evidence](../docs/product/DC_0004_EVIDENCE.md) distinguishes both runners. The product owner explicitly approved G1–G6;
all corresponding expected outcomes are unconditional M001 requirements. Test IDs are stable; add cases without renumbering.

Use synthetic validated L/F/start/safety/order fixtures with labeled test-only
values. Include boundary/minimal valid configurations as well as larger tracks;
do not select final product dimensions from a fixture. Externally supply rolls,
choices and seeds. Compare complete snapshots and ordered events, not only visuals.

| Test ID | Rules | Setup / action | Expected evidence / invariants |
| --- | --- | --- | --- |
| T01 | R01 | Initialize valid config | 16 unique InAbyss pieces, 4 per owner; explicit initial player; no roll/winner (I01, I02, I09) |
| T02 | R02 | All pieces InAbyss, rolls 1..5 then 6 independently | No summons for 1..5; on 6 each piece can summon to p=0, no extra movement (I07) |
| T03 | R02, R11 | Friendly occupies owner's start; roll 6 | No summon there; existing track piece may still yield legal advance |
| T04 | R03 | Advance by each r in 1..6, including shared-index wrap | Exact route progress and correct modulo index, no accidental path entry at another player's start (I04, I07) |
| T05 | R04 | Several legal advances plus summons on 6 | Complete legal set; selected member alone moves; no voluntary pass (I11) |
| T06 | R04 | Bot chooses from same legal set | Fixed policy/input gives same choice; no bot-only legal actions (I12); strategy supplied separately |
| T07 | R05 | No moves for roll 1..5 | No piece changes; NoLegalMoves then TurnAdvanced; no pending roll (I09, I11) |
| T08 | R06 | Legal nonwinning move on 6 | Resolve move events before BonusRollGranted; same active player, AwaitingRoll |
| T09 | R07 | All summon destinations blocked, no advances, roll 6 | G2: NoLegalMoves then BonusRollGranted, no movement; never wait for an impossible selection |
| T10 | R08 | Exact landing on opponent, non-safe destination | Victim reset to InAbyss; mover occupies square; PieceBanished before mover event (I05, I06) |
| T11 | R09, R10 | Empty safe landing; opponent on each safe start/additional safe index | Empty is legal; G1: occupied safe landing/summon illegal; no banishment (I06) |
| T12 | R11 | Friendly at destination on track/private path | Excluded from legal set; forced submission rejected unchanged (I05, I10) |
| T13 | R12 | Occupied intermediate positions, including safe/friendly/opponent | G5: traversal allowed; only destination determines capture/occupancy |
| T14 | R13 | p=L-1, roll 1; separately cross boundary with residual steps | G3: path j=1, or Completed if F=1; no second start visit; correct owner (I04, I07) |
| T15 | R14 | Remaining distance d in 1..5; roll d+1 | Overshoot excluded; if no other legal moves use empty-set transition |
| T16 | R15 | Exact finish from private path and directly from track | Completed, no occupiable endpoint; no later move; other pieces can finish (accepted G3; I03) |
| T17 | R16 | Three pieces completed, fourth finishes on 6 | MatchWon, Finished, no BonusRollGranted or TurnAdvanced; subsequent roll/move rejected (I08) |
| T18 | R17 | Nonwinning capture/completion on non-six and on six | G6: no extra bonus on non-six; exactly one bonus opportunity on six |
| T19 | R18 | Sequence of three or more sixes, with mixed legal/empty rolls | G2/G6: remain same player; no penalty/cap; victory still takes precedence |
| T20 | R19 | Nonbonus resolution at last configured seat | G4: wrap to first; order is a permutation and initial seat is explicit |
| T21 | R20 | Restart mid-selection, after a bonus, after victory | Initial gameplay values restored; same config; no old pending roll/winner; stale pre-restart selection rejected (I12) |
| T22 | R21 | Wrong player/phase, out-of-range roll, forged move, duplicate or stale action | Snapshot unchanged and zero gameplay success events (I10) |
| T23 | R01, R13 | Invalid/missing L/F, duplicate starts, missing safe starts, invalid order | Reject initialization before creating a playable state; include boundary valid configs |
| T24 | R01–R21 | Replay same initial state, externally supplied rolls/choices twice | Identical snapshots and ordered event traces; no clock or RNG dependence (I12) |
| T25 | R01–R21 | Future seeded simulations across approved configs | Check I01–I12 after every action; report seed/trace on failure and watchdog noncompletion separately |

Human review has approved G1–G6 and the resulting M001 expectations. Each T-prefixed
method executes its corresponding row. T25 runs 12 integration matches in the
unit suite; the [separate 1000-match batch](evidence/dc-0004/summary.json) also passed
with zero invariant failures or watchdog cases. These are DC-0004 implementation
results, not proof of a playable UI or M001 completion.
