# Game implementation boundary

Unity 6 is accepted by [ADR 0002](../docs/adr/0002-unity-6-engine.md). The
[approved M001 rules](../docs/GDD.md) are implemented as plain, deterministic C#.

- [DemonCodex.Unity](DemonCodex.Unity/README.md): playable local-match scene,
  domain/session/presentation assemblies and EditMode/PlayMode tests. Editor 6000.6.3f1 is pinned.
- [DemonCodex.Rules.csproj](DemonCodex.Rules/DemonCodex.Rules.csproj): .NET Standard
  2.1 build of those same rules sources for headless validation; no duplicate logic.
- [Test matrix](../tests/M001_RULE_TEST_MATRIX.md) and
  [implementation evidence](../docs/product/DC_0004_EVIDENCE.md): actual execution
  results including 48 passing Unity EditMode tests.

Board values remain validated configuration, not production defaults. Test-only
RNG and selection policy live outside the domain and are excluded from player builds.
DC-0005 adds a prototype board, session RNG, deterministic simple bots and UI;
see [play instructions and evidence](../docs/product/DC_0005_EVIDENCE.md).
DC-0004 is human-approved DONE; DC-0005 is human-accepted DONE. No backend/economy authority belongs here.
