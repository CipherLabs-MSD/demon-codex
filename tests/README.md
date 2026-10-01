# Executable rules and documentation evidence

From repository root with .NET SDK 8.0.425:

```text
dotnet test tests/DemonCodex.Rules.Tests/DemonCodex.Rules.Tests.csproj -c Release --logger "trx;LogFileName=rules.trx" --results-directory artifacts/dotnet-tests
dotnet run --project tools/DemonCodex.Simulation.Cli -c Release -- 1000 1 artifacts/simulation 10000
python tools/check_foundation.py
```

The [M001 matrix](M001_RULE_TEST_MATRIX.md) maps T01–T25 to shared NUnit source in
the Unity project. The same files compile in the headless .NET test project.
[Evidence](../docs/product/DC_0004_EVIDENCE.md) records 48 passing .NET cases and
1,000 complete matches with invariant checks; raw CI artifacts are uploaded on
every run. [Committed per-seed results](evidence/dc-0004/matches.csv) support exact
cross-runtime replay comparison. All board numbers are synthetic test fixtures.

State and transition checks enforce I01–I12 after every harness transition, including
reapplying identical input to compare ordered events and state. Failed actions are
checked for unchanged snapshots. On simulation failure/watchdog, full traces are
saved alongside the per-seed CSV. A watchdog does not create a gameplay draw.

Unity 6000.6.3f1 EditMode validation is complete: **48 passed, 0 failed, 0 skipped**
on 2026-10-01; [verified results](evidence/dc-0004/unity-attempt.json) supersede the
earlier license blocker. Use [the Unity helper](../tools/run_unity_tests.ps1) to
reproduce. No mobile or IL2CPP build pass is claimed.

Foundation checks cover documentation links and required files, task references,
schema boundaries and milestone exclusions. Generated Unity/.NET directories are
excluded from documentation traversal. They do not replace narrative review.
DC-0005 adds a playable scene, nine shared session tests, five Editor-only
development-scenario replay tests and three Unity PlayMode tests. Run `dotnet test tests/DemonCodex.LocalMatch.Tests/DemonCodex.LocalMatch.Tests.csproj -c Release`.
Add `-TestPlatform PlayMode` to the Unity helper for scene/controller validation.
See [DC-0005 execution evidence and owner acceptance](../docs/product/DC_0005_EVIDENCE.md).
DC-0004 and DC-0005 are DONE.
DC-0006 adds a read-only pacing analysis of the prototype configuration (shipped
session and bots, invariants on every transition). Run
`dotnet run --project tools/DemonCodex.Pacing.Cli -c Release -- 1000 1 artifacts/pacing 10000`;
[committed results](evidence/dc-0006/pacing-summary.json) feed the [M001 review](../docs/product/M001_REVIEW.md).
