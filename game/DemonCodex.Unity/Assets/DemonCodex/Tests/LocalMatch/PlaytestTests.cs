using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DemonCodex.Development;
using DemonCodex.LocalMatch;
using DemonCodex.Rules;
using NUnit.Framework;

namespace DemonCodex.LocalMatchTests
{
    public class PlaytestTests
    {
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

        // Drives a session the way the Unity controller does, with a fake clock: a bot command
        // waits the variant's delay and a human command takes a fixed think time.
        private sealed class Harness
        {
            public const double HumanThink = 2.0;
            public double Now, HumanSeconds, BotSeconds;
            public int HumanActions, BotActions, Choices, Forced;
            public readonly List<double> ChoiceTimes = new List<double>();
            public readonly List<(bool human, Transition transition)> Log = new List<(bool, Transition)>();
            public readonly LocalMatchSession Session;
            public readonly PlaytestRecorder Recorder;
            private readonly float botDelay;
            private bool humanActing;

            public Harness(PlaytestVariant variant, uint seed)
            {
                botDelay = PlaytestExperiment.BotDelaySeconds(variant);
                Session = new LocalMatchSession(seed);
                Session.Start();
                Recorder = new PlaytestRecorder(Session, variant, "test", () => Now, () => Epoch.AddSeconds(Now));
                Session.Transitioned += t => Log.Add((humanActing, t));
            }

            public bool Step()
            {
                if (Session.State.Phase == MatchPhase.Finished) return false;
                humanActing = Session.IsHumanTurn;
                if (humanActing)
                {
                    Now += HumanThink; HumanSeconds += HumanThink; HumanActions++;
                    if (Session.State.Phase == MatchPhase.AwaitingRoll) Assert.That(Session.RollHuman(), Is.True);
                    else
                    {
                        if (Session.LegalMoves.Count > 1) { Choices++; ChoiceTimes.Add(Now); } else Forced++;
                        Assert.That(Session.SelectHuman(BotPolicy.Choose(Session.State, Session.LegalMoves)), Is.True);
                    }
                }
                else { Now += botDelay; BotSeconds += botDelay; BotActions++; Assert.That(Session.StepBot(), Is.True); }
                return true;
            }

            public Harness RunToEnd()
            {
                for (int i = 0; i < 20000 && Step(); i++) { }
                Assert.That(Session.State.Phase, Is.EqualTo(MatchPhase.Finished));
                return this;
            }
            public IEnumerable<DomainEvent> Events => Log.SelectMany(l => l.transition.Events);
        }

        [Test]
        public void VariantsDifferOnlyInBotPresentationDelay()
        {
            Assert.That(Enum.GetValues(typeof(PlaytestVariant)).Length, Is.EqualTo(2));
            Assert.That(PlaytestExperiment.BotDelaySeconds(PlaytestVariant.ControlA), Is.EqualTo(PlaytestExperiment.ControlBotDelaySeconds));
            Assert.That(PlaytestExperiment.ControlBotDelaySeconds, Is.EqualTo(0.65f), "Control A is the M001 pacing.");
            Assert.That(PlaytestExperiment.BotDelaySeconds(PlaytestVariant.VariantB), Is.EqualTo(0.25f));
            foreach (uint seed in PlaytestExperiment.SuggestedSeeds)
            {
                var a = new Harness(PlaytestVariant.ControlA, seed).RunToEnd();
                var b = new Harness(PlaytestVariant.VariantB, seed).RunToEnd();
                // Same seed and same choices: identical domain events, final state and gameplay counts.
                Assert.That(b.Log.Select(l => Invariants.EventFingerprint(l.transition.Events)),
                    Is.EqualTo(a.Log.Select(l => Invariants.EventFingerprint(l.transition.Events))));
                Assert.That(Invariants.Fingerprint(b.Session.State), Is.EqualTo(Invariants.Fingerprint(a.Session.State)));
                var ra = a.Recorder.Record; var rb = b.Recorder.Record;
                Assert.That(Gameplay(rb), Is.EqualTo(Gameplay(ra)));
                // Only presentation timing differs.
                Assert.That(ra.botDelaySeconds, Is.Not.EqualTo(rb.botDelaySeconds));
                Assert.That(ra.humanTurnSeconds, Is.EqualTo(rb.humanTurnSeconds).Within(1e-6));
                Assert.That(ra.botTurnSeconds / rb.botTurnSeconds, Is.EqualTo(0.65 / 0.25).Within(1e-3));
            }
        }

        private static string Gameplay(PlaytestRecord r) => string.Join(",", r.seed, r.winner, r.naturalVictory, r.rolls,
            r.humanRolls, r.turns, r.humanActions, r.botActions, r.meaningfulChoices, r.forcedChoices, r.humanNoMoveRolls,
            r.sixes, r.knockouts, r.humanKnockoutsSuffered, r.summons, r.ascensionEntries, r.completedPieces, r.humanCompletedPieces);

        [Test]
        public void SameSeedDealsIdenticalDiceWhateverTheHumanChooses()
        {
            // Matched A/B pairs rely on this: the die stream and its seat order do not depend on choices.
            var policy = new LocalMatchSession(82); var contrary = new LocalMatchSession(82);
            var policyRolls = new List<string>(); var contraryRolls = new List<string>();
            policy.Transitioned += t => policyRolls.AddRange(t.Events.Where(e => e.Kind == EventKind.DieRolled).Select(e => e.Player + ":" + e.Roll));
            contrary.Transitioned += t => contraryRolls.AddRange(t.Events.Where(e => e.Kind == EventKind.DieRolled).Select(e => e.Player + ":" + e.Roll));
            Play(policy, legal => BotPolicy.Choose(policy.State, legal));
            Play(contrary, legal => legal[legal.Count - 1]);
            Assert.That(Invariants.Fingerprint(contrary.State), Is.Not.EqualTo(Invariants.Fingerprint(policy.State)), "Choices did diverge.");
            int shared = Math.Min(policyRolls.Count, contraryRolls.Count);
            Assert.That(contraryRolls.Take(shared), Is.EqualTo(policyRolls.Take(shared)));
        }

        private static void Play(LocalMatchSession s, Func<IReadOnlyList<LegalMove>, LegalMove> human)
        {
            s.Start();
            for (int i = 0; i < 20000 && s.State.Phase != MatchPhase.Finished; i++)
                Assert.That(!s.IsHumanTurn ? s.StepBot() : s.State.Phase == MatchPhase.AwaitingRoll ? s.RollHuman() : s.SelectHuman(human(s.LegalMoves)), Is.True);
        }

        [Test]
        public void CompletedMatchMatchesIndependentTallyAndDc0006Evidence()
        {
            var h = new Harness(PlaytestVariant.ControlA, 1).RunToEnd();
            var r = h.Recorder.Record;
            var events = h.Events.ToList();
            Assert.That(h.Recorder.IsRecording, Is.False);
            Assert.That(r.naturalVictory, Is.True); Assert.That(r.aborted, Is.False);
            Assert.That(r.abortSeconds, Is.EqualTo(-1)); Assert.That(r.abortReason, Is.Null);
            Assert.That(r.winner, Is.EqualTo(h.Session.State.Winner.ToString()));
            Assert.That(r.rolls, Is.EqualTo(events.Count(e => e.Kind == EventKind.DieRolled)));
            Assert.That(r.humanRolls, Is.EqualTo(h.Log.Where(l => l.human).SelectMany(l => l.transition.Events).Count(e => e.Kind == EventKind.DieRolled)));
            Assert.That(r.humanNoMoveRolls, Is.EqualTo(h.Log.Where(l => l.human).SelectMany(l => l.transition.Events).Count(e => e.Kind == EventKind.NoLegalMoves)));
            Assert.That(r.sixes, Is.EqualTo(events.Count(e => e.Kind == EventKind.DieRolled && e.Roll == 6)));
            Assert.That(r.turns, Is.EqualTo(1 + events.Count(e => e.Kind == EventKind.TurnAdvanced)));
            Assert.That(r.rounds, Is.EqualTo(r.turns / 4.0));
            Assert.That(r.knockouts, Is.EqualTo(events.Count(e => e.Kind == EventKind.PieceBanished)));
            Assert.That(r.humanKnockoutsSuffered, Is.EqualTo(events.Count(e => e.Kind == EventKind.PieceBanished && e.Piece.Value.Owner == PlayerId.P0)));
            Assert.That(r.summons, Is.EqualTo(events.Count(e => e.Kind == EventKind.PieceSummoned)));
            Assert.That(r.ascensionEntries, Is.EqualTo(events.Count(e => e.Kind == EventKind.PieceMoved &&
                e.From.Value.Kind == PieceKind.OnMainTrack && e.To.Value.Kind != PieceKind.OnMainTrack)));
            Assert.That(r.completedPieces, Is.EqualTo(events.Count(e => e.Kind == EventKind.PieceCompleted)));
            Assert.That(r.humanCompletedPieces, Is.EqualTo(h.Session.State.Pieces.Count(p => p.Owner == PlayerId.P0 && p.Position.Kind == PieceKind.Completed)));
            Assert.That(r.humanActions, Is.EqualTo(h.HumanActions)); Assert.That(r.botActions, Is.EqualTo(h.BotActions));
            Assert.That(r.meaningfulChoices, Is.EqualTo(h.Choices)); Assert.That(r.forcedChoices, Is.EqualTo(h.Forced));
            // Time attribution: human turn time plus bot presentation time is the whole session.
            Assert.That(r.humanTurnSeconds, Is.EqualTo(h.HumanSeconds).Within(1e-6));
            Assert.That(r.botTurnSeconds, Is.EqualTo(h.BotSeconds).Within(1e-6));
            Assert.That(r.elapsedSeconds, Is.EqualTo(h.Now).Within(1e-6));
            var marks = new[] { 0.0 }.Concat(h.ChoiceTimes).Concat(new[] { h.Now }).ToArray();
            Assert.That(r.longestSecondsWithoutMeaningfulChoice, Is.EqualTo(marks.Zip(marks.Skip(1), (x, y) => y - x).Max()).Within(1e-6));
            Assert.That(r.secondsPerMeaningfulChoice, Is.EqualTo(h.Now / h.Choices).Within(1e-6));
            Assert.That(r.meaningfulChoicesPerMinute, Is.EqualTo(h.Choices / (h.Now / 60)).Within(1e-6));
            Assert.That(r.startUtc, Is.EqualTo("2026-10-01T12:00:00Z"));
            Assert.That(r.endUtc, Is.EqualTo(Epoch.AddSeconds(h.Now).UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            // Same counts as tests/evidence/dc-0006/pacing-matches.csv, seed 1 (independent tool).
            Assert.That(new[] { r.humanActions, r.botActions, r.rolls, r.humanRolls, r.turns, r.sixes, r.humanNoMoveRolls,
                r.meaningfulChoices, r.forcedChoices, r.knockouts, r.humanKnockoutsSuffered, r.summons, r.humanCompletedPieces },
                Is.EqualTo(new[] { 116, 342, 249, 61, 212, 37, 6, 23, 32, 8, 3, 20, 3 }));
            Assert.That(r.winner, Is.EqualTo("P3"));
            Assert.That(r.ToJson(), Does.Contain("\"abortSeconds\": null").And.Contain("\"naturalVictory\": true").And.Contain("\"answers\": null"));
        }

        [Test]
        public void AbortRecordsStopTimeAndIgnoresLaterPlay()
        {
            var h = new Harness(PlaytestVariant.VariantB, 82);
            for (int i = 0; i < 40; i++) h.Step();
            bool humanWaiting = h.Session.IsHumanTurn;
            h.Now += 5;
            h.Recorder.Abort("player stopped");
            var r = h.Recorder.Record;
            Assert.That(h.Recorder.IsRecording, Is.False);
            Assert.That(r.aborted, Is.True); Assert.That(r.naturalVictory, Is.False); Assert.That(r.winner, Is.Null);
            Assert.That(r.abortReason, Is.EqualTo("player stopped"));
            Assert.That(r.abortSeconds, Is.EqualTo(h.Now).Within(1e-6)); Assert.That(r.elapsedSeconds, Is.EqualTo(h.Now).Within(1e-6));
            Assert.That(r.humanTurnSeconds, Is.EqualTo(h.HumanSeconds + (humanWaiting ? 5 : 0)).Within(1e-6));
            Assert.That(r.botTurnSeconds, Is.EqualTo(h.BotSeconds + (humanWaiting ? 0 : 5)).Within(1e-6));
            Assert.That(r.humanActions + r.botActions, Is.EqualTo(40));
            string json = r.ToJson();
            for (int i = 0; i < 30; i++) h.Step();
            h.Recorder.Abort("again");
            Assert.That(r.ToJson(), Is.EqualTo(json), "Nothing is recorded after the stop.");
            Assert.That(json, Does.Contain("\"aborted\": true").And.Contain("\"winner\": null").And.Contain("\"abortReason\": \"player stopped\""));
        }

        [Test]
        public void RestartBeforeVictoryIsRecordedAsAbort()
        {
            var h = new Harness(PlaytestVariant.ControlA, 150);
            for (int i = 0; i < 10; i++) h.Step();
            Assert.That(h.Session.PlayAgain(150), Is.True);
            Assert.That(h.Recorder.Record.aborted, Is.True);
            Assert.That(h.Recorder.Record.abortReason, Is.EqualTo("match restarted before victory"));
            Assert.That(h.Recorder.Record.humanActions + h.Recorder.Record.botActions, Is.EqualTo(10));
        }

        [Test]
        public void RecorderNeedsAStartedUnfinishedMatch()
        {
            Assert.Throws<ArgumentException>(() => new PlaytestRecorder(new LocalMatchSession(82), PlaytestVariant.ControlA,
                "test", () => 0, () => Epoch));
        }

        [Test]
        public void AnswersAreValidatedAndSafelySerialized()
        {
            var h = new Harness(PlaytestVariant.ControlA, 364);
            for (int i = 0; i < 12; i++) h.Step();
            h.Recorder.Abort("player stopped");
            var r = h.Recorder.Record;
            Assert.Throws<ArgumentOutOfRangeException>(() => r.SetAnswers(0, "good", 3, 3, "yes", null));
            Assert.Throws<ArgumentException>(() => r.SetAnswers(3, "fine", 3, 3, "yes", null));
            Assert.That(r.answered, Is.False);
            r.SetAnswers(4, "too long", 2, 5, "maybe", "  Bots were \"slow\"\\\nthen fine  ");
            Assert.That(r.answered, Is.True);
            string json = r.ToJson();
            Assert.That(json, Does.Contain("\"fun\": 4").And.Contain("\"matchLength\": \"too long\"").And.Contain("\"playAgain\": \"maybe\"")
                .And.Contain("\"note\": \"Bots were \\\"slow\\\"\\\\\\nthen fine\""));
#if !UNITY_5_3_OR_NEWER
            using var parsed = System.Text.Json.JsonDocument.Parse(json); // .NET only: proves the file is valid JSON
            var root = parsed.RootElement;
            Assert.That(root.GetProperty("variant").GetString(), Is.EqualTo("ControlA"));
            Assert.That(root.GetProperty("seed").GetInt64(), Is.EqualTo(364));
            Assert.That(root.GetProperty("answers").GetProperty("waiting").GetInt32(), Is.EqualTo(5));
            Assert.That(root.GetProperty("answers").GetProperty("note").GetString(), Is.EqualTo("Bots were \"slow\"\\\nthen fine"));
#endif
        }

        [Test]
        public void SavingWritesOneFilePerSessionAndUpdatesItInPlace()
        {
            var h = new Harness(PlaytestVariant.VariantB, 82);
            for (int i = 0; i < 8; i++) h.Step();
            h.Recorder.Abort("player stopped");
            string directory = Path.Combine(Path.GetTempPath(), "dc0037-" + Guid.NewGuid().ToString("N"));
            try
            {
                string first = PlaytestFiles.Save(h.Recorder.Record, directory);
                Assert.That(Path.GetFileName(first), Is.EqualTo("20261001T120000Z-VariantB-seed82.json"));
                h.Recorder.Record.SetAnswers(3, "good", 3, 2, "yes", null);
                Assert.That(PlaytestFiles.Save(h.Recorder.Record, directory), Is.EqualTo(first));
                Assert.That(Directory.GetFiles(directory).Length, Is.EqualTo(1));
                Assert.That(File.ReadAllText(first), Does.Contain("\"answers\": { \"fun\": 3"));
            }
            finally { Directory.Delete(directory, true); }
        }

        [Test]
        public void ExperimentToolingStaysOutOfPlayerAssemblies()
        {
            const string editorOnly = "\"includePlatforms\"\\s*:\\s*\\[\\s*\"Editor\"\\s*\\]"; // Unity's NUnit takes string patterns
            Assert.That(File.ReadAllText(Asset("Development/DemonCodex.Development.asmdef")), Does.Match(editorOnly));
            Assert.That(File.ReadAllText(Asset("Development/DemonCodex.Development.asmdef")), Does.Contain("\"autoReferenced\": false"));
            Assert.That(File.ReadAllText(Asset("Editor/DemonCodex.Prototype.Editor.asmdef")), Does.Match(editorOnly));
            foreach (var runtime in new[] { "Rules/DemonCodex.Rules.asmdef", "LocalMatch/DemonCodex.LocalMatch.asmdef", "Presentation/DemonCodex.Presentation.asmdef" })
                Assert.That(File.ReadAllText(Asset(runtime)), Does.Not.Contain("DemonCodex.Development"), runtime);
            foreach (var folder in new[] { "Rules", "LocalMatch", "Presentation" })
                foreach (var source in Directory.GetFiles(Asset(folder), "*.cs"))
                    Assert.That(File.ReadAllText(source), Does.Not.Contain("Playtest"), source);
        }

        private static string Asset(string relative, [CallerFilePath] string here = "")
        {
            // here: .../Assets/DemonCodex/Tests/LocalMatch/PlaytestTests.cs (absolute in .NET, project-relative in Unity)
            string path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here), "..", "..", relative));
            return File.Exists(path) || Directory.Exists(path) ? path : Path.GetFullPath(Path.Combine("Assets", "DemonCodex", relative));
        }
    }
}
