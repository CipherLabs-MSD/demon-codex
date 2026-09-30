# Demon Codex

Dark-fantasy, play-to-collect game and IP built around **666 canonical demons**.
Foundation v0.1 established the project control center. DC-0004 adds a deterministic
C# rules core with executable tests; no playable client or UI exists yet.

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

For the rules core install .NET SDK 8.0.425 and run:

```text
dotnet test tests/DemonCodex.Rules.Tests/DemonCodex.Rules.Tests.csproj -c Release
dotnet run --project tools/DemonCodex.Simulation.Cli -c Release -- 1000 1 artifacts/simulation 10000
```

[Game structure](game/README.md) and [DC-0004 evidence](docs/product/DC_0004_EVIDENCE.md)
explain the shared C# sources, synthetic fixtures and actual test results.
The [Unity scaffold](game/DemonCodex.Unity/README.md) pins Unity 6000.6.3f1.
Unity import/EditMode tests are currently blocked by missing local license entitlement;
headless .NET results do not certify editor or mobile builds.

`backend/`, `assets/` and `blockchain/` contain scope notes only. `content/` remains a
proposed authoring contract. No production backend or deployment exists. DC-0005
and a playable client require separate authorization; see [M001](docs/product/MILESTONES.md).
