# Game design

PROPOSAL: ROLL → MOVE → ATTACK/KNOCKOUT → REWARD → COLLECT → DISCOVER → FORGE → PROGRESS.
Only the board portion belongs to [M001](product/MILESTONES.md).

## Product clarification: mechanic-family lineage

CANON (human product clarification): the first playable uses an original race /
movement / knockout board-game loop informed by long-established public-domain
cross-and-circle mechanics such as Pachisi, Ludo and Fia med knuff. This is
inspiration at the mechanic-family level.

Demon Codex must develop its own board layout, progression, terminology, visuals,
abilities, economy, content, UX and meta systems. Do not copy proprietary layouts,
assets, wording, progression systems or distinctive presentation from contemporary
commercial games. This direction does not add abilities or meta systems to M001.
No specific rules are implied by these references; use the explicit M001 rules below.

## M001 scope

ACCEPTED FOR M001: the product owner supplied the mechanical rules in DC-0002.
They apply to one local/offline match with four players, one human and three bots,
four pieces per player and one six-sided die. They do not authorize implementation.
Human product review of PR #2 / DC-0002 is complete. G1–G6 below are explicitly
accepted for M001; the specification task is approved without selecting board values.

WORKING DECISION: **Abyss**, **Summon**, **Banished** and **Ascension Path** are
provisional terms, not immutable CANON. Internal state names below are also working
vocabulary and carry no new lore. Abilities are outside this ruleset.

## Abstract topology and configuration

WORKING DECISION: describe the board as a directed cyclic main track plus four
private final paths, not a visual layout. The configuration contains:

| Field | Meaning and validation | Concrete M001 value |
| --- | --- | --- |
| mainTrackLength L | Integer count of main-track positions; at least four to support distinct starts | UNKNOWN |
| startIndex[player] | Four distinct indices in 0..L-1, one per player | UNKNOWN |
| safeSpaceIndices | Explicit subset of 0..L-1 containing every start index | Starts required; additional indices UNKNOWN |
| finalPathLength[player] | Positive integer steps from the last main-track position to completion; call the moving owner's length F | UNKNOWN |
| playerOrder | Permutation of all four player IDs, used cyclically under accepted G4 | Concrete seating/order UNKNOWN |
| initialPlayer | One member of playerOrder, explicit match input | Selection policy UNKNOWN |

Incrementing an index modulo L defines abstract forward movement; it does not pick
a clockwise artwork orientation. Paths have owner-qualified coordinates. A square
with the same local number in two private paths is not the same position. Safety
is a property of the shared main-track index, not of the player occupying it.
Validate configuration before constructing a match; reject missing/invalid values.
No arbitrary board dimensions, final art or runtime defaults are supplied here.

### Accepted M001 route convention (G3)

ACCEPTED FOR M001 — G3: summoning places a piece at its start with progress p=0. Track progress
p ranges from 0 to L-1; its shared index is (startIndex[owner] + p) modulo L.
After visiting each main-track position once, the next step enters private path
position j=1. The start is not visited a second time. For a roll r, q=p+r:

- q < L: remain OnMainTrack at progress q.
- L <= q < L+F-1: enter OnAscensionPath at j=q-L+1.
- q = L+F-1: become Completed.
- q > L+F-1: illegal overshoot.

OnAscensionPath uses j=1..F-1. A roll advances to j+r; equality with F completes
the piece, exceeding F is illegal. F=1 has no occupiable private-path positions:
the first step off the main track completes the piece. A roll can cross the track
boundary and complete in one move if its distance is exact. There is no extra lap,
branch choice, optional early entry or movement into an opponent's private path.
The final destination is a completed state, not a square that finished pieces block.
This convention makes off-by-one behavior reviewable without choosing L or F.

## State vocabulary

WORKING DECISION: a match snapshot contains validated immutable configuration,
four player IDs/control types, 16 stable piece IDs and owners, the current phase,
activePlayer, pendingRoll (absent or 1..6), optional winner and a revision identifier.
All players stay in the order until victory; M001 has no elimination subsystem.
Successful commands advance the revision; restart must use a fresh revision rather
than reusing the original one. Invalid commands leave it unchanged. A roll passed
to move generation in AwaitingSelection must match pendingRoll; other rolls are rejected.

| Piece state | Position data | Meaning |
| --- | --- | --- |
| InAbyss | None | Off-board and eligible to summon only on 6 |
| OnMainTrack | Owner-relative progress p | Shared square derived from configuration |
| OnAscensionPath | Private path position j, owner from piece | Only this owner's path is reachable |
| Completed | None | Finished, immobile, counted toward victory |

InAbyss and Completed hold multiple pieces because they are states, not occupiable
squares. No piece can have two states at once. Banishing resets it to InAbyss,
discarding all route progress. Summoning consumes the entire rolled 6 and places
the piece on its starting square; it does not also move six more spaces.

WORKING DECISION: match phases are AwaitingRoll, AwaitingSelection and Finished.
Resolution is atomic, not a separately interactive phase. Starting/restarting a
match initializes all pieces InAbyss, no pending roll or winner, the configured
initialPlayer and AwaitingRoll. Restart recreates this complete gameplay state
using the same configuration and initial-player input; external RNG reset/seed
selection is explicit caller policy, never hidden rules-core randomness.

## Legal-move generation and resolution

WORKING DECISION: the conceptual function is
`state + active player + die roll → set of legal moves`.
It accepts a valid, nonterminal state and an integer roll 1..6 for the active player.
The function is pure: it does not roll, pick a piece, change state or consume time.

For each of the active player's four pieces:

1. InAbyss yields a Summon candidate only on 6, targeting that owner's start.
2. OnMainTrack or OnAscensionPath yields an Advance candidate using the exact roll
   and route convention above. Remove overshoots; Completed yields no candidate.
3. Remove candidates ending on a friendly occupied main-track or private-path
   position. Summons use the same destination occupancy checks as other moves.
4. A non-safe main-track destination occupied by an opponent is a legal banishment
   destination. Remove candidates targeting an opponent-occupied safe square (G1):
   entry is illegal and its occupant is never banished. Intermediate occupancy
   does not block movement or cause banishment (G5); only the destination matters.
5. Return every surviving candidate, identified by piece, action, source, target
   and current roll/revision. On 6 both Summon and Advance candidates can coexist.

The human chooses from the full set; bot selection must be a separate deterministic,
testable policy that returns a member of that same set. Exact bot ranking is UNKNOWN
and outside DC-0002. There is no voluntary pass when legal moves exist: choosing
a piece resolves the roll. A singleton set may be auto-selected by the caller.

WORKING DECISION: `state + selected legal move → new state + events` uses the stored
pendingRoll and recomputes legality. Reject wrong phase/player, stale revision,
invalid roll or altered/illegal move without mutating state or emitting gameplay
success events. Duplicate submissions must not apply a move twice. Transport error
reporting is separate from domain events.

For a valid move, atomically remove the moving piece from its source, banish any
eligible destination opponent, place/complete the mover, and check victory. Emit
PieceBanished if applicable, then PieceSummoned / PieceMoved / PieceCompleted as
applicable, followed by the match/turn outcome. A completing advance emits PieceMoved
then PieceCompleted. Events record IDs and before/after positions; display wording
is not canon. Consumers must never observe a half-resolved occupancy conflict.

## Turn-state transitions

WORKING DECISION: RollProvided is a command carrying an externally generated 1..6.
No rules function samples randomness. Replaying the same initial configuration,
rolls and valid choices produces identical state and ordered events. Seeded RNG and
simulation limits live in the caller/test harness.

| Current phase / input | Validation and transition | Result |
| --- | --- | --- |
| AwaitingRoll + RollProvided | Validate active player and roll; emit DieRolled; generate legal set | Nonempty set stores roll and enters AwaitingSelection |
| AwaitingRoll + empty legal set, roll 6 | Emit NoLegalMoves, then BonusRollGranted (G2); move no piece | Same player, AwaitingRoll; clear pending roll |
| AwaitingRoll + empty legal set, roll 1..5 | Emit NoLegalMoves, then TurnAdvanced (G4); move no piece | Next cyclic player, AwaitingRoll; clear pending roll |
| AwaitingSelection + selected legal move | Atomically resolve and check all four owned pieces | On victory emit MatchWon and enter Finished, clear pending roll |
| AwaitingSelection + nonwinning legal move, roll 6 | Emit BonusRollGranted; retain active player | AwaitingRoll, clear pending roll |
| AwaitingSelection + nonwinning legal move, roll 1..5 | Emit TurnAdvanced according to G4 | Next player's AwaitingRoll, clear pending roll |
| Any phase + invalid normal action | Reject; no gameplay mutation or success events | Identical snapshot |
| Finished + roll/move command | Reject; no normal transitions after victory | Finished remains unchanged |
| Any phase + Restart | Recreate valid initial gameplay state; invalidate old move tokens | AwaitingRoll and MatchRestarted |

Victory takes precedence over every bonus, including a winning 6. One accepted roll
resolves at most one piece move. The next roll cannot arrive while awaiting a choice.
No legal move is an automatic roll resolution, not a user-selected pass. Repeated
sixes have no penalty or cap. Only a six grants a bonus; knockout and completion
do not grant one themselves (accepted G6). Victory always takes precedence.

## Rule and edge-case table

ACCEPTED denotes human-approved M001 mechanics, including the explicit G1–G6
rulings from PR #2 review. Rule IDs are stable references for tests.

| Rule | Situation / example | Required outcome | Authority |
| --- | --- | --- | --- |
| R01 | Start/restart | Four players, 16 pieces, all InAbyss; one human and three bots | ACCEPTED |
| R02 | Abyss piece, roll 1..5 / 6 | No summon / candidate to own start only; occupied destination checks still apply | ACCEPTED; state interpretation |
| R03 | On-track/path piece, roll r | Move exactly r along own route; no split movement or optional shorter move | ACCEPTED |
| R04 | Several legal pieces, including summon and advance on 6 | Choose exactly one legal piece; bots obey identical legality | ACCEPTED |
| R05 | No legal piece, roll 1..5 | No movement; advance player | ACCEPTED |
| R06 | Legal move on 6 | Resolve move first, then grant another roll to same player unless victory | ACCEPTED |
| R07 | No legal move on 6 | Emit NoLegalMoves, then BonusRollGranted to the same active player; no movement | ACCEPTED G2 |
| R08 | Land exactly on opponent on non-safe track square | Banish opponent to InAbyss, reset its progress, occupy destination | ACCEPTED |
| R09 | Land on empty safe square | Legal; all starting squares are safe for every occupant | ACCEPTED |
| R10 | Land/summon on opponent-occupied safe square | Illegal destination; never banish or stack with opponent | ACCEPTED G1 |
| R11 | Land/summon on friendly occupied square | Illegal, including own start and private path | ACCEPTED |
| R12 | Pass over occupied positions with free/legal destination | Allowed; intermediate occupancy never blocks or captures | ACCEPTED G5 |
| R13 | Last track position to private path | Use G3 route boundary; carry remaining steps into owner-only path | ACCEPTED G3 |
| R14 | Remaining distance d; roll greater than d | Illegal for that piece; another legal piece may still move | ACCEPTED |
| R15 | Roll exactly remaining distance | Piece becomes Completed and cannot move again; completion occupancy uses G3 | ACCEPTED G3 |
| R16 | Fourth owned piece completes | Immediate victory; freeze normal play, including any earned six bonus | ACCEPTED |
| R17 | Capture or completion without full victory | No extra roll unless roll was 6; victory overrides bonus | ACCEPTED G6 |
| R18 | Consecutive sixes | No penalty, cap or three-sixes rule | ACCEPTED G6 |
| R19 | Last seat resolves nonbonus roll | Wrap to first configured seat under G4; initial seat stays explicit | ACCEPTED G4 |
| R20 | Restart during choice, bonus sequence or after victory | Clear positions/progress, pending roll and winner; restore initial active player; reject old selections | ACCEPTED restart; WORKING DECISION state contract |
| R21 | Out-of-turn, duplicate, stale or malformed action | Reject atomically; no state mutation | WORKING DECISION validation contract |

## Invariants for DC-0004

WORKING DECISION: check these after every accepted transition and failed action:

- I01: exactly four players and 16 unique pieces exist, four immutable owners/pieces per player.
- I02: every piece has exactly one state with valid position data; configuration remains unchanged.
- I03: Completed pieces never move; only restart can return them to InAbyss.
- I04: private-path positions are owner-qualified; no opponent can enter another player's path.
- I05: at most one piece occupies any shared track square or owner-qualified
  private-path position. Friendly stacking and cross-player safe stacking are forbidden (G1).
- I06: safe-space occupants are never banished; banishment removes all prior progress.
- I07: legal movement consumes the exact roll, except summon consumes 6 to enter at p=0.
- I08: winner exists exactly when Finished, with all four winning pieces Completed; victory halts normal transitions.
- I09: AwaitingSelection has one valid pending roll and a nonempty legal set; other phases have no pending roll.
- I10: invalid actions preserve the complete snapshot and emit no gameplay success events.
- I11: only six grants a bonus, including an empty legal set; bonuses retain the
  active player. No consecutive-six penalty/cap exists. Nonbonus turns advance
  cyclically; one roll moves at most one piece. Victory suppresses every bonus (G2/G4/G6).
- I12: identical inputs yield identical new state and ordered events; restart restores initial gameplay values and invalidates old moves.

## Accepted M001 gap resolutions

The human product owner explicitly approved all six rulings in PR #2 / DC-0002
review. These are unconditional M001 expectations, not proposals or UNKNOWNs.

| Ruling | Accepted resolution | Authority |
| --- | --- | --- |
| G1 | Opponent-occupied safe squares block entry; illegal destination, no banishment and no cross-player stacking | ACCEPTED FOR M001 |
| G2 | A six with no legal move emits NoLegalMoves, then grants another roll to the same active player | ACCEPTED FOR M001 |
| G3 | One circuit as specified, no second start visit; owner-only final path; exact terminal non-occupiable completion; no extra lap, branch or early entry | ACCEPTED FOR M001 |
| G4 | Fixed configured cyclic permutation of four players, explicit initialPlayer; nonbonus turns advance cyclically | ACCEPTED FOR M001 |
| G5 | Pass over friendly/opposing pieces; only destination affects occupancy, banishment or move legality | ACCEPTED FOR M001 |
| G6 | Six is the only bonus source; no knockout/completion bonus, consecutive-six penalty, cap or three-sixes rule; victory takes precedence | ACCEPTED FOR M001 |

## Explicit UNKNOWNs outside DC-0002 approval

UNKNOWN configuration: concrete L, four starts, any additional safe indices, F and
first-player/seating policy. Choose them explicitly before a playable board; tests
may use clearly labeled synthetic configurations, never promote fixture numbers to
product decisions. UNKNOWN outside DC-0002: bot strategy, external restart seed
policy, final artwork and terminology. Stalemate/draw rules are UNKNOWN; do not
invent a draw, timeout win or forced move. Simulation watchdog expiration is a
reported noncompletion, not a gameplay outcome. No finite completion guarantee is
claimed for arbitrary dice/choices. Assess liveness in later simulation work.

DC-0002 is approved and DONE. The rules core can remain parameterized; these
remaining configuration/design choices do not block its approval. They require
separate explicit decisions and must not become implicit defaults. DC-0004 remains
BACKLOG and unstarted; this approval does not authorize gameplay implementation.
See the [rule-to-test matrix](../tests/M001_RULE_TEST_MATRIX.md) and
[DC-0002 review record](product/DC_0002_REVIEW.md).

PROPOSAL: a later collection slice could add four identities, twelve cards,
three initial rarities, a Codex screen, one currency, one chest and one Forge recipe.
It is not required for M001. Accessibility, readable move feedback and touch input
need evaluation in subsequent game-feel work.
