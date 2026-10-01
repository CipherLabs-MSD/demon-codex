using System.Linq;
using DemonCodex.Development;
using DemonCodex.LocalMatch;
using DemonCodex.Rules;
using NUnit.Framework;

namespace DemonCodex.LocalMatchTests
{
    public class ScenarioTests
    {
        [TestCase(ScenarioKind.Knockout)]
        [TestCase(ScenarioKind.Ascension)]
        [TestCase(ScenarioKind.Completion)]
        [TestCase(ScenarioKind.Victory)]
        public void ReplayProducesValidLegalHumanOutcome(ScenarioKind kind)
        {
            var scenario = ScenarioReplay.Prepare(kind);
            var session = scenario.Session;
            var before = session.State;
            Invariants.AssertState(before);
            Assert.That(before.ActivePlayer, Is.EqualTo(PlayerId.P0));
            Assert.That(session.SelectableMoves, Does.Contain(scenario.IntendedMove));
            Assert.That(before.Revision, Is.GreaterThan(0), "Must be reached through real commands.");
            Transition actual = null;
            session.Transitioned += t => actual = t;
            Assert.That(session.SelectHuman(scenario.IntendedMove), Is.True);
            Invariants.AssertTransition(before, new SelectMove(PlayerId.P0, scenario.IntendedMove, before.Revision), actual);
            Assert.That(ScenarioReplay.Matches(kind, scenario.IntendedMove, actual), Is.True);
            // Independent outcome assertions, not just the fixture's own predicate.
            switch (kind)
            {
                case ScenarioKind.Knockout:
                    var banished = actual.Events.Single(e => e.Kind == EventKind.PieceBanished).Piece.Value;
                    Assert.That(before.Piece(banished).Position.Kind, Is.EqualTo(PieceKind.OnMainTrack));
                    Assert.That(actual.State.Piece(banished).Position.Kind, Is.EqualTo(PieceKind.InAbyss));
                    break;
                case ScenarioKind.Ascension:
                    Assert.That(scenario.IntendedMove.Source.Kind, Is.EqualTo(PieceKind.OnMainTrack));
                    Assert.That(actual.State.Piece(scenario.IntendedMove.Piece).Position.Kind, Is.EqualTo(PieceKind.OnAscensionPath));
                    break;
                case ScenarioKind.Completion:
                    Assert.That(actual.State.Piece(scenario.IntendedMove.Piece).Position.Kind, Is.EqualTo(PieceKind.Completed));
                    Assert.That(actual.State.Winner, Is.Null);
                    break;
                case ScenarioKind.Victory:
                    Assert.That(before.Pieces.Count(p => p.Owner == PlayerId.P0 && p.Position.Kind == PieceKind.Completed), Is.EqualTo(3));
                    Assert.That(actual.State.Winner, Is.EqualTo(PlayerId.P0));
                    Assert.That(actual.State.Phase, Is.EqualTo(MatchPhase.Finished));
                    break;
            }
            var again = ScenarioReplay.Prepare(kind);
            Assert.That(Invariants.Fingerprint(again.Session.State), Is.EqualTo(Invariants.Fingerprint(before)));
            TestContext.WriteLine(kind + ": seed " + session.Seed + ", revision " + before.Revision + ", " + scenario.Instruction);
        }
        [Test] public void PreparingScenariosDoesNotChangeNormalStartOrSeededPlay()
        {
            var baseline = new LocalMatchSession(2026);
            string initial = Invariants.Fingerprint(baseline.State);
            ScenarioReplay.Prepare(ScenarioKind.Victory);
            var normal = new LocalMatchSession(2026);
            Assert.That(normal.Started, Is.False);
            Assert.That(Invariants.Fingerprint(normal.State), Is.EqualTo(initial));
            normal.Start(); baseline.Start();
            for (int i = 0; i < 100; i++)
            {
                Step(normal); Step(baseline);
                Assert.That(Invariants.Fingerprint(normal.State), Is.EqualTo(Invariants.Fingerprint(baseline.State)));
            }
        }
        private static void Step(LocalMatchSession s)
        {
            if (!s.IsHumanTurn) s.StepBot();
            else if (s.State.Phase == MatchPhase.AwaitingRoll) s.RollHuman();
            else s.SelectHuman(BotPolicy.Choose(s.State, s.LegalMoves));
        }
    }
}
