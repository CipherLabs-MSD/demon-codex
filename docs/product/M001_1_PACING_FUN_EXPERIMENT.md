# M001.1 Pacing & Fun Experiment

Task: [DC-0037](MASTER_BACKLOG.md). Follows the [M001 review](M001_REVIEW.md)
(owner decision ITERATE, 2026-10-01). Started at the owner's request on 2026-10-01.
Status: **REVIEW / TEST** — experiment contract and Editor tooling are ready; **no
human playtest session has been recorded yet**.

Labels: the design below is a WORKING DECISION. Every target is a **PROVISIONAL
PRODUCT TARGET** that needs owner approval; none is a permanent requirement.

## Question

Is the core Demon Codex match enjoyable at its current length, and if not, what is
causing the pacing problem?

This first comparison isolates **one** candidate cause: waiting for bots. It does
not assume that cause is correct. The other candidates from M001 — too many rolls,
forced actions, few meaningful choices, board length, no-legal-move frequency,
weak feedback, low tension or risk/reward, lack of differentiation — stay open.

## Hypotheses

- **H0:** faster bots do not materially change measured match time, felt length or fun.
- **H1:** bot waiting is a major part of the felt length. Simulation predicts B cuts
  median bot waiting from about 4.6 to about 1.8 minutes; human time is unmeasured.
- **H2:** even with fast bots the match feels long or low in control, pointing to
  decision density, no-move rolls or board length rather than bot delay.
- **H3:** length feels fine but fun or control is low, pointing to tension and
  meaningful choices rather than duration.

## Control

**CONTROL A — ORIGINAL PACING** is the merged M001 game, unchanged: the M001
PROTOTYPE CONFIGURATION (L=40, starts 0/10/20/30, safe squares every 5, F=6, four
players with four pieces, P0 human, P1–P3 bots), the approved DC-0002 rules
through the DC-0004 reducer, the DC-0005 bot policy and seeded dice, and the
committed scene's bot delay of **0.65 s**.

## Variant

**VARIANT B — FAST BOTS** is identical except `LocalMatchController.BotDelaySeconds`
= **0.25 s**.

Why 0.25 s: the controller waits one delay before every bot command — the roll,
then the move — counted from the latest transition. At 0.25 s each bot roll and
move stays on screen for about 15 frames at 60 fps, enough to register the die
value and the piece jump without animation, and a typical bot turn drops from
about 1.3 s to 0.5 s. A value of 0.20 s would save only about 0.35 more minutes
per median match while showing each un-animated bot result for just 0.2 s. The
match log keeps every event either way. This is a presentation value, not a rule.

## Variables Held Constant

| Held constant in A and B | How it is ensured |
| --- | --- |
| Board, movement, exact completion, summon on six, six bonus, knockout (Banished), safe squares, Ascension Path, victory | Same DC-0004 reducer and prototype configuration; no rule code changed |
| Bot decision policy | Same `BotPolicy`; tests show identical events for A and B |
| Dice | Same seed gives the same die values, rolled by the same seats in the same order, for as long as both matches last (tested). It does **not** make the matches identical: see Known Limitations |
| Scene, Game view and controls | Same scene; experiment controls live only in the Editor window; the Game view shows no variant name |

Only `BotDelaySeconds` differs.

## Metrics

Recorded automatically for each session in one JSON file:

| Field | Meaning |
| --- | --- |
| `variant`, `botDelaySeconds`, `seed`, `tester` | A or B, its delay, the seed, and `owner` or `non-builder` (no names) |
| `startUtc`, `endUtc`, `elapsedSeconds` | Real time from the A/B button to victory or stop |
| `humanTurnSeconds` | Real time while the game waited for the human (reading, thinking, clicking) |
| `botTurnSeconds` | Real time while a bot was due to act; together with human time it equals elapsed time |
| `rolls`, `humanRolls`, `turns`, `rounds` | Rounds are turns ÷ 4 |
| `humanActions`, `botActions` | Human ROLL and piece clicks; bot commands |
| `meaningfulChoices`, `forcedChoices` | Human piece selections offering 2+ distinct outcomes, or exactly one. Summoning any Abyss piece is one outcome: every Abyss piece lands on the same start square |
| `equivalentSummonChoices` | Forced selections where several interchangeable Abyss pieces were offered; `meaningfulChoices + equivalentSummonChoices` equals DC-0006's legal-piece count |
| `humanNoMoveRolls`, `sixes` | Human rolls with no legal move; sixes by anyone |
| `knockouts`, `humanKnockoutsSuffered`, `summons` | All seats; human pieces Banished; summons |
| `ascensionEntries`, `completedPieces`, `humanCompletedPieces` | Moves off the shared track into an Ascension Path or home; pieces home |
| `winner`, `naturalVictory`, `aborted`, `abortSeconds`, `abortReason` | How the session ended |
| `longestSecondsWithoutMeaningfulChoice`, `secondsPerMeaningfulChoice`, `meaningfulChoicesPerMinute` | Decision density in real time |
| `answers` | The five questions and an optional note |

Derived in analysis: bot waiting share = `botTurnSeconds` ÷ `elapsedSeconds`.

DC-0006 counted any selection with 2+ legal pieces as a choice (median 32). With
interchangeable summons excluded, the same 1,000 seeds give a median of **30**
meaningful choices (p10 14, p90 45); a recorder test reproduces both numbers.

## Five Playtest Questions

Asked in the Editor window after every finished or stopped match:

1. How fun was the match? 1–5
2. How did the match length feel? Too short / Good / Too long
3. How much meaningful control did you feel you had? 1–5
4. How much of the time felt like waiting? 1–5
5. Would you immediately play another match? Yes / Maybe / No

Plus an optional short note.

## Procedure

1. Unity 6000.6.3f1: open `LocalMatch.unity`, press Play, then open
   `Demon Codex > M001.1 Playtest`.
2. Each tester plays one **pair**: A and B with the **same seed**. Suggested seeds
   82, 150 and 364 are the DC-0006 seeds closest to the median simulated length.
   The seed repeats the die rolls, not the match: different choices in A and B
   still lead to different positions, bot moves, length and winner.
3. Order: the owner plays A then B (seed 82). Testers who did not build the game
   play B then A, to balance learning and fatigue effects.
4. Play naturally until someone wins, or press STOP MATCH when you genuinely want
   to stop. Stopping is data, not failure.
5. Answer the five questions immediately. Tell testers only that they will play
   two matches; do not describe the variants or help unless asked.
6. Session files appear in `tests/evidence/dc-0037/sessions/`; commit them with a
   short A/B summary.

Minimum evidence: three recorded sessions, including the owner's A and B. Target:
three pairs (six sessions), including at least two people who did not build the
game. External sessions are recorded only when they actually happen.

## Provisional Targets

PROVISIONAL PRODUCT TARGETS — owner approval required:

| Measure | M001 baseline | Provisional target |
| --- | --- | --- |
| Full-match duration (median) | 8–12 min estimated, never measured | ≤ 10 min |
| Bot waiting share | 39–56% estimated | ≤ 15% (B alone is predicted to land near 20–30%) |
| Meaningful choice frequency | about one per 16–23 s estimated (30 per median match) | at least one every 20–30 s; longest gap reported |
| Fun (question 1) | unmeasured | mean ≥ 4 / 5 |
| Immediate replay (question 5) | unmeasured | majority Yes or Maybe |
| Natural completion | owner stopped (0 of 1) | every session ends in victory or a recorded stop reason |

## Decision Rules

The experiment succeeds if it **reduces uncertainty**, not if B wins. Required:
measured durations and bot waiting share; at least three human sessions including
the owner's A and B; victory or a recorded stop for every session; all ratings
recorded; an A/B comparison; no rules regression and unchanged deterministic play.

| If the evidence shows | Then next |
| --- | --- |
| B clearly improves pacing (for example ≥ 25% shorter, or felt length moves to Good) and waiting ratings improve | Propose fast bot pacing as the default (separate owner decision); next investigate decision density and game feel |
| A and B both feel too long | Next experiment tests board or rule pacing; configuration changes need owner approval, rule changes need DC-0002-style review |
| Length feels good but fun or control is low (≤ 3) | Work on meaningful choices, tension and risk/reward, not duration |
| Players enjoy the simple loop (fun ≥ 4, replay Yes/Maybe) | Preserve it; layer Demon Codex identity carefully, starting with [DC-0036](MASTER_BACKLOG.md) vocabulary |
| Players stop early | Record when and why (stop time, longest gap without a choice) before changing rules |
| Mixed or too few sessions | Add sessions before deciding; do not over-read one match |

Thresholds above (25%, ≤ 3, ≥ 4) are provisional.

## Evidence Format

One file per session, `tests/evidence/dc-0037/sessions/<startUtc>-<variant>-seed<seed>.json`
(schema version 1, fields as in Metrics). It is written as soon as the match ends,
is stopped, Play Mode exits or the playtest window closes, so timing survives
skipped questions, and it is rewritten when answers are saved. Abbreviated example:

```json
{
  "experiment": "M001.1", "variant": "VariantB", "botDelaySeconds": 0.25,
  "tester": "owner", "seed": 82, "elapsedSeconds": 512.4,
  "humanTurnSeconds": 401.9, "botTurnSeconds": 110.5,
  "naturalVictory": true, "winner": "P0", "aborted": false,
  "meaningfulChoices": 29, "forcedChoices": 31, "equivalentSummonChoices": 3,
  "answers": { "fun": 4, "matchLength": "good", "control": 3, "waiting": 2, "playAgain": "yes", "note": null }
}
```

The example values are illustrative, not results. Files contain no names,
accounts or machine identity; the optional note is the tester's own words.

Tooling validation, executed 2026-10-01, is automated evidence, **not** playtest
evidence:
- In the real scene with seed 82 and identical choices, A and B produced
  identical domain events.
- Measured time per bot command was 0.649 s in A and 0.250 s in B.
- Recorder counts match an independent tally and reproduce all 1,000 DC-0006
  pacing rows. Meaningful choices are checked against an independent rule: two
  options are equivalent only when they leave the same board.
- A Windows player build contains no experiment code or text.

## Known Limitations

- Few sessions; the owner built the game and knows which variant is running.
- Learning and fatigue effects between the two matches (order is counterbalanced).
- **A seed fixes the die rolls, not the match.** A and B with one seed get the same
  die values in the same seat order, but once the player chooses differently the
  positions, bot decisions, knockouts, length and winner can all diverge. The same
  roll can help in one match and be useless in the other, so this reduces dice-luck
  differences between A and B; it does not remove them. Repeated dice also make the
  second match of a pair slightly less novel.
- Human turn time includes reading, thinking and moving from the playtest window
  to the Game view after pressing A or B; the clock keeps running if the Editor is
  paused.
- Desktop Editor only, no mobile or touch. Each bot command can take up to one
  extra frame. No animation exists in either variant.
- DC-0006 simulation figures are proxy estimates; this experiment replaces them
  with measurements.

## Differentiation Question

Open and deliberately unanswered here: **What makes Demon Codex intrinsically
different from Ludo/Fia once theme, collection and economy are removed?**
M001.1 tests pacing only. Any answer comes after pacing evidence and must respect
the review gate for rule changes.

## Exit Gate

DC-0037 stays **REVIEW / TEST** until real human sessions exist. DONE requires:
the decision-rule evidence above; committed session files and an A/B summary; and
the owner's review choosing the next lever. If no non-builder sessions are
available, the owner must explicitly accept the reduced sample. Out of scope:
Codex collection, economy, Forge, blockchain, NFTs, backend, multiplayer,
progression, monetization, events, cards, power-ups and new demon abilities.
