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

Actually executed on 2026-10-01:

| Check | Result |
| --- | --- |
| Existing .NET rules tests | 48 passed, 0 failed |
| New shared .NET session tests | 9 passed, 0 failed |
| Existing deterministic simulation | 1000/1000 complete; 0 invariant failures, 0 watchdogs; CSV identical to baseline |
| Unity 6000.6.3f1 EditMode | 57 passed (48 original + 9 session), 0 failed |
| Unity 6000.6.3f1 PlayMode | 2 passed, 0 failed; real scene, two complete matches plus timing/restart test |

[Machine-readable evidence](../../tests/evidence/dc-0005/validation.json) records
individual Unity outcomes, raw XML paths/hashes and simulation totals. Raw local
logs/XML stay in ignored artifacts; the committed summary omits machine identity.
An initial screenshot-enabled batch run failed because the Game view did not
repaint. The corrected helper's rendered run passed and produced all four images.

Rendered screenshots were inspected; an Abyss/track overlap was corrected before
the final capture. These do not replace owner visual acceptance. The local Editor
Game view was unusually wide, so the proportional layout is letterboxed.

- [Initial board](../../tests/evidence/dc-0005/01-initial.png)
- [Human legal selection](../../tests/evidence/dc-0005/02-selection.png)
- [Active match](../../tests/evidence/dc-0005/03-active.png)
- [Victory](../../tests/evidence/dc-0005/04-victory.png)

Foundation/link/whitespace checks and GitHub CI are verified before final delivery.

Authored tests cover session start, roll equivalence with the reducer, exact legal
selectable sets, stale selection, six bonuses, no-move turns, bot ranking,
reproducible complete matches, terminal locking and restart. PlayMode loads the
committed scene and exercises real Update-driven bots, two complete matches,
knockout/completion position projection, pacing and pending-turn cancellation.

Automated human actions call the controller API. They are **not** evidence of
owner clicks or accepted readability/game feel. Complete the checklist before DONE.

## Human acceptance checklist — NOT YET EXECUTED

Record Unity version, seed, date, reviewer, pass/fail and notes for each item.

- [ ] Project opens without compilation errors.
- [ ] LocalMatch enters Play Mode and shows all 16 pieces.
- [ ] START MATCH and human ROLL work through actual controls.
- [ ] Only legal pieces are selectable; board and side buttons work.
- [ ] Three bots play automatically and yield turns or win.
- [ ] Six bonus and no-move feedback are understandable.
- [ ] Knockout visibly returns the opponent to its Abyss.
- [ ] A piece visibly enters its owner's Ascension Path.
- [ ] Exact completion visibly moves a piece home.
- [ ] Full match reaches a clear winner and gameplay stops.
- [ ] PLAY AGAIN clears die/log, resets pieces and restores the human turn.
- [ ] A second match begins and can be played to completion.
- [ ] Colors, safe markers, text and legal choices are readable.
- [ ] Owner explicitly approves DC-0005 DONE or supplies corrections.

## Open limits

Permanent dimensions, production first-player policy and final bot strategy remain
design choices. Stalemate/draw policy remains UNKNOWN; no timeout win or gameplay
cap is added. Test bounds are watchdogs only. No save/resume, animation, sound,
polished art, mobile validation, advanced AI, collection, backend, inventory,
economy or online systems are included. DC-0006 is not started.
