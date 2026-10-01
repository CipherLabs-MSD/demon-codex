# Unity 6 local match prototype

Editor: **6000.6.3f1 (45d8eee7de74)**. Open this folder in Unity, open
`Assets/DemonCodex/Scenes/LocalMatch.unity`, enter Play Mode, maximize the Game view
and press START MATCH. Landscape 1200 × 850 is the reference layout. P0 is human:
ROLL, then click a highlighted piece or its button. Three bots act automatically.
The winner gets PLAY AGAIN, using the domain Restart command.

See [DC-0005 evidence and owner acceptance](../../docs/product/DC_0005_EVIDENCE.md)
for configuration, controls, bot policy and actual validation. Simple IMGUI
primitives are placeholders; mobile/device acceptance remains pending.

## Structure and mobile boundary

- `Assets/DemonCodex/Rules`: unchanged authoritative plain C# rules.
- `Assets/DemonCodex/LocalMatch`: Unity-independent session, RNG and bot policy.
- `Assets/DemonCodex/Presentation`: timing, event feedback, layout and IMGUI;
  no duplicate move rules.
- `Assets/DemonCodex/Scenes/LocalMatch.unity`: playable scene, enabled in build list.
- `Assets/DemonCodex/Editor`: optional scene regeneration menu, the
  `Demon Codex > Development Playtest Scenarios` window and the
  `Demon Codex > M001.1 Playtest` A/B window (DC-0037); not needed to play.
- `Assets/DemonCodex/Development`: Editor-only scenario replay for fast owner
  endgame checks. It replays ordinary seeded sessions through normal commands and
  is absent from player builds; START MATCH never uses it. It also holds the M001.1
  playtest recorder, which only observes session events and writes JSON to
  `tests/evidence/dc-0037/sessions`.
- `Assets/DemonCodex/Tests`: rules/simulation, session and PlayMode tests, excluded
  from normal player builds.
- `ProjectSettings`: editor version, build list and player settings tracked.
  Other local Unity-generated settings are preserved, not part of this task.
- `Packages`: Test Framework 1.8.0 and built-in ScreenCapture 1.0.0, with locked
  NUnit, IMGUI, JSON and image conversion dependencies. No network/gameplay package.

Unity must not own future authoritative economy/inventory state. Core and session
use .NET Standard 2.1, C# 9-compatible shared sources. iOS/Android remain the
primary direction; installed modules support Windows only. No mobile build,
signing, IL2CPP or touch validation is claimed.

## Reproduce validation

Close the project in the Editor first. From repository root, use
`tools/run_unity_tests.ps1 -UnityEditor '<absolute path to Unity.exe>'` for EditMode.
Add `-TestPlatform PlayMode` for PlayMode, and optionally `-CaptureScreenshots`.
Each run uses fresh paths and checks exit code and NUnit XML. Screenshots require
normal Game view rendering: that option launches without batch mode, with a hidden
window requested. Ordinary runs use batch/no-graphics. The helper does not acquire
licenses or install an editor. CI runs shared .NET tests and simulation; Unity
execution is recorded as local evidence.
