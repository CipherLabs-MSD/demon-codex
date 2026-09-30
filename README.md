# Demon Codex

Dark-fantasy, play-to-collect game and IP built around **666 canonical demons**.
Foundation v0.1 establishes the project control center. No game is implemented yet.

## Start here

- [Vision and decision labels](docs/VISION.md)
- [Game design and M001 rules specification](docs/GDD.md)
- [Architecture](docs/ARCHITECTURE.md) and [decision records](docs/adr/README.md)
- [M001 scope](docs/product/MILESTONES.md), [OKRs](docs/product/OKRS.md), and [master backlog](docs/product/MASTER_BACKLOG.md)
- [Agent instructions](AGENTS.md) and [contribution workflow](CONTRIBUTING.md)

## Reference library

[Economy](docs/ECONOMY.md) · [Commercial](docs/COMMERCIAL.md) ·
[Growth](docs/GROWTH.md) · [LiveOps](docs/LIVEOPS.md) ·
[Security](docs/SECURITY.md) · [Roadmap](docs/ROADMAP.md) ·
[Lore](docs/lore/LORE_BIBLE.md) · [Codex](docs/lore/CODEX.md) ·
[Forgemaster](docs/lore/FORGEMASTER.md) · [Mythics](docs/lore/MYTHICS.md) ·
[Art](docs/art/ART_BIBLE.md) · [Growth backlog](docs/product/GROWTH_MASTER_BACKLOG.md)

## Local setup

Clone the repository and install Python 3.11 or newer for foundation checks only.
Run `python tools/check_foundation.py` from the repository root.
The check needs no third-party packages. CI runs the same command.
There is no game build, engine dependency, backend, or deployment procedure yet.

`game/`, `backend/`, `assets/`, and `blockchain/` contain scope notes only.
`content/` contains a proposed data contract, not approved demon records.
`tests/` describes the future evidence strategy. Begin implementation only in a
separately authorized task after the M001 rules are reviewed. Unity 6 is the accepted engine for M001 and
the primary mobile client; see [ADR 0002](docs/adr/0002-unity-6-engine.md).
