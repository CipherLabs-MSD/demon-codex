# M001 Review — First Playable (v0.01)

Task: DC-0006 (validate M001 and capture playtest). Date: 2026-10-01.
Reviewed main: `f9d30b0ce38a44dd48d6690bb094dcba877ad7b6` (DC-0005 merged).
Status: **DONE** — M001 is closed. Product-owner decision: **ITERATE** (2026-10-01).
KR2–KR5 PASS; KR1 remains PARTIAL and carries into the next iteration.

## Objective

[O001](OKRS.md) (WORKING DECISION): *Prove that the core Demon Codex board-game loop
is playable and worth developing further.* [M001](MILESTONES.md) acceptance:
START → ROLL → SELECT → MOVE → KNOCK OUT → REACH HOME → WIN → RESTART, one human
against three bots, with automated and manual evidence satisfying the OKR.

The objective has two halves. **Playable** is technical and usability evidence.
**Worth developing further** is a product judgment that automated evidence cannot
make; it needs human play and the owner's decision.

## What Was Built

- DC-0002: approved M001 rules, including G1–G6 ([GDD](../GDD.md)).
- DC-0004: deterministic, parameterized C# rules core with invariants and replay
  ([evidence](DC_0004_EVIDENCE.md)).
- DC-0005: Unity 6 local match — one human vs three deterministic bots on the
  revisable M001 PROTOTYPE CONFIGURATION (L=40, starts 0/10/20/30, safe every 5,
  F=6), placeholder IMGUI board, PLAY AGAIN, and Editor-only endgame scenarios
  ([evidence](DC_0005_EVIDENCE.md)).
- DC-0006: this review, a read-only pacing analysis of the prototype configuration
  ([tool](../../tools/DemonCodex.Pacing.Cli/Program.cs)) and a fresh-checkout record.

Not built, by design: art, animation, sound, abilities, Codex/collection, economy,
backend, online play, mobile builds.

## Evidence

| Evidence | Result | Source |
| --- | --- | --- |
| Rules tests (T01–T25 matrix) | 48/48 in .NET, Unity EditMode and CI | [matrix](../../tests/M001_RULE_TEST_MATRIX.md) |
| Synthetic-board simulation (seeds 1–1000, 10,000-roll budget) | 1000/1000, 0 invariant failures, baseline byte-identical | [DC-0004](DC_0004_EVIDENCE.md) |
| Prototype-board simulation (same seeds, shipped session and bots) | 1000/1000, 0 invariant failures, 0 watchdogs, deterministic | [pacing summary](../../tests/evidence/dc-0006/pacing-summary.json) |
| Session + scenario tests | 14/14 | [DC-0005](DC_0005_EVIDENCE.md) |
| Unity EditMode / PlayMode | 62/62 and 3/3 (two complete scene matches plus restart) | [DC-0005](DC_0005_EVIDENCE.md) |
| Windows player build | compile and Editor-tooling isolation pass | [DC-0005](DC_0005_EVIDENCE.md) |
| Fresh clone of main, documented steps only | all pass, including Unity from an empty Library | [clean setup](../../tests/evidence/dc-0006/clean-setup.json) |
| Owner human acceptance | normal play worked; endgame checks A–E PASS | [DC-0005](DC_0005_EVIDENCE.md) |
| GitHub CI on main | foundation and rules workflows green | [workflows](../../.github/workflows/rules.yml) |

## O001 Key Results

| KR | Evidence required | Evidence found | Status |
| --- | --- | --- | --- |
| KR1 Complete a match from start through victory and restart | Reproducible human session record | Owner started and played a normal match (seed 2026) but stopped before the endgame. Victory and restart were human-accepted through reproducible seeded scenarios (D, E). PlayMode completes two full matches plus restart in the real scene. **No continuous human match from start to victory is recorded.** | **PARTIAL** |
| KR2 Seeded automated matches finish without invalid state | Seeds, commands, results, invariant checks and explicit timeouts | 2 × 1,000 seeded matches (synthetic boards; prototype board with shipped bots), per-seed CSV, invariants on every transition, 10,000-roll budget, 0 failures/watchdogs | **PASS** |
| KR3 Core approved rules pass automated tests | Rule-to-test mapping and passing run | T01–T25 matrix; 48/48 in .NET, Unity and CI | **PASS** |
| KR4 Clean setup works from documented instructions | Fresh-checkout build/run record | Fresh clone of `f9d30b0`: foundation, 48 + 14 .NET tests, simulation + baseline, Unity EditMode 62 and PlayMode 3 all pass. Same machine with tools preinstalled; not a new contributor. | **PASS** (caveats below) |
| KR5 Capture human playtest feedback and a continuation decision | Observations, friction points and owner go/iterate/stop decision | Observations and friction captured here (one owner session). Owner decision recorded: **ITERATE**, 2026-10-01 (see Continuation Decision). | **PASS** |

No KR is FAIL. KR1 is partial *because* of the pacing observation below; nothing
technical prevents a human from finishing a match. The owner closed M001 with KR1
still PARTIAL; a timed full human match is part of the next experiment.

## Human Playtest Findings

- Sample: one reviewer — the product owner, who also commissions the work and is
  not an independent player. One normal session in the Unity Editor, plus the A–E
  scenario checks. No session timings were recorded.
- Worked: launch, START MATCH, ROLL, legal selection, movement, bot turns,
  knockout, Ascension, completion, victory, PLAY AGAIN and a second match start.
- Friction: a normal match felt long enough that the owner stopped before the
  endgame. Development scenarios were then needed to reach the endgame quickly.
- Not observed by humans: a continuous full match, play by anyone other than the
  owner, learnability for a newcomer, replay desire, touch/mobile play.

## Technical Findings

- The rules core stayed deterministic and invariant-safe across 2,000 seeded
  matches on two board families. DC-0005 needed no rule change.
- Boundaries held: presentation never decides legality; Editor-only tooling is
  absent from the player build.
- Seat balance under identical bot policy: P0 254, P1 258, P2 247, P3 241 wins per
  1,000 — no visible first-player bias in this configuration.
- Clean setup reproduces from documentation. Frictions: the .NET SDK must be
  installed separately; Unity batch tests need the project closed in the Editor;
  after Unity runs, a clone is dirty (20 untracked default ProjectSettings files and
  a whitespace-only rewrite of `ProjectSettings.asset`); Python 3.10.6 worked
  although README asks for 3.11+.
- Unverified: iOS/Android, touch, IL2CPP and device performance (the primary
  platform direction per [ADR 0002](../adr/0002-unity-6-engine.md)).

## Product Findings

- The loop is understandable enough for the owner to accept every endgame state.
- There is **no evidence yet** that the loop is fun, tense or replayable, or that it
  feels distinct from Ludo/Fia. Automated tests cannot provide this evidence.
- Mechanically the prototype stays close to its public-domain mechanic family, as
  intended for M001. The GDD requires Demon Codex to develop its own progression,
  terminology, abilities and presentation; none is expressed in play yet.
- Vocabulary already diverges (knockout vs Banished/Banishment; home vs
  completion; Abyss, Summon, Ascension Path are provisional). See Deferred Work.

## Pacing Observation

The owner's pause is genuine product feedback, **not proof that the game is too
long**. To ground it, the prototype board was simulated for seeds 1–1000 with the
shipped session and bots; P0 uses the bot policy as a proxy for human choices.

| Per match (median, p10–p90) | Value |
| --- | --- |
| Rolls / turns | 316 (239–392) / 263 (200–329), about 66 rounds |
| Human actions (ROLL + piece clicks) | 142 (106–181) |
| Human selections with a real choice (2+ legal pieces) | 32 (17–48) |
| Human selections forced (exactly one legal piece, still a click) | 30 (18–43) |
| Human rolls with no legal move | 21% of human rolls |
| Knockouts (all seats / by human / suffered by human) | 13 / 3 / 3 |
| Human's first summon | 4th human turn (p90: 13th) |
| First completion, any seat / human's 4th completion when winning | round 15.5 / round 64 |
| Pure bot waiting at 0.65 s per bot command | 4.6 min (3.5–5.7) |
| Estimated match length at 1.5 s / 3 s per human action | 8.1 min (6.2–10.2) / 11.7 min (8.9–14.7) |

The human-action times are an ASSUMPTION, not measured; first-time play with
reading and thinking is likely slower. About 77% of human actions involve no
decision (each ROLL plus forced picks), and bot waiting is roughly 39–56% of the
estimate.

Candidate explanations, none yet tested in isolation:

| Explanation | Evidence so far |
| --- | --- |
| Bot pacing | Supported as a contributor: ~4.6 min of pure waiting per match |
| Low decision density | Supported: ~32 real choices among ~142 actions |
| Board configuration (L=40, F=6) | Plausible; drives ~66 rounds; revisable configuration |
| Raw presentation (snap moves, no animation or feedback) | Plausible; untested |
| No progression or rewards | Plausible; absent by M001 design |
| Underlying rules pacing | Partial: summoning is quick (median 4th turn), while knockouts and exact completion stretch the endgame |

## Open Questions

Smallest set that matters before expanding scope, highest priority first:

1. How long does a real human full match take now, and what length feels right?
2. Are there enough meaningful decisions per minute, or does rolling and forced
   clicking dominate the experience?
3. How much of the perceived length is waiting for bots?
4. Are knockout and Ascension satisfying, or merely visible?
5. Do sixes, knockouts and comebacks create tension and replay desire?
6. What will make Demon Codex distinct from Ludo/Fia (for example demon abilities
   or board events)? Defer until questions 1–3 have evidence.

## Risks

- Building collection, economy or Forge on an unvalidated core loop.
- Sample bias: one owner session is the only human evidence.
- Misattributing length to rules when bot delay or presentation dominates, or the
  reverse; levers must be tested separately.
- Proxy metrics differ from real human choices and timing.
- Changing approved rules prematurely; rule changes need owner approval and review.
- Perception as a generic Ludo clone without a distinct identity.
- The primary platform (mobile) remains unverified.

## Continuation Decision

Framework (repository): KR5 requires an **owner go/iterate/stop decision**; the
M001 exit gate requires that a reviewer confirms all acceptance evidence and the
product owner records whether to iterate or proceed.

Reviewer confirmation: technical viability and the playable loop are evidenced
(KR2–KR4 PASS). Before the owner decision, KR1 and KR5 were PARTIAL. "Worth
developing further" is not yet evidenced.

Reviewer recommendation: ITERATE.

- Not STOP: no blocking defect and no negative signal beyond an unexplained length
  concern.
- Not GO: proceeding to Game Feel, Codex or collection scope would build on the
  largest open uncertainty — whether the core loop is enjoyable at its length.
- ITERATE: run one focused pacing and fun playtest before expanding scope.

### Owner decision — ITERATE (2026-10-01)

Recorded by the product owner. Rationale, as given:

- Technical viability is proven.
- The deterministic rules engine is stable.
- The human-vs-bots prototype works.
- Automated evidence is strong.
- Core human interaction has been validated.
- Fun, pacing, tension, replay value and differentiation from generic Ludo/Fia
  are not yet sufficiently validated.
- The owner paused a normal match before the endgame because it felt long. This
  is a product signal, not proof of a specific pacing defect.
- Therefore the project continues with a focused pacing/fun iteration before
  larger systems such as collection, economy, Forge, backend, blockchain or NFTs.

This record completes KR5 and the M001 exit gate. M001 is closed; DC-0006 is DONE.

## Recommended Next Experiment

PROPOSAL — direction endorsed by the ITERATE decision; scope, targets and start
still need owner approval. Not started:
**M001.1 Pacing and fun playtest** ([DC-0037](MASTER_BACKLOG.md)).

Question: is a complete human match enjoyable at its current length, and which
lever matters most — bot pacing, decision density or board length?

1. Baseline: at least three complete human matches on the current build, including
   at least two players who did not build it. Time each match with a stopwatch.
   After each, ask five questions: fun (1–5), felt length, best moment, confusion,
   would play again.
2. At most one variant, chosen by the owner, using only revisable levers that do
   not change approved rules, for example bot delay 0.65 s → 0.25 s (about 1.8 min
   of waiting instead of 4.6) or a shorter prototype track length.
3. The owner reviews results and chooses the next lever, or confirms pacing is fine.

| Measure | M001 baseline | Proposed target (owner to confirm) |
| --- | --- | --- |
| Players who finish a full match without stopping | Owner stopped (0 of 1) | every tester |
| Measured median full-match minutes | unmeasured (estimate 8–12) | measure first; owner sets the bound |
| Bot waiting share of match time | estimated 39–56% | under 25% if the delay variant is tested |
| Real choices per match | 32 (proxy) | record actual; compare with fun score |
| Fun / would play again | unmeasured | owner sets the threshold before testing |

Out of scope for the experiment: Codex collection, economy, Forge, blockchain,
NFTs, backend, multiplayer, LiveOps, monetization and demon roster production.

## Deferred Work

- PROPOSAL [DC-0036](MASTER_BACKLOG.md) **Demon Codex Lexicon & Naming Bible**:
  controlled vocabulary for demons, titles, players, pieces, turns, rolls, sixes,
  board and safe spaces, knockout/Banishment, Abyss, Ascension, completion,
  victory, events, seasons, rarities, factions, Forge, Altar and Codex terms. No
  names are chosen and no demons are generated in DC-0006.
- Unity ProjectSettings hygiene found by the clean setup (commit Unity defaults or
  ignore them; decide whether Unity YAML is exempt from whitespace checks).
- Game feel (DC-0007), art (DC-0009), Codex (DC-0010), economy/Forge/Altar
  (DC-0011–DC-0014), social, backend, LiveOps, commercial, blockchain and content
  scale-up remain BACKLOG behind the continuation decision.
- Mobile/touch/IL2CPP smoke test before any platform claim; permanent board values,
  final bot strategy and stalemate policy stay UNKNOWN.

Reproduce the pacing analysis:
`dotnet run --project tools/DemonCodex.Pacing.Cli -c Release -- 1000 1 artifacts/pacing 10000`
