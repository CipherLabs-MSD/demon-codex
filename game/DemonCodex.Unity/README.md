# Unity 6 rules-core scaffold

Editor: **6000.6.3f1 (45d8eee7de74)**, matching the locally installed editor.
This is a minimal source-controlled scaffold, not a playable project or visual board.
There are no scenes in the build list. Do not add UI or artwork as part of DC-0004.

## Structure and mobile boundary

- `Assets/DemonCodex/Rules`: plain C# domain assembly; `noEngineReferences: true`.
- `Assets/DemonCodex/Tests/EditMode`: shared NUnit tests, editor-only.
- `Assets/DemonCodex/Tests/Simulation`: editor-only synthetic harness; owns seeded
  RNG and test move selection. Not included in a mobile player build.
- `ProjectSettings`: exact editor version and empty build list. Other settings
  were generated locally during licensed import; these untracked settings are not
  part of the evidence-only update. Rendering is undecided.
- `Packages`: only Test Framework is directly requested; no render, network or
  gameplay packages. Stable `.meta` GUIDs are committed beside assets/folders.

iOS/Android remain the primary client direction. The domain targets .NET Standard
2.1 with C# 9-compatible source, shared by Unity and headless .NET builds. No scene
object, platform API or rendering dependency enters the rules assembly. Mobile
build modules, signing, application identifiers and device settings are not chosen
or verified here. The inspected editor installation has Windows player support only.
Future economy/inventory authority must remain outside the client.

## Required packages

Unity 6000.6.3f1 ships these built-in packages, pinned from their installed manifests:

| Package | Version | Role |
| --- | --- | --- |
| com.unity.test-framework | 1.8.0 | EditMode test runner; direct dependency |
| com.unity.ext.nunit | 2.1.0 | Runner dependency, Unity's NUnit 3.5-based fork |
| com.unity.modules.imgui | 1.0.0 | Runner dependency |
| com.unity.modules.jsonserialize | 1.0.0 | Runner dependency |

The lock file reflects the bundled package dependency graph. A successful local
Unity 6000.6.3f1 import/compilation and EditMode run on 2026-10-01 now verifies
**48 tests passed, 0 failed, 0 skipped**. This supersedes the earlier license-blocked
attempt. Mobile and IL2CPP builds remain unverified.

## Reproduce Unity validation

Activate an appropriate Unity license through the user's normal Unity sign-in
flow, then run from the repository root in PowerShell:

```powershell
./tools/run_unity_tests.ps1 -UnityEditor 'C:\Program Files\Unity 6000.6.3f1\Editor\Unity.exe'
```

The helper runs hidden batch mode, waits for exit and checks a fresh NUnit result
file. It does not acquire a license or install an editor. The successful run is
recorded with all 48 outcomes and the raw XML hash. See [implementation evidence](../../docs/product/DC_0004_EVIDENCE.md).

References: [editor release](https://unity.com/releases/editor/whats-new/6000.6.3f1)
and [Unity test command-line reference](https://docs.unity.com/en-us/engine/6000.6/manual/scripting/test-framework-introduction/reference-command-line).
