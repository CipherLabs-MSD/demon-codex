# DC-0005 — Local match UI + bots

Status: REVIEW / TEST; owner playtest is required before DONE. DC-0006 remains BACKLOG.
Base: approved main `91229d89c9295387d7248b3f70773ec9714ce52f`.
This adds the first playable local Unity scene; no approved domain rule changes.

## Play the prototype

1. Open `game/DemonCodex.Unity` using Unity **6000.6.3f1**.
2. Open `Assets/DemonCodex/Scenes/LocalMatch.unity` and enter Play Mode.
3. Maximize the Game view; use landscape, preferably 1200 × 850 or larger.
4. Leave seed `2026` (or enter an unsigned 32-bit integer), then START MATCH.
5. Play Human/P0: ROLL, then click a white-bordered piece or its Piece button.
   Illegal pieces have no click action. A six may summon or move; it grants another
   roll even if no move is legal. Watch the die and chronological match log.
6. Bots act automatically, with 0.65 seconds between their commands.
7. Race four pieces through your colored Ascension Path to exact completion.
   The winner banner replaces gameplay controls with PLAY AGAIN.
8. PLAY AGAIN uses the domain Restart command, clears presentation history and
   resets the external die to the chosen seed. Play a second complete match.

The scene is the first enabled build scene. No packaged binary is claimed.
Desktop Editor input is the acceptance target; iOS/Android builds, touch
ergonomics and IL2CPP remain unverified.

## Owner playtest record — PARTIAL

Owner report, 2026-10-01, Unity 6000.6.3f1, normal START MATCH flow:

- The normal Unity prototype launched successfully.
- The owner started and played a normal match.
- Rolling, piece movement and automatic bot turns were observed working.
- The owner paused before the endgame because the manual match took too long.

Endgame human acceptance (knockout, Ascension, completion, victory, PLAY AGAIN
and a second match) remains **PENDING**. The development scenarios below were
added only to make that remaining acceptance fast.

## Development test scenarios (Editor only)

Menu `Demon Codex > Development Playtest Scenarios` offers TEST KNOCKOUT,
TEST ASCENSION, TEST COMPLETION and TEST VICTORY during Play Mode.
[ScenarioReplay](../../game/DemonCodex.Unity/Assets/DemonCodex/Development/ScenarioReplay.cs)
replays ordinary seeded sessions through the normal session API, checking
invariants after every transition. It stops at the first human selection where a
reducer-supplied legal move would produce the outcome; the preview is a pure
`RulesEngine.Apply` on the immutable state. No snapshots, rule overrides or special
dice are fabricated. The owner's click then submits the normal `SelectMove`
command, so the knockout, Ascension, completion and victory transitions and events
come from the DC-0004 reducer, not the presentation layer.

| Button | Source seed / revision | Die | Click | Reducer result |
| --- | --- | --- | --- | --- |
| TEST KNOCKOUT | 1 / 193 | 4 | Piece 3, square 9 → 13 | Bot II piece 2 banished to its Abyss |
| TEST ASCENSION | 1 / 154 | 5 | Piece 2, square 35 → path 1 | Piece enters the blue Ascension Path |
| TEST COMPLETION | 1 / 269 | 1 | Piece 2, path 5 → home | Piece completes; no winner |
| TEST VICTORY | 3 / 546 | 3 | Piece 4, path 3 → home | Fourth piece home; Human wins |

Isolation: the `DemonCodex.Development` assembly is Editor-only and not
auto-referenced; controller/view hooks are `#if UNITY_EDITOR`, so player builds
contain none of it (verified by a Windows player build, below). START MATCH never calls the replay. While a scenario is
loaded the title reads `DEVELOPMENT TEST SCENARIO` and bot scheduling is paused
so the result stays visible. After the scenario move, the panel shows the normal
seed field and PLAY AGAIN (inherited code showed it only after a win; fixed here).
PLAY AGAIN issues the domain Restart command, clears scenario mode and resumes
bots: an ordinary seeded match follows. Automated scenario tests are **not**
human acceptance.

## M001 PROTOTYPE CONFIGURATION

These are explicit, revisable DC-0005 choices, **not permanent canon**. The rules
remain parameterized. See [GDD](../GDD.md) for authoritative mechanics.

| Setting | Prototype value |
| --- | --- |
| Main track length L | 40 shared squares, indices 0–39, clockwise |
| Starts P0 / P1 / P2 / P3 | 0 / 10 / 20 / 30 |
| Safe squares | 0, 5, 10, 15, 20, 25, 30, 35 (including starts) |
| Ascension length F | 6 each: five occupiable squares j=1–5, then terminal completion at 6 |
| Player order | P0 → P1 → P2 → P3 → P0 |
| Initial-player policy | P0 always starts; fixed prototype policy, not random seating |
| Controls | P0 human; P1–P3 bots; four pieces each |
| Initial seed | 2026; editable before start and after victory |
| Restart seed | Same entered seed by default; owner may choose another |

An original radial board presents a shared ring, four private inward spokes,
separate Abyss panels and four home displays. Home slots are visual counters, not
occupiable domain squares. The start is not revisited: the domain handles entry
after one circuit. Safe markers, player colors/names, piece numbers and white
legal-selection borders distinguish states. No commercial layout or assets copied.

## Architecture and bot policy

- [LocalMatchSession](../../game/DemonCodex.Unity/Assets/DemonCodex/LocalMatch/LocalMatchSession.cs)
  owns lifecycle and seeded session dice, dispatches existing commands and exposes
  the reducer's legal candidates. Its assembly has no Unity references.
- `BotPolicy` ranks only legal candidates: completion, then knockout, then summon,
  then furthest source progress (private path after main track), then lowest piece
  index. Summoning is preferred whenever legal after the first two priorities.
  No advanced strategy or randomness enters selection.
- Dice use xorshift32 with rejection sampling. Zero seed maps to nonzero state
  `0x6d2b79f5`. Reproducibility requires the same seed and human choices.
- [LocalMatchController](../../game/DemonCodex.Unity/Assets/DemonCodex/Presentation/LocalMatchController.cs)
  submits actions and schedules one bot command per delay using Unity time. Domain
  transitions happen first. Ordered events update die/log; then the view references
  the new immutable snapshot. Timing is not game logic.
- [LocalMatchView](../../game/DemonCodex.Unity/Assets/DemonCodex/Presentation/LocalMatchView.cs)
  renders IMGUI primitives and controls. `BoardLayout` translates positions into
  coordinates, not moves. Pieces snap to results; interpolation is deferred.
- Restart invokes reducer Restart, retains increasing revision, resets RNG/log/die
  and cancels the old bot deadline. No manual piece reset.
- Optional debug panel: seed, revision, player, phase, pending roll, legal count,
  winner. No future economy/inventory authority is introduced.

## Validation

Actually executed on 2026-10-01, after the development-scenario work (supersedes
the earlier 57 EditMode / 2 PlayMode run):

| Check | Result |
| --- | --- |
| Existing .NET rules tests (SDK 8.0.425) | 48 passed, 0 failed |
| Shared .NET session + scenario tests | 14 passed (9 session + 5 scenario), 0 failed |
| Existing deterministic simulation | 1000/1000 complete; 0 invariant failures, 0 watchdogs; CSV identical to baseline |
| Unity 6000.6.3f1 EditMode | 62 passed (48 rules + 9 session + 5 scenario), 0 failed |
| Unity 6000.6.3f1 PlayMode (batch) | 3 passed, 0 failed |
| Unity 6000.6.3f1 PlayMode (rendered, screenshots) | 3 passed, 0 failed; 8 images |
| Throwaway Windows x64 player build (isolation check) | Success; only Rules, LocalMatch and Presentation assemblies; no scenario code or text in the player |
| `python tools/check_foundation.py` (local Python 3.10.6) | PASS, including local links |
| `git diff --check` | clean |

The owner's Editor had the project open, so Unity ran on a clean scratch copy of
the exact committed file set (tracked files, new scenario files and their
Editor-generated `.meta` files; untracked local ProjectSettings excluded).
[Machine-readable evidence](../../tests/evidence/dc-0005/validation.json) records
individual Unity outcomes, raw XML paths/hashes, scenario data and simulation
totals. Raw local logs/XML stay in ignored artifacts; the committed summary omits
machine identity. Unity reports CS0618 deprecation warnings for
`FindFirstObjectByType` (pre-existing in PlayMode tests, repeated in the new
window); they are not errors. The player build only verifies compilation and
isolation; it is not a distributed or tested binary.

Rendered screenshots were inspected. The four normal-match images regenerated
byte-identical to the earlier commit, so normal presentation is unchanged. Each
scenario image shows the banner, the reducer result and the PLAY AGAIN exit.
These do not replace owner visual acceptance. The local Editor Game view was
unusually wide, so the proportional layout is letterboxed.

- [Initial board](../../tests/evidence/dc-0005/01-initial.png)
- [Human legal selection](../../tests/evidence/dc-0005/02-selection.png)
- [Active match](../../tests/evidence/dc-0005/03-active.png)
- [Victory](../../tests/evidence/dc-0005/04-victory.png)
- Scenario results: [knockout](../../tests/evidence/dc-0005/05-scenario-knockout.png),
  [Ascension](../../tests/evidence/dc-0005/05-scenario-ascension.png),
  [completion](../../tests/evidence/dc-0005/05-scenario-completion.png),
  [victory](../../tests/evidence/dc-0005/05-scenario-victory.png)

Authored tests cover session start, roll equivalence with the reducer, exact legal
selectable sets, stale selection, six bonuses, no-move turns, bot ranking,
reproducible complete matches, terminal locking and restart. PlayMode loads the
committed scene and exercises real Update-driven bots, two complete matches,
knockout/completion position projection, pacing and pending-turn cancellation.
Scenario tests check each replay reaches a legal human selection through real
commands, the intended move is selectable, invariants hold, the reducer produces
the outcome, replays are reproducible and preparing one leaves normal seeded play
unchanged. The PlayMode scenario test loads each into the real scene, checks bots
stay paused, the PLAY AGAIN exit is offered, and Restart resumes an ordinary match.

Automated human actions call the controller API. They are **not** evidence of
owner clicks or accepted readability/game feel. Complete the checklist before DONE.

## Human acceptance checklist — PARTIAL

Record date, reviewer, pass/fail and notes. "Observed" items come from the owner's
2026-10-01 report; everything else is still open.

| Item | Status | Fast path |
| --- | --- | --- |
| Project opens without compilation errors | Observed | — |
| START MATCH and human ROLL work through actual controls | Observed | — |
| Pieces move and three bots take turns automatically | Observed | — |
| Only legal pieces are selectable; board and side buttons work | PENDING | Any scenario |
| Knockout visibly returns the opponent to its Abyss | PENDING | TEST KNOCKOUT |
| A piece visibly enters its owner's Ascension Path | PENDING | TEST ASCENSION |
| Exact completion visibly moves a piece home | PENDING | TEST COMPLETION |
| Match reaches a clear winner and gameplay stops | PENDING | TEST VICTORY |
| PLAY AGAIN clears die/log, resets pieces and restores the human turn | PENDING | After any scenario |
| A second match begins and bots resume | PENDING | After PLAY AGAIN |
| Six bonus and no-move feedback are understandable | PENDING | Normal match, if seen |
| Colors, safe markers, text and legal choices are readable | PENDING | Throughout |
| Owner explicitly approves DC-0005 DONE or supplies corrections | PENDING | — |

Playing a second match all the way to victory is covered by automated PlayMode
evidence only; the owner decides whether "second match begins" is sufficient.

## Open limits

Permanent dimensions, production first-player policy and final bot strategy remain
design choices. Stalemate/draw policy remains UNKNOWN; no timeout win or gameplay
cap is added. Test bounds are watchdogs only. No save/resume, animation, sound,
polished art, mobile validation, advanced AI, collection, backend, inventory,
economy or online systems are included. Development scenarios are an Editor
acceptance aid, not a gameplay feature or save system. DC-0006 is not started.
