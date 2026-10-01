# DC-0002 — Rules specification review

Status: DONE. The human product owner explicitly completed PR #2 / DC-0002 review
and approved G1–G6 exactly as recorded below. Foundation approval was supplied in
the original DC-0002 request. This task does not implement gameplay or Unity code.

## Evidence supplied for review

- [GDD](../GDD.md): accepted mechanics, working terminology, parameterized topology,
  position vocabulary, move generation/resolution, atomic turn transitions,
  21 rule/edge-case rows, 12 invariants and accepted G1–G6 rulings.
- [Rule-to-test matrix](../../tests/M001_RULE_TEST_MATRIX.md): at DC-0002 review,
  25 future cases were NOT RUN. The subsequent DC-0004 evidence records execution;
  no gameplay results were claimed by the rules-only task.
- Supplied decisions cover four players / four pieces, one human / three bots,
  1d6, entry on 6, six bonuses, exact movement, knockout, safety, friendly
  occupancy, private paths, exact finish, no-move resolution, victory and restart.

## Explicit human approval of exposed edge cases

All six former proposals are ACCEPTED FOR M001 by explicit product-owner review:

| Ruling | Approved outcome |
| --- | --- |
| G1 | Opponent-occupied safe squares block entry; destination illegal, no banishment and no cross-player stacking |
| G2 | Six with no legal move resolves NoLegalMoves, then grants another roll to the same active player |
| G3 | Proposed route convention accepted as written: visit shared track once, no second start visit, then owner-only private path; exact terminal non-occupiable completion; no extra lap, branch, early entry or opponent path |
| G4 | Fixed configured cyclic permutation of four players; initialPlayer is explicit input; nonbonus turns advance cyclically |
| G5 | Pass over friendly/opposing pieces; only destination affects occupancy, banishment or legality |
| G6 | Only six grants bonus rolls; no independent knockout/completion bonus, consecutive-six penalty, cap or three-sixes rule; victory overrides any bonus |

Working terminology remains a WORKING DECISION, not immutable CANON.

## Remaining decisions and readiness

All DC-0002 acceptance criteria are met: rules, edge cases, parameterized topology,
state transitions, invariants and test expectations are documented and human-approved.
Main track length, concrete start indices, additional safe-space indices,
final-path length, first-player selection policy, bot strategy and stalemate/draw
policy remain explicit UNKNOWNs or separately configurable choices. They do not
block DC-0002 approval because the rules core remains parameterized. No defaults,
draw outcome or gameplay implementation are introduced; simulation timeouts must
be reported. Artwork and external seed policy remain outside this specification.
DC-0002 is DONE. At rules approval DC-0004 was unstarted; the subsequent explicit
implementation request is tracked in [DC-0004 evidence](DC_0004_EVIDENCE.md).

## Validation

Local `python tools/check_foundation.py`: PASS — 36 required files, 36 Markdown
files, 35 tasks, local links, schema boundaries and M001 exclusions. The desktop's
bundled Python was added to this command's PATH; no dependencies were installed.
`git diff --check`: PASS. CI runs the same foundation checker; see the PR for its
separate execution result. These checks do not validate gameplay behavior.

Manual coverage review maps every supplied decision to the specification:

| Supplied decisions | Specification evidence |
| --- | --- |
| 1–5: players, pieces, control types, initial Abyss, 1d6 | M001 scope, state vocabulary, R01–R02, T01–T02 |
| 6–9: summon on six, bonus, piece choice, testable bots | R02, R04, R06–R07, T02–T03, T05–T09 |
| 10–16: exact movement, knockout, safe/start/configuration, friendly occupancy | Topology, R03, R08–R12, T04, T10–T13 |
| 17–20: owner-only final path, exact completion, overshoot | Route convention, R13–R15, T14–T16; G3 accepted |
| 21–22: empty legal set and player advancement | Turn transitions, R05–R07, R19, T07–T09, T20; G2/G4 accepted |
| 23–25: all-four victory, halt, restart | R16, R20, T17, T21–T22 |

No Unity/gameplay files, actual board dimensions, assets or new dependencies are
introduced. DC-0001 is marked DONE solely because the owner explicitly approved
foundation; DC-0003 stays DONE. DC-0004 status is tracked in the backlog. DC-0002 is DONE based on
documented evidence and explicit human approval, not on unrun gameplay tests.
