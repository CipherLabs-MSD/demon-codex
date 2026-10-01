# Initial OKR

WORKING DECISION — O001: **Prove that the core Demon Codex board-game loop is
playable and worth developing further.** Owner: product owner; milestone: M001.

| KR | Evidence required | Status |
| --- | --- | --- |
| KR1 Complete a match from start through victory and restart | Reproducible human session record | PARTIAL — no continuous human match to victory; endgame and restart human-accepted via seeded scenarios |
| KR2 Seeded automated matches finish without invalid state | Seeds, commands, results, invariant checks and explicit timeouts | PASS — 2 × 1,000 seeded matches, 0 failures |
| KR3 Core approved rules pass automated tests | Rule-to-test mapping and passing run | PASS — T01–T25, 48/48 |
| KR4 Clean setup works from documented instructions | Fresh-checkout build/run record | PASS — fresh clone record, same machine |
| KR5 Capture human playtest feedback and a continuation decision | Observations, friction points and owner go/iterate/stop decision | PARTIAL — feedback captured; owner decision pending |

No retention, revenue, simulation count or performance target is invented before
baseline data. Statuses as of 2026-10-01 are justified in the [M001 review](M001_REVIEW.md).
Before implementing KR2, choose a reviewed seed suite and termination
budget; report hangs as failures, not successful matches. Foundation validation
does not satisfy these playable KRs.
