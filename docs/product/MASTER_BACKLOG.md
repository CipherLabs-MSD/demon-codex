# Master backlog

**This backlog represents what we can see now and will evolve with evidence.**

WORKING DECISION: stable DC-0001 style IDs are never reused or renumbered. New tasks
use the next unused number. Epics are the workstream names below. Later rows are
PROPOSAL discovery work, not commitments or authorization to implement systems.
Owner is an accountable role pending human assignment; no agent is dispatched.
P0 precedes P1, P2 and P3. M001 rows support O001 / KR1–KR5; later work is unassigned
to an OKR until reviewed. F001 is the foundation prerequisite.

States: BACKLOG, READY, IN PROGRESS, REVIEW / TEST, BLOCKED, DONE.
READY requires clear acceptance and satisfied dependencies; DONE requires linked
actual evidence and review. The Evidence column specifies expected artifacts,
not results. A blocked task records reason, owner and unblock action in its issue.
DC-0002/0003 become executable only after DC-0001 review; their state remains BACKLOG
until that gate is satisfied. Foundation outputs do not imply human approval.

| ID | Epic | Task | Owner/agent | Priority | Dependencies | Acceptance criteria | Evidence required | Status | Milestone |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| DC-0001 | Foundation | Review foundation controls | Product owner | P0 | None | README navigation, boundaries and validation reviewed | Foundation review and check output | REVIEW / TEST | F001 |
| DC-0002 | Core Game | Specify board and turn rules | Core Game lead (unassigned) | P0 | DC-0001 | Resolve every GDD rule unknown with examples and owner approval | Reviewed rules and edge-case table | BACKLOG | M001 |
| DC-0003 | Core Game | Select prototype engine and runtime | Core Game lead (unassigned) | P0 | DC-0001 | Compare lightweight options against local play, testing and mobile direction | Accepted ADR; no implementation | BACKLOG | M001 |
| DC-0004 | Core Game | Implement deterministic rules core | Core Game lead (unassigned) | P0 | DC-0002, DC-0003 | Legal moves, turns, knockout, home and win follow approved rules | Rule tests and reproducible transition traces | BACKLOG | M001 |
| DC-0005 | Core Game | Build local match UI and bots | Core Game lead (unassigned) | P0 | DC-0004 | One human and three bots can start, finish and restart a match | Manual complete-match record | BACKLOG | M001 |
| DC-0006 | Release | Validate M001 and capture playtest | Release lead (unassigned) | P0 | DC-0005 | All O001 KRs evidenced including simulations and clean setup | Test outputs, seed suite, playtest and owner decision | BACKLOG | M001 |
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
