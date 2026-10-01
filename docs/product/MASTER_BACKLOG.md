# Master backlog

**This backlog represents what we can see now and will evolve with evidence.**

WORKING DECISION: stable DC-0001 style IDs are never reused or renumbered. New tasks
use the next unused number. Epics are the workstream names below. Later rows are
PROPOSAL discovery work, not commitments or authorization to implement systems.
Owner is an accountable role pending human assignment unless explicitly recorded; no agent is dispatched.
P0 precedes P1, P2 and P3. M001 rows support O001 / KR1–KR5; later work is unassigned
to an OKR until reviewed. F001 is the foundation prerequisite.

States: BACKLOG, READY, IN PROGRESS, REVIEW / TEST, BLOCKED, DONE.
READY requires clear acceptance and satisfied dependencies; DONE requires linked
actual evidence and review. The Evidence column specifies expected artifacts for open tasks and actual
evidence for completed tasks. A blocked task records reason, owner and unblock action in its issue.
Foundation v0.1 is human-approved; DC-0001 is DONE. The owner approved the
DC-0002 specification and G1–G6 rulings; DC-0002 is DONE. Remaining board parameters
and separate design policies do not block this approval. DC-0004 is DONE after explicit human approval and successful Unity EditMode
validation (48 passed, 0 failed). DC-0005 is DONE after owner acceptance on 2026-10-01 (endgame checks A–E PASS)
and executed automated evidence. DC-0006 is DONE: the product owner recorded ITERATE on 2026-10-01 and M001 is closed
([M001 review](M001_REVIEW.md)). DC-0036 is a PROPOSAL row, BACKLOG and not started. DC-0037 (M001.1) is REVIEW / TEST: the experiment
contract and Editor-only A/B tooling are ready; human playtest sessions are pending.
The product owner explicitly
resolved DC-0003 independently through ADR 0002; its prior foundation-review
dependency is waived for that decision only. Other foundation outputs do not imply approval.

| ID | Epic | Task | Owner/agent | Priority | Dependencies | Acceptance criteria | Evidence required | Status | Milestone |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| DC-0001 | Foundation | Review foundation controls | Product owner | P0 | None | README navigation, boundaries and validation reviewed | [Foundation review](FOUNDATION_REVIEW.md); explicit owner approval in DC-0002 request | DONE | F001 |
| DC-0002 | Core Game | Specify board and turn rules | Product owner (review); rules agent (specification) | P0 | DC-0001 | Specify supplied mechanics, resolve G1–G6, and record explicit topology unknowns with owner approval | [Rules and edge cases](../GDD.md), [test matrix](../../tests/M001_RULE_TEST_MATRIX.md), [review record](DC_0002_REVIEW.md); explicit human approval of PR #2 / G1–G6 | DONE | M001 |
| DC-0003 | Core Game | Accept Unity 6 engine decision | Product owner | P0 | None (human decision; see note above) | Record accepted Unity 6 decision, mobile direction and local M001 boundaries | [Accepted ADR 0002](../adr/0002-unity-6-engine.md); explicit human approval | DONE | M001 |
| DC-0004 | Core Game | Implement deterministic rules core | Core Game lead (unassigned) | P0 | DC-0002, DC-0003 | Legal moves, turns, knockout, home and win follow approved rules | [Implementation/evidence](DC_0004_EVIDENCE.md): 48 .NET tests, 1000 completed matches, 48 Unity EditMode tests passed; human-approved | DONE | M001 |
| DC-0005 | Core Game | Build local match UI and bots | Core Game lead (unassigned) | P0 | DC-0004 | One human and three bots can start, finish and restart a match | [Playable scene and evidence](DC_0005_EVIDENCE.md): 48 rules + 14 session/scenario .NET tests, 1000 matches, 62 Unity EditMode + 3 PlayMode tests; owner acceptance A–E PASS 2026-10-01 | DONE | M001 |
| DC-0006 | Release | Validate M001 and capture playtest | Release lead (unassigned) | P0 | DC-0005 | All O001 KRs evidenced including simulations and clean setup | [M001 review](M001_REVIEW.md): KR2–KR5 PASS, KR1 PARTIAL; 2 × 1,000 seeded matches, prototype pacing baseline, fresh-checkout record, owner playtest; owner decision ITERATE 2026-10-01 | DONE | M001 |
| DC-0007 | Game Feel | Explore readable feedback | Game Feel lead (unassigned) | P1 | DC-0006 | Review movement, dice and knockout feedback with accessibility notes | Recorded comparison and feedback | BACKLOG | Later |
| DC-0008 | Demon Data | Review public entity contract | Demon Data lead (unassigned) | P1 | DC-0001 | Review schema, uniqueness and migration; no roster generation | Schema review and future validator plan | BACKLOG | Later |
| DC-0009 | Art | Approve small visual exploration brief | Art lead (unassigned) | P1 | DC-0007 | Define silhouettes, rights and readability evaluation | Owner-reviewed brief | BACKLOG | Later |
| DC-0010 | Codex | Specify discovery and ownership UI | Codex lead (unassigned) | P1 | DC-0008 | Separate numbered completion from ownership and anomalies | Reviewed wireflow using placeholders | BACKLOG | Later |
| DC-0011 | Economy | Document resource hypotheses | Economy lead (unassigned) | P1 | DC-0006 | Separate sources, sinks and untested assumptions without live parameters | Reviewed hypothesis map | BACKLOG | Later |
| DC-0012 | Drops | Specify reward fairness and observability | Drops lead (unassigned) | P2 | DC-0011 | List reward constraints and required review before probabilities | Reward proposal and evidence plan | BACKLOG | Later |
| DC-0013 | Forge | Design one candidate recipe | Forge lead (unassigned) | P2 | DC-0011 | Explain duplicate utility, resource consumption and failure safety | Recipe proposal; no balance commitment | BACKLOG | Later |
| DC-0014 | Altar | Evaluate sacrifice purpose | Altar lead (unassigned) | P2 | DC-0011 | Assess value, irreversible loss protections and whether to defer | Decision memo | BACKLOG | Later |
| DC-0015 | Mythics | Review encounter interface | Mythics lead (unassigned) | P2 | DC-0008 | Separate scarcity dimensions, outcomes and witness history | Redacted interface review | BACKLOG | Later |
| DC-0016 | Restricted Forged architecture | Design compartmentalized authoring access | Restricted Forged architecture lead (unassigned) | P1 | DC-0030 | Specify least privilege and reveal approval without narrative payloads | Redacted access model | BACKLOG | Later |
| DC-0017 | Social | Evaluate social needs | Social lead (unassigned) | P2 | DC-0006 | Identify value, moderation and privacy before online scope | Reviewed discovery notes | BACKLOG | Later |
| DC-0018 | Trading | Assess transfer risks | Trading lead (unassigned) | P2 | DC-0017, DC-0031 | Define fraud, ownership and moderation questions before market design | Risk review and go/defer decision | BACKLOG | Later |
| DC-0019 | Backend | Justify first backend boundary | Backend lead (unassigned) | P2 | DC-0017, DC-0030 | Compare service need with local prototype and threat model | Human-reviewed ADR proposal | BACKLOG | Later |
| DC-0020 | LiveOps | Draft reversible event process | LiveOps lead (unassigned) | P2 | DC-0011 | Define approvals, reward safety, rollback and release packets | Reviewed runbook proposal | BACKLOG | Later |
| DC-0021 | Commercial | Evaluate non-pay-to-win offers | Commercial lead (unassigned) | P2 | DC-0006, DC-0031 | Map offers to fair play and platform review needs | Commercial hypothesis review | BACKLOG | Later |
| DC-0022 | Distribution | Evaluate target launch platforms | Distribution lead (unassigned) | P1 | DC-0003, DC-0031 | Record device, store and release constraints from current sources | Dated source-backed review | BACKLOG | Later |
| DC-0023 | Finance | Define financial reporting categories | Finance lead (unassigned) | P2 | DC-0021 | Clarify fees, taxes, refunds, COGS and margin definitions with owner | Reviewed model specification; no spending | BACKLOG | Later |
| DC-0024 | Growth | Define content engine experiment | Growth lead (unassigned) | P1 | DC-0006 | Choose reusable approved content and baseline measurement | Reviewed content brief | BACKLOG | Later |
| DC-0025 | Creators | Design creator attribution proposal | Creators lead (unassigned) | P2 | DC-0024, DC-0032 | Define IDs, funnel and compensation options without contracts | Reviewed attribution and consent proposal | BACKLOG | Later |
| DC-0026 | Community | Draft moderation and community scope | Community lead (unassigned) | P2 | DC-0024 | Define channel purpose, staffing and escalation | Reviewed community plan | BACKLOG | Later |
| DC-0027 | Referral | Evaluate abuse-resistant referrals | Referral lead (unassigned) | P2 | DC-0025 | Define eligibility, abuse risks and privacy constraints | Referral proposal | BACKLOG | Later |
| DC-0028 | PR | Prepare approved public narrative brief | PR lead (unassigned) | P2 | DC-0024 | Use released material only with publication approval gate | Redacted approved brief | BACKLOG | Later |
| DC-0029 | Paid Acquisition | Establish experiment prerequisites | Paid Acquisition lead (unassigned) | P2 | DC-0032, DC-0023 | Require baseline, consent, budget approval and stop conditions | Experiment proposal; no ads or spend | BACKLOG | Later |
| DC-0030 | Security | Draft initial threat and access model | Security lead (unassigned) | P1 | DC-0001 | Cover client, content, secrets, agent access and future player data | Reviewed threat model | BACKLOG | Later |
| DC-0031 | Legal/Platform | Identify review requirements | Legal/Platform lead (unassigned) | P1 | DC-0001 | List rights, privacy, age, commerce and store questions for qualified review | Dated authoritative sources and owner actions | BACKLOG | Later |
| DC-0032 | Analytics | Define baseline events and consent | Analytics lead (unassigned) | P1 | DC-0006, DC-0031 | Specify activation, retention and privacy with no invented targets | Reviewed event dictionary proposal | BACKLOG | Later |
| DC-0033 | Physical Products | Evaluate serial-linked artifacts | Physical Products lead (unassigned) | P2 | DC-0021, DC-0031 | Consider rights, manufacturing, redemption and returns | Feasibility proposal; no fulfillment | BACKLOG | Later |
| DC-0034 | Blockchain Later | Assess exceptional provenance need | Blockchain Later lead (unassigned) | P3 | DC-0033, DC-0030 | Compare signed certificates and optional chain; ordinary play independent | Human go/defer ADR; no deployment | BACKLOG | Later |
| DC-0035 | Content Scale-up | Define staged catalog quality gates | Content Scale-up lead (unassigned) | P2 | DC-0008, DC-0009 | Set review process toward 66 then 666 without generating roster | Content approval and rights checklist | BACKLOG | Later |
| DC-0036 | Lexicon | Create Demon Codex Lexicon & Naming Bible (PROPOSAL) | Lore lead (unassigned) | P1 | DC-0006 | Controlled, labeled vocabulary for demon, player, board, match, collection and system terms; naming rules only, no roster or restricted lore | Owner-reviewed lexicon proposal | BACKLOG | Later |
| DC-0037 | Core Game | Run pacing and fun playtest | Product owner (decision); Core Game lead (unassigned) | P0 | DC-0006 | Experiment contract; Editor-only CONTROL A (0.65 s bots) vs VARIANT B (0.25 s bots) with rules unchanged; 3+ recorded human sessions incl. owner A and B (target 2+ non-builders); owner picks next lever | [Experiment contract](M001_1_PACING_FUN_EXPERIMENT.md); recorder, window and tests ready; human session files and owner decision pending | REVIEW / TEST | M001.1 |
