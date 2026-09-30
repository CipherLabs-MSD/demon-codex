# Game design

PROPOSAL: ROLL → MOVE → ATTACK/KNOCKOUT → REWARD → COLLECT → DISCOVER → FORGE → PROGRESS.
Only the board portion belongs to [M001](product/MILESTONES.md).

WORKING DECISION: first playable targets one board, four players (one human,
three bots), four pieces per player and one die. Include turns, legal move
validation, movement, knockout/send-home, safe/home handling, victory and restart.

## Rules awaiting human review

UNKNOWN: board topology and track length; entry roll; die faces; extra turns;
exact-roll home entry; safe-square occupancy; stacking/blockades; knockout bonus;
overshoot behavior; turn order; bot choice policy; stalemate handling.
TODO: specify these together with examples before coding. Familiar Ludo rules
must not become defaults silently. Abilities are outside the first ruleset.

PROPOSAL: a later collection slice could add four identities, twelve cards,
three initial rarities, a Codex screen, one currency, one chest and one Forge recipe.
It is not required for M001. Accessibility, readable move feedback and touch input
need evaluation in subsequent game-feel work.
