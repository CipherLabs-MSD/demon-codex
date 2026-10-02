using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DemonCodex.LocalMatch;
using DemonCodex.Presentation;
using DemonCodex.Rules;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DemonCodex.PlayTests
{
    public class LocalMatchPlayTests
    {
#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator PlaytestVariantsShareRulesAndOnlyBotPacingDiffers()
        {
            yield return SceneManager.LoadSceneAsync("LocalMatch");
            var c = UnityEngine.Object.FindAnyObjectByType<LocalMatchController>();
            Assert.That(c.BotDelaySeconds, Is.EqualTo(DemonCodex.Development.PlaytestExperiment.ControlBotDelaySeconds),
                "Control A is the committed scene pacing.");
            var variants = new[] { DemonCodex.Development.PlaytestVariant.ControlA, DemonCodex.Development.PlaytestVariant.VariantB };
            var events = new List<string>[2];
            var records = new DemonCodex.Development.PlaytestRecord[2];
            for (int v = 0; v < 2; v++)
            {
                // Same launcher as Demon Codex > M001.1 Playtest; real Update-driven bots and real time.
                var recorder = DemonCodex.Editor.PlaytestLauncher.Start(c, variants[v], 82, "test",
                    () => Time.realtimeSinceStartupAsDouble, () => DateTimeOffset.UtcNow);
                Assert.That(c.BotDelaySeconds, Is.EqualTo(DemonCodex.Development.PlaytestExperiment.BotDelaySeconds(variants[v])));
                Assert.That(c.Session.Seed, Is.EqualTo(82u));
                var log = new List<string>();
                events[v] = log;
                c.Session.Transitioned += t => log.Add(Invariants.EventFingerprint(t.Events));
                int humanActions = 0;
                float deadline = Time.realtimeSinceStartup + 60;
                while (humanActions < 6 || !c.Session.IsHumanTurn)
                {
                    Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Watchdog: bots did not hand the turn back.");
                    if (humanActions < 6 && c.Session.IsHumanTurn)
                    {
                        var s = c.Session;
                        Assert.That(s.State.Phase == MatchPhase.AwaitingRoll ? c.Roll() : c.Select(BotPolicy.Choose(s.State, s.LegalMoves)), Is.True);
                        humanActions++;
                    }
                    yield return null;
                }
                recorder.Abort("test complete");
                DemonCodex.Editor.PlaytestLauncher.RestoreControlPacing(c);
                Assert.That(c.BotDelaySeconds, Is.EqualTo(DemonCodex.Development.PlaytestExperiment.ControlBotDelaySeconds));
                records[v] = recorder.Record;
                Assert.That(records[v].humanActions, Is.EqualTo(6));
                Assert.That(records[v].botActions, Is.GreaterThan(3));
            }
            Assert.That(events[1], Is.EqualTo(events[0]), "Same seed and choices: identical domain events in A and B.");
            Assert.That(records[1].botActions, Is.EqualTo(records[0].botActions));
            double perBotA = records[0].botTurnSeconds / records[0].botActions, perBotB = records[1].botTurnSeconds / records[1].botActions;
            TestContext.WriteLine($"Seconds per bot command: A {perBotA:0.000}, B {perBotB:0.000} over {records[0].botActions} commands");
            Assert.That(perBotA, Is.InRange(0.6, 0.85), "Control A waits about 0.65 s per bot command.");
            Assert.That(perBotB, Is.InRange(0.22, 0.45), "Variant B waits about 0.25 s per bot command.");
        }

        [UnityTest]
        public IEnumerator DevelopmentScenariosUseNormalSelectionAndRestartRestoresNormalPlay()
        {
            yield return SceneManager.LoadSceneAsync("LocalMatch");
            var c = UnityEngine.Object.FindFirstObjectByType<LocalMatchController>();
            foreach (DemonCodex.Development.ScenarioKind kind in Enum.GetValues(typeof(DemonCodex.Development.ScenarioKind)))
            {
                var scenario = DemonCodex.Development.ScenarioReplay.Prepare(kind);
                c.LoadDevelopmentSession(scenario.Session, scenario.Instruction);
                Assert.That(c.DevelopmentScenario, Is.EqualTo(scenario.Instruction));
                Assert.That(c.enabled, Is.False, "Only scenario bot scheduling is paused.");
                Assert.That(c.LatestRoll, Is.EqualTo(scenario.IntendedMove.Roll));
                Assert.That(c.DevelopmentScenarioAwaitingExit, Is.False);
                Assert.That(c.Select(scenario.IntendedMove), Is.True);
                Assert.That(c.PresentedState, Is.SameAs(c.Session.State));
                Assert.That(c.DevelopmentScenarioAwaitingExit, Is.True, "View must offer PLAY AGAIN while bots are paused.");
                bool captured = false, completed = false, path = false;
                CheckProjection(c, ref captured, ref completed, ref path);
                var outcome = c.PresentedState;
                yield return new WaitForSecondsRealtime(.7f);
                Assert.That(c.PresentedState, Is.SameAs(outcome), "Outcome stays visible for inspection.");
                yield return Capture("scenario-" + kind.ToString().ToLowerInvariant());
                Assert.That(c.PlayAgain(2026), Is.True);
                Assert.That(c.DevelopmentScenario, Is.Null);
                Assert.That(c.DevelopmentScenarioAwaitingExit, Is.False);
                Assert.That(c.enabled, Is.True);
                Assert.That(c.LatestRoll, Is.Null);
                Assert.That(c.Session.State.Pieces.All(p => p.Position.Kind == PieceKind.InAbyss), Is.True);
                Assert.That(c.Session.State.Revision, Is.EqualTo(outcome.Revision + 1));
                Assert.That(c.Roll(), Is.True);
                // An ordinary seeded match is active again; no replay/dice override remains.
                var normal = new LocalMatchSession(2026); normal.Start(); normal.RollHuman();
                Assert.That(c.Session.State.ActivePlayer, Is.EqualTo(normal.State.ActivePlayer));
                Assert.That(c.Session.State.PendingRoll, Is.EqualTo(normal.State.PendingRoll));
                Assert.That(c.Session.State.Phase, Is.EqualTo(normal.State.Phase));
            }
        }
#endif
        private sealed class OneDie : IDieSource { public int Next() => 1; }
        [UnityTest]
        public IEnumerator BotDelayPacesCommandsAndRestartCancelsPendingTurn()
        {
            yield return SceneManager.LoadSceneAsync("LocalMatch");
            var c = UnityEngine.Object.FindFirstObjectByType<LocalMatchController>();
            c.BotDelaySeconds = .2f;
            c.Initialize(1, new OneDie()); c.Session.Start();
            Assert.That(c.Roll(), Is.True);
            long revision = c.Session.State.Revision;
            yield return null;
            Assert.That(c.Session.State.Revision, Is.EqualTo(revision));
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(c.Session.State.Revision, Is.GreaterThan(revision));
            Assert.That(c.PlayAgain(2026), Is.True);
            var restarted = c.Session.State;
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(c.Session.State, Is.SameAs(restarted));
            Assert.That(c.Session.IsHumanTurn, Is.True);
        }

        [UnityTest]
        public IEnumerator ScenePlaysTwoMatchesAndPresentationFollowsDomainEvents()
        {
            yield return SceneManager.LoadSceneAsync("LocalMatch");
            var controller = UnityEngine.Object.FindFirstObjectByType<LocalMatchController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetComponent<LocalMatchView>().enabled, Is.True);
            Invariants.AssertState(controller.Session.State);
            Assert.That(controller.Session.Started, Is.False);
            yield return Capture("01-initial");
            controller.BotDelaySeconds = 0;
            controller.StartMatch(2026);
            bool selection = false, active = false, captured = false, completed = false, path = false;
            for (int match = 0; match < 2; match++)
            {
                int steps = 0;
                while (controller.Session.State.Phase != MatchPhase.Finished && steps++ < 20000)
                {
                    var session = controller.Session;
                    if (session.IsHumanTurn)
                    {
                        if (session.State.Phase == MatchPhase.AwaitingRoll) Assert.That(controller.Roll(), Is.True);
                        else {
                            Assert.That(session.SelectableMoves, Is.EqualTo(RulesEngine.LegalMoves(session.State, session.State.ActivePlayer, session.State.PendingRoll.Value)));
                            if (!selection) { selection = true; yield return Capture("02-selection"); }
                            Assert.That(controller.Select(BotPolicy.Choose(session.State, session.LegalMoves)), Is.True);
                        }
                    }
                    CheckProjection(controller, ref captured, ref completed, ref path);
                    // Actual MonoBehaviour.Update drives bot commands, not a simulated callback.
                    yield return null;
                    CheckProjection(controller, ref captured, ref completed, ref path);
                    if (!active && captured && path) { active = true; yield return Capture("03-active"); }
                }
                Assert.That(controller.Session.State.Phase, Is.EqualTo(MatchPhase.Finished));
                Assert.That(controller.Roll(), Is.False);
                Assert.That(controller.Session.StepBot(), Is.False);
                if (match == 0) yield return Capture("04-victory");
                long revision = controller.Session.State.Revision;
                Assert.That(controller.PlayAgain(2026), Is.True);
                Assert.That(controller.PresentedState.Revision, Is.EqualTo(revision + 1));
                Assert.That(controller.PresentedState.Pieces.All(p => p.Position.Kind == PieceKind.InAbyss), Is.True);
                Assert.That(controller.LatestRoll, Is.Null);
                Assert.That(controller.Log.Count, Is.EqualTo(1));
                Assert.That(controller.PresentedState.Winner, Is.Null);
            }
            Assert.That(captured && completed && path && selection, Is.True);
        }
        private static void CheckProjection(LocalMatchController c, ref bool captured, ref bool completed, ref bool path)
        {
            Assert.That(c.PresentedState, Is.SameAs(c.Session.State));
            foreach (var e in c.LastEvents)
            {
                if (e.Kind == EventKind.PieceBanished) {
                    captured = true;
                    var piece = c.PresentedState.Piece(e.Piece.Value);
                    Assert.That(BoardLayout.Piece(c.PresentedState, piece), Is.EqualTo(BoardLayout.Slot(BoardLayout.Abyss(piece.Owner), piece.Id.Index)));
                }
                if (e.Kind == EventKind.PieceCompleted) {
                    completed = true;
                    var piece = c.PresentedState.Piece(e.Piece.Value);
                    Assert.That(BoardLayout.Piece(c.PresentedState, piece), Is.EqualTo(BoardLayout.Slot(BoardLayout.Home(piece.Owner), piece.Id.Index)));
                }
            }
            path |= c.PresentedState.Pieces.Any(p => p.Position.Kind == PieceKind.OnAscensionPath);
            Invariants.AssertState(c.PresentedState);
        }
        private static IEnumerator Capture(string name)
        {
            string directory = Environment.GetEnvironmentVariable("DEMON_CODEX_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) yield break;
            Directory.CreateDirectory(directory);
            string file = Path.Combine(directory, name + ".png");
            ScreenCapture.CaptureScreenshot(file);
            float deadline = Time.realtimeSinceStartup + 10;
            while (!File.Exists(file) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(File.Exists(file), Is.True, "Screenshot requested but not produced: " + file);
        }
    }
}
