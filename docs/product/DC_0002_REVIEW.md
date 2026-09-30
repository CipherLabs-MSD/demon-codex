# DC-0002 — Rules specification review

Status: REVIEW / TEST. Foundation approval was supplied explicitly by the human
product owner in the DC-0002 request. This task does not implement gameplay or Unity code.

## Evidence supplied for review

- [GDD](../GDD.md): accepted mechanics, working terminology, parameterized topology,
  position vocabulary, move generation/resolution, atomic turn transitions,
  21 rule/edge-case rows, 12 invariants and explicit review gaps.
- [Rule-to-test matrix](../../tests/M001_RULE_TEST_MATRIX.md): 25 future cases,
  each marked NOT RUN; no simulated or gameplay test results are claimed.
- Supplied decisions cover four players / four pieces, one human / three bots,
  1d6, entry on 6, six bonuses, exact movement, knockout, safety, friendly
  occupancy, private paths, exact finish, no-move resolution, victory and restart.

## Edge cases exposed

Opponent occupancy on safe squares does not follow from immunity to capture.
A six with no legal move needs an explicit bonus ruling. The phrase "circuit"
needs an off-by-one route convention. Completion must distinguish a terminal
state from an occupied square. Intermediate occupancy, extra reward rolls,
repeated sixes and turn order also need explicit treatment. G1–G6 in the GDD
offer reviewable resolutions; they are not silently inferred from other games.
Winning on six must stop play before any bonus. Blocked summon is not permission
to stack, capture a safe occupant or move another player's piece.

## Remaining decisions and readiness

Ready for human rules review, not implementation approval. The owner must resolve
G1–G6. Concrete board dimensions/indices and initial-player policy remain UNKNOWN;
they can be supplied as reviewed configuration without hard-coding the rules core.
Bot strategy, artwork and external seed policy are outside this specification.
No stalemate or draw outcome is invented; simulation timeouts must be reported.
DC-0002 remains REVIEW / TEST until review; DC-0004 remains BACKLOG and is not started.

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
| 17–20: owner-only final path, exact completion, overshoot | Route convention, R13–R15, T14–T16; G3 needs review |
| 21–22: empty legal set and player advancement | Turn transitions, R05–R07, R19, T07–T09, T20; G2/G4 need review |
| 23–25: all-four victory, halt, restart | R16, R20, T17, T21–T22 |

No Unity/gameplay files, actual board dimensions, assets or new dependencies are
introduced. DC-0001 is marked DONE solely because the owner explicitly approved
foundation; DC-0003 stays DONE and DC-0004 stays BACKLOG. DC-0002 is not marked DONE.
