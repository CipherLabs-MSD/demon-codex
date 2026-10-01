# DC-0004 — Deterministic rules core evidence

Status: DONE — explicitly approved by the human product owner on 2026-10-01
after successful Unity EditMode validation. At that approval DC-0005 was BACKLOG;
its subsequent work is tracked in [DC-0005 evidence](DC_0005_EVIDENCE.md).
Scope: first domain implementation, not DC-0005's playable UI or final bot AI.
Based on approved main `614ba9bbbdf6428d8da80e284aab4ebe4ea480b3` and [GDD](../GDD.md).

## Architecture implemented

The single source of domain logic is
[Rules](../../game/DemonCodex.Unity/Assets/DemonCodex/Rules/RulesEngine.cs), with
[model/configuration](../../game/DemonCodex.Unity/Assets/DemonCodex/Rules/Model.cs),
[commands/events](../../game/DemonCodex.Unity/Assets/DemonCodex/Rules/Commands.cs)
and [invariants](../../game/DemonCodex.Unity/Assets/DemonCodex/Rules/Invariants.cs).
The Unity assembly has no engine references. A .NET Standard 2.1 project compiles
the exact same files, without copying a second implementation. C# language level: 9.

Immutable snapshots and defensively copied configuration separate mutation from
input. LegalMoves is a pure complete-set query in stable piece-index order. Apply
validates phase/player/roll/revision and recomputes submitted move legality before
producing an atomic new snapshot and ordered events. Rejections return the same
snapshot and zero domain events. Bad legal-move queries throw diagnostic argument/
phase exceptions rather than returning a misleading empty legal set.
Generate selectable move tokens from the **post-roll** AwaitingSelection snapshot:
ProvideDieRoll advances the revision, so pre-roll preview tokens become stale.

Piece IDs encode immutable owner plus index 0–3. Path coordinates are owner-qualified
by construction. Long intermediate route arithmetic avoids integer overflow for
extreme valid configurations. Revisions advance once per accepted command, including
restart; long.MaxValue produces a diagnostic rejection rather than wrapping.
Revision tokens are scoped to a caller-owned match; this is not network replay
protection or a persistent save-game protocol. Public match creation starts from
validated configuration only; arbitrary snapshot loading is intentionally absent.

State and transition invariant checks cover I01–I12, including deterministic
reapplication, event order, safe banishment, movement distances, occupancy, ownership,
victory priority and restart. InternalsVisibleTo permits invariant-checked test
fixtures, not public state mutation. The caller can run diagnostic checks without
the domain having logging, time, RNG or service dependencies.

## Tests actually executed

NUnit 3.14.0, NUnit3TestAdapter 4.6.0, Microsoft.NET.Test.Sdk 17.11.1 on .NET SDK
8.0.425, Windows x64. **48 tests passed, 0 failed, 0 skipped.** Includes parameterized
T01–T25 cases, corrupt-state diagnostics, overflow boundaries and deliberate
watchdog classification. Every command in the tests is checked for invariants and
input immutability. T24 compares full replay traces and event ordering; T25 runs
twelve seeded integration matches in addition to the separate 1,000-match batch.
All test sources also belong to the Unity EditMode test assembly.

[Sanitized test results](../../tests/evidence/dc-0004/unit-tests.json) list actual
test names/outcomes. [Test source](../../game/DemonCodex.Unity/Assets/DemonCodex/Tests/EditMode/RulesTests.cs)
uses names prefixed T01–T25, matching the [matrix](../../tests/M001_RULE_TEST_MATRIX.md).
The raw local TRX is ignored under `artifacts/dotnet-tests/`; CI uploads its raw results.

## Simulation actually executed

The [separate harness](../../game/DemonCodex.Unity/Assets/DemonCodex/Tests/Simulation/Simulation.cs)
owns xorshift32 with rejection-sampled 1d6 and a deterministic completion-first /
furthest-progress / piece-index tie-break policy. It is a test policy, not final bot
AI. Seeds select explicitly synthetic boards with track lengths 12/20/28, per-owner
path lengths 1–6, differing initial players, fixed cyclic order and extra safety.
These values are fixtures, not product canon or balance choices.

| Measurement | Actual result |
| --- | --- |
| Seeds | 1–1000 inclusive |
| Matches completed | 1,000 / 1,000 |
| Rolls | 185,097 |
| Resolved player turns (includes bonus sequence) | 154,204 |
| Accepted transitions checked for I01–I12 | 291,977 |
| Invariant/harness failures | 0 |
| Watchdog noncompletions | 0 |
| Watchdog limit | 10,000 rolls per match |

[Summary](../../tests/evidence/dc-0004/summary.json) and
[per-seed CSV](../../tests/evidence/dc-0004/matches.csv) retain seed, rolls, turns,
transitions, winner, failures, watchdog classification and full-trace SHA-256.
No failure seeds exist in this run. On failure or timeout the harness writes the
complete initial state, action/event/state trace and diagnostics to `seed-N.trace.txt`.
Reproduce one seed with count=1 and first-seed=N. A watchdog is a harness failure/
noncompletion, never a draw or gameplay win. A separate unit test deliberately
forces a one-roll watchdog and verifies its classification; it is not one of the
1,000 production-sized batch results. These samples do not prove all seeds terminate.

## Successful Unity EditMode validation

Pinned installed editor: **6000.6.3f1 (45d8eee7de74)**. Minimal scene-free scaffold
under [DemonCodex.Unity](../../game/DemonCodex.Unity/README.md), with bundled Test
Framework 1.8.0; its package graph is documented there. No extra render/game packages.

The owner supplied a successful headless EditMode run on **2026-10-01 at 08:43:32 UTC**.
The result XML was inspected: **48 tests passed, 0 failed, 0 skipped**. The accompanying
editor log confirms version 6000.6.3f1 (45d8eee7de74) and successful exit code 0.
Editor import/compilation and execution of the shared test assembly are now verified
locally. This supersedes the earlier exit-198 license blocker.

Raw result: `artifacts/unity/e668cc08def3423e852dd4dab9cfe5e8/results.xml`.
[Sanitized Unity evidence](../../tests/evidence/dc-0004/unity-attempt.json) records
all 48 test outcomes, source path/hash, timestamps and PASSED status. Raw artifacts
remain local/ignored. Generated local project settings are preserved but not included
in this documentation-only update. No IL2CPP, iOS or Android build is claimed;
mobile build modules/settings remain future work.

## Reproduce validation

From repository root, with .NET SDK 8.0.425 and Python 3.11+ on PATH:

```text
dotnet test tests/DemonCodex.Rules.Tests/DemonCodex.Rules.Tests.csproj -c Release --logger "trx;LogFileName=rules.trx" --results-directory artifacts/dotnet-tests
dotnet run --project tools/DemonCodex.Simulation.Cli -c Release -- 1000 1 artifacts/simulation 10000
python tools/check_foundation.py
git diff --check
```

PowerShell, after Unity license activation:

```powershell
./tools/run_unity_tests.ps1 -UnityEditor 'C:\Program Files\Unity 6000.6.3f1\Editor\Unity.exe'
```

Local .NET was installed in a task-specific temporary directory; DOTNET_CLI_HOME
was also task-local. No global SDK or PATH change is required. GitHub Actions
[rules workflow](../../.github/workflows/rules.yml) runs the domain suite and 1,000
matches, compares the deterministic per-seed baseline, and uploads raw artifacts.
The [foundation workflow](../../.github/workflows/foundation.yml) runs documentation/
link checks. CI does not pretend to run licensed Unity tests. Consult the PR for
the actual workflow result on its current commit.

Local foundation/link validation passed (36 required files, 38 Markdown documents,
35 tasks); whitespace validation passed. Generated caches are excluded from the
documentation scan. Unity validation is evidenced separately by the successful run above.

## Open issues and review boundary

Unity EditMode validation is complete; mobile and IL2CPP builds remain unverified.
Board dimensions, starts, additional safe indices, path lengths, first-player
policy, final bot strategy and draw/stalemate policy remain unresolved outside
the parameterized core. No visual board, UI, abilities, backend or economy exists.
The human owner explicitly approved DC-0004 as DONE based on implementation,
headless tests/simulation and successful Unity validation. At that approval DC-0005
was BACKLOG and unstarted; that approval did not authorize its implementation.
