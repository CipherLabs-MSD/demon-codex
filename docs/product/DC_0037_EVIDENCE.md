# DC-0037 — M001.1 interim playtest evidence

Status: **REVIEW / TEST — interim evidence, not a decision.** Updated 2026-10-02
from the raw session files. Contract: [M001.1 Pacing & Fun Experiment](M001_1_PACING_FUN_EXPERIMENT.md).

## Sessions recorded

Three owner sessions on seed 82, recorded with the reviewed tooling. Raw files are
committed byte-for-byte; [summary.json](../../tests/evidence/dc-0037/summary.json)
holds the derived values and SHA-256 hashes.

| # | Session | Type | Result |
| --- | --- | --- | --- |
| 1 | [CONTROL A, 13:20 UTC](../../tests/evidence/dc-0037/sessions/20261002T132011Z-ControlA-seed82.json) | Stopped session | Stopped by player at 4:04 |
| 2 | [VARIANT B, 13:24 UTC](../../tests/evidence/dc-0037/sessions/20261002T132457Z-VariantB-seed82.json) | Stopped session | Stopped by player at 0:30 |
| 3 | [VARIANT B, 21:43 UTC](../../tests/evidence/dc-0037/sessions/20261002T214300Z-VariantB-seed82.json) | **Full match** | Natural victory at 2:30; winner P3 (Bot III) |

Session 3 is the first full human match measured in the project. It was the
owner's third session on seed 82, so the dice were the same ones already seen in
sessions 1 and 2.

## Data checks

All checks pass for all three files:
- **Format:** schema and required fields are present, and each file name matches
  its start time, variant and seed.
- **Time:** human turn time plus bot time equals elapsed time, and the UTC start
  and end span matches elapsed time.
- **Counts:** human actions equal rolls plus selections, and selections equal rolls
  without a legal move subtracted.
- **Answers:** all five are saved in every file.
- **Seed 82:** replaying the real session die reproduces every session. The first
  138 rolls give 23 sixes and 34 human rolls (session 1); the first 52 give 8 sixes
  and 14 human rolls (session 2); the first 297 give 45 sixes, 76 human rolls and
  252 turns, ending on a 1 rolled by P3, the recorded winner (session 3).
- **Variant delay:** it was applied during real play, at 0.654 s per bot action in
  A and 0.250–0.251 s in both B sessions.

## Comparison

Full matches and stopped sessions are shown separately. A stopped session's time
is **not** a match duration.

| Measure | **Full match:** B #3 | Stopped: A #1 | Stopped: B #2 |
| --- | --- | --- | --- |
| Ending | Natural victory (P3 / Bot III) | Stopped at 4:04 | Stopped at 0:30 |
| Reached | 63 rounds, 297 rolls | 28.75 rounds, 138 rolls | 11.25 rounds, 52 rolls |
| Elapsed time | **150.1 s (2:30), full match** | 244.1 s (partial) | 30.4 s (partial) |
| Bot waiting | 94.5 s (63.0%) | 116.5 s (47.7%) | 14.3 s (47.0%) |
| Human turn time per action | 0.42 s | 2.46 s | 0.62 s |
| Time per round | 2.4 s | 8.5 s | 2.7 s |
| Meaningful choices | 39 (17 forced, 2 interchangeable summons) | 5 (13 forced, 2) | 2 (10 forced, 1) |
| Meaningful choices per minute | 15.6 (one per 3.8 s) | 1.23 | 3.95 |
| Longest stretch without a meaningful choice | 40.0 s | 117.1 s | 18.5 s |
| Human rolls with no legal move | 20 of 76 (26%) | 16 of 34 (47%) | 2 of 14 (14%) |
| Knockouts (all / suffered by human) | 10 / 2 | 6 / 1 | 2 / 1 |
| Human pieces home | 3 of 4 | 1 | 0 |
| Q1 fun (1–5) | 3 | 2 | 4 |
| Q2 match length | Good | Too long | Good |
| Q3 meaningful control (1–5) | 4 | 2 | 3 |
| Q4 time that felt like waiting (1–5) | 3 | 2 | 1 |
| Q5 play again immediately | Maybe | No | Maybe |
| Note | none | none | none |

Session 3's 63 rounds and 297 rolls are close to the DC-0006 simulated median
(65.75 rounds, 316 rolls), so it was a typical-length match, not a short one.

## Owner feedback after the first A/B pair

Given by the product owner on 2026-10-02 after sessions 1 and 2, verbatim:

> Spelet är fortfarande en tidig prototyp. Avsaknaden av utvecklad grafik och
> intressant visuell presentation bidrar till att det känns tråkigt, vilket är
> förståeligt i detta stadium. Det snabbare tempot kändes roligare. Kommentstadium.

Translation: "The game is still an early prototype. The lack of developed graphics
and interesting visual presentation contributes to it feeling boring, which is
understandable at this stage. The faster pace felt more fun."

How it bears on the evidence:
- It matches the owner's ratings: the faster pace (B) felt more fun.
- It names placeholder presentation as one reason the game feels boring. Graphics
  are **identical in A and B**, so they cannot explain the A/B difference, but they
  may lower fun scores in both. That is a limitation, not a change to the target.
- It is general feedback, not a stated reason for either stop.

## Provisional targets against the evidence

The targets are PROVISIONAL and unchanged. The fun target keeps its original
definition: a mean of at least 4 out of 5 on question 1.

| Provisional target | Evidence | Reading |
| --- | --- | --- |
| Full-match median ≤ 10 min | B: one full match, 2.5 min; A: no full match | Met for B (n = 1); unknown for A |
| Bot waiting ≤ 15% of match time | B full match 63% | Not met |
| A meaningful choice at least every 20–30 s | B full match: one per 3.8 s on average, longest gap 40 s | Average met; one gap above 30 s |
| Fun mean ≥ 4 / 5 | Full match 3; all sessions 3.0 (B sessions 3.5) | Not met |
| Majority Yes or Maybe for immediate replay | Maybe, No, Maybe | Met (2 of 3) |
| Every session ends in victory or a recorded stop | 1 victory, 2 recorded stops | Met; stop reasons not stated |

## What the results support

- **A full match at fast bot pacing works for the owner.** Session 3 reached a
  natural victory at a typical length in 2.5 minutes, and the owner rated its
  length "good".
- **Length is not the owner's problem with B.** Both B sessions were rated
  "good" length. At normal pace (A) the owner stopped at 4:04 and rated it too
  long, as also happened in M001.
- **Fun is still below the provisional target over a full match**, at 3 / 5. The
  30-second B fragment scored higher (4) than the full B match (3), and felt
  waiting rose from 1 to 3, so the short fragment overstated how good B feels.
- **Choices were frequent in the full B match** (15.6 per minute), but the owner
  spent only 0.42 s per action. The recorder counts choices that were *available*;
  it cannot show whether they were *deliberate*.
- **Bots dominate elapsed time even with fast pacing** (63%), because the owner
  acts quickly.

## What remains uncertain

- **No full CONTROL A match exists**, so there is no full-match A/B comparison.
  At A's observed pace (8.5 s per round), 63 rounds would take about 9 minutes, but
  that is an extrapolation, not a measurement.
- **The causes are confounded.** Session 3 was the third play of the same dice and
  came last, and the owner knew which variant was running. Familiarity and order
  could explain part of B's better ratings.
- **Why sessions 1 and 2 were stopped** is not stated in those sessions' notes.
- **Generality:** one tester, who built the game; no non-builder sessions.
- **Outcome effect:** the owner lost session 3 with 3 of 4 pieces home. A loss
  may affect ratings.
- **Presentation** may lower fun in both variants. Game feel belongs to
  [DC-0007](MASTER_BACKLOG.md), not this experiment.
- The recorder stores totals only, so it cannot show *when* gaps or no-move
  streaks happened.
- The [differentiation question](M001_1_PACING_FUN_EXPERIMENT.md#differentiation-question)
  is untouched.

## Exit gate check

| Exit gate item | Status |
| --- | --- |
| At least 3 human sessions including the owner's A and B | **Met** (3 sessions) |
| Measured durations | Partly met: one full match (B); A only partial |
| Bot waiting share measured | Met for all sessions, including the B full match |
| Every session ends in victory or a recorded stop | Met; reasons for the two stops not stated |
| All ratings recorded | Met (5 of 5 answers in all 3 sessions) |
| A/B comparison | Partly met: B full match against A stopped session only |
| No rules regression; deterministic play unchanged | Met (no rule code changed; CI green) |
| Committed session files and A/B summary | Met (this document and summary.json) |
| Non-builder sessions, or explicit owner acceptance of the reduced sample | **Not met** |
| Owner review choosing the next lever | **Not met** |

**The exit gate is not met.** DC-0037 stays REVIEW / TEST.

Read through the contract's decision rules, the full B match fits the row "length
feels good but fun or control is low (≤ 3)", with fun at 3. That row points toward
meaningful choices, tension and risk/reward rather than duration. The waiting
rating did not improve over A (3 against 2), so the row "B clearly improves
pacing and waiting ratings improve" is not fully met. The contract's small-sample
rule also applies, so this is a reading of the evidence, not a decision.
