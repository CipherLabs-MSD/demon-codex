# DC-0037 — M001.1 interim playtest evidence

Status: **REVIEW / TEST — interim evidence, not a decision.** Compiled 2026-10-02
from the raw session files. Contract: [M001.1 Pacing & Fun Experiment](M001_1_PACING_FUN_EXPERIMENT.md).

## Sessions recorded

Two owner sessions, played on 2026-10-02 in the order the contract asks for:
CONTROL A, then VARIANT B, both on seed 82. They were recorded with the reviewed
tooling (the files include `equivalentSummonChoices`). The raw files are committed
byte-for-byte; [summary.json](../../tests/evidence/dc-0037/summary.json) holds the
derived values and SHA-256 hashes.

- [CONTROL A session](../../tests/evidence/dc-0037/sessions/20261002T132011Z-ControlA-seed82.json)
- [VARIANT B session](../../tests/evidence/dc-0037/sessions/20261002T132457Z-VariantB-seed82.json)

## Data checks

All checks pass for both files:
- **Format:** schema and required fields are present, and each file name matches
  its start time, variant and seed.
- **Time:** human turn time plus bot time equals elapsed time, and the UTC start
  and end span matches elapsed time.
- **Counts:** human actions equal rolls plus selections, and selections equal rolls
  without a legal move subtracted.
- **Answers:** all five are saved in both files.
- **Seed 82:** replaying the real session die shows that the first 138 rolls give
  23 sixes and 34 human rolls (A), and the first 52 give 8 sixes and 14 human rolls
  (B), exactly as recorded. B's 52 rolls are A's first 52.
- **Variant delay:** it was applied during real play, at 0.654 s per bot action in
  A and 0.251 s in B.

## Comparison

| Measure | CONTROL A (0.65 s bots) | VARIANT B (0.25 s bots) |
| --- | --- | --- |
| Ending | Stopped by player at 4:04 | Stopped by player at 0:30 |
| Reached | 28.75 rounds, 138 rolls | 11.25 rounds, 52 rolls |
| Elapsed time | 244.1 s | 30.4 s |
| Bot waiting | 116.5 s (47.7%) | 14.3 s (47.0%) |
| Human turn time per action | 2.46 s | 0.62 s |
| Time per round | 8.5 s | 2.7 s |
| Meaningful choices | 5 (13 forced, 2 of them interchangeable summons) | 2 (10 forced, 1 interchangeable) |
| Meaningful choices per minute | 1.23 | 3.95 |
| Longest stretch without a meaningful choice | 117.1 s | 18.5 s |
| Human rolls with no legal move | 16 of 34 (47%) | 2 of 14 (14%) |
| Pieces home (human / all seats) | 1 / 2 | 0 / 0 |
| Q1 fun (1–5) | 2 | 4 |
| Q2 match length | Too long | Good |
| Q3 meaningful control (1–5) | 2 | 3 |
| Q4 time that felt like waiting (1–5) | 2 | 1 |
| Q5 play again immediately | No | Maybe |
| Note | none | none |

## Owner feedback after the A/B sessions

Given by the product owner on 2026-10-02, verbatim:

> Spelet är fortfarande en tidig prototyp. Avsaknaden av utvecklad grafik och
> intressant visuell presentation bidrar till att det känns tråkigt, vilket är
> förståeligt i detta stadium. Det snabbare tempot kändes roligare. Kommentstadium.

Translation: "The game is still an early prototype. The lack of developed graphics
and interesting visual presentation contributes to it feeling boring, which is
understandable at this stage. The faster pace felt more fun."

How it bears on the evidence:
- It confirms the owner's ratings: the faster pace (B) felt more fun.
- It names placeholder presentation as one reason the game feels boring. Graphics
  are **identical in A and B**, so they cannot explain the A/B difference. They
  can, however, depress absolute fun scores in both variants, so with placeholder
  visuals the fun target (mean ≥ 4/5) is better read as A versus B than as an
  absolute bar.
- It is general feedback, not a stated reason for either specific stop.

## What the results support

- The tooling records complete, internally consistent sessions in real use, and
  B's faster pacing is actually applied.
- The owner again stopped a normal-pace match before victory: about 4 minutes in,
  at round 29, rating it too long, fun 2 and no replay. Together with the M001
  pause, that is two owner stops at normal pace (owner-only evidence).
- In that A segment, real decisions were sparse: 5 meaningful choices in 4 minutes
  (one per 49 s, against a provisional target of one every 20–30 s), a 117 s
  stretch with none, and nearly half of the human rolls with no legal move.
- In the owner's ratings and later comment, the faster pace felt more fun; the short
  B session scored better on all five questions.

## What remains uncertain

- **Full-match duration is still unmeasured** for both variants; neither session
  reached victory.
- **B is a 30-second fragment.** Its "good" length and fun 4 describe 11 rounds,
  not a match, so they cannot show that B fixes a full match.
- **The causes are confounded.** The ratings may differ because of bot pacing,
  because B covered only the early game (14% no-move rolls versus 47% across A),
  because B came second, or because the owner knew which variant was running.
- **Bot waiting share did not fall** (about 47% in both), because the owner also
  acted four times faster in B. Whether B lowers waiting over a full match is open.
- **The pacing driver is unclear.** In A the owner rated waiting low (2) while
  fun and control were also low, which hints that sparse decisions may matter as
  much as bot speed. That is a hypothesis, not a finding.
- **Why each session was stopped is not stated**; both notes are empty. The owner's
  later comment (placeholder presentation feels boring) is general feedback.
- **Presentation is a separate open factor.** Placeholder visuals may lower fun in
  both variants, so absolute fun scores now mix pacing with presentation; game feel
  belongs to [DC-0007](MASTER_BACKLOG.md), not this experiment.
- **Generality:** one tester, who built the game, and one session per variant.
- The recorder stores totals only, so it cannot show *when* in A the 117 s gap or
  the no-move streak happened.
- Rough extrapolation, not a measurement: at the observed pace, a full match of
  about 66 rounds (DC-0006 simulated median) would take about 9 minutes in A and
  3 in B. Later rounds may run slower, with more choices and exact completion.
- The [differentiation question](M001_1_PACING_FUN_EXPERIMENT.md#differentiation-question)
  is untouched.

## Missing evidence

| Contract item | Status |
| --- | --- |
| At least 3 sessions including the owner's A and B | 2 of 3 |
| Target: 3 pairs including at least 2 non-builders | 1 owner pair; 0 non-builder sessions |
| Full-match duration measured | Missing: no natural victory yet |
| Every session ends in victory or a recorded stop reason | Stops recorded; per-session reasons not stated (general owner feedback recorded) |
| All five answers per session | Complete in both sessions |
| A/B comparison | Interim only (table above) |
| Owner decision on the next lever | Not yet |

Per the contract's decision rules, early stops must be explained (when **and**
why) and too few sessions mean adding sessions before deciding. The provisional
targets refer to full matches, so they cannot be judged on two stopped sessions.
DC-0037 stays REVIEW / TEST.
