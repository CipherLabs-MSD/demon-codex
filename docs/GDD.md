# Game design

PROPOSAL: ROLL → MOVE → ATTACK/KNOCKOUT → REWARD → COLLECT → DISCOVER → FORGE → PROGRESS.
Only the board portion belongs to [M001](product/MILESTONES.md).

## Product clarification: mechanic-family lineage

CANON (human product clarification): the first playable uses an original race /
movement / knockout board-game loop informed by long-established public-domain
cross-and-circle mechanics such as Pachisi, Ludo and Fia med knuff. This is
inspiration at the mechanic-family level.

Demon Codex must develop its own board layout, progression, terminology, visuals,
abilities, economy, content, UX and meta systems. Do not copy proprietary layouts,
assets, wording, progression systems or distinctive presentation from contemporary
commercial games. This direction does not add abilities or meta systems to M001.
Detailed board rules remain UNKNOWN pending DC-0002; no specific rules are implied
by these references.

## M001 scope

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
