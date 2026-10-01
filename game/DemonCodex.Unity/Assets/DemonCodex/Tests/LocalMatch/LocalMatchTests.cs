using System;
using System.Collections.Generic;
using System.Linq;
using DemonCodex.LocalMatch;
using DemonCodex.Rules;
using NUnit.Framework;

namespace DemonCodex.LocalMatchTests
{
    public sealed class FixedDie : IDieSource
    {
        private readonly int value;
        public int Calls { get; private set; }
        public FixedDie(int value) { this.value = value; }
        public int Next() { Calls++; return value; }
    }
    public class SessionTests
    {
        [Test] public void PrototypeConfigurationIsExplicitAndValid()
        {
            var session = new LocalMatchSession();
            Invariants.AssertState(session.State);
            Assert.That(session.State.Pieces.Count, Is.EqualTo(16));
            Assert.That(session.State.Board.MainTrackLength, Is.EqualTo(40));
            Assert.That(session.State.Board.StartIndex.Values, Is.EqualTo(new[] { 0, 10, 20, 30 }));
            Assert.That(session.State.Players.Count(p => p.Control == ControlType.Human), Is.EqualTo(1));
            Assert.That(session.RollHuman(), Is.False);
            Assert.That(session.StepBot(), Is.False);
        }
        [Test] public void HumanRollUsesDomainAndSelectablesAreExactLegalCandidates()
        {
            var die = new FixedDie(6);
            var session = new LocalMatchSession(12, die); session.Start();
            var expected = RulesEngine.Apply(session.State, new ProvideDieRoll(PlayerId.P0, 6, 0));
            Assert.That(session.RollHuman(), Is.True);
            Assert.That(Invariants.Fingerprint(session.State), Is.EqualTo(Invariants.Fingerprint(expected.State)));
            Assert.That(session.SelectableMoves, Is.EqualTo(RulesEngine.LegalMoves(session.State, PlayerId.P0, 6)));
            Assert.That(session.SelectableMoves.Count, Is.EqualTo(4));
            Assert.That(session.RollHuman(), Is.False);
            Assert.That(die.Calls, Is.EqualTo(1));
        }
        [Test] public void SixGrantsBonusAndStaleSelectionIsRejected()
        {
            var session = new LocalMatchSession(1, new FixedDie(6)); session.Start(); session.RollHuman();
            var move = session.SelectableMoves[0];
            var events = new List<EventKind>(); session.Transitioned += t => events.AddRange(t.Events.Select(e => e.Kind));
            Assert.That(session.SelectHuman(move), Is.True);
            Assert.That(session.State.ActivePlayer, Is.EqualTo(PlayerId.P0));
            Assert.That(session.State.Phase, Is.EqualTo(MatchPhase.AwaitingRoll));
            Assert.That(events, Does.Contain(EventKind.BonusRollGranted));
            session.RollHuman();
            var before = session.State;
            Assert.That(session.SelectHuman(move), Is.False);
            Assert.That(session.State, Is.SameAs(before));
        }
        [Test] public void NoMoveTurnsYieldWithoutConsumingExtraDiceOrHumanInput()
        {
            var die = new FixedDie(1);
            var session = new LocalMatchSession(1, die); session.Start(); session.RollHuman();
            Assert.That(session.State.ActivePlayer, Is.EqualTo(PlayerId.P1));
            Assert.That(session.RollHuman(), Is.False);
            Assert.That(session.SelectableMoves, Is.Empty);
            for (int i = 0; i < 3; i++) Assert.That(session.StepBot(), Is.True);
            Assert.That(session.State.ActivePlayer, Is.EqualTo(PlayerId.P0));
            Assert.That(die.Calls, Is.EqualTo(4));
        }
        [TestCase(1u)] [TestCase(2026u)] [TestCase(42u)]
        public void CompleteMatchesAreLegalReproducibleAndRestartable(uint seed)
        {
            string first = Run(seed);
            Assert.That(Run(seed), Is.EqualTo(first));
        }
        private static string Run(uint seed)
        {
            var session = new LocalMatchSession(seed); session.Start();
            bool captured = false, completed = false;
            session.Transitioned += t => {
                captured |= t.Events.Any(e => e.Kind == EventKind.PieceBanished);
                completed |= t.Events.Any(e => e.Kind == EventKind.PieceCompleted);
                Invariants.AssertState(t.State);
            };
            for (int i = 0; i < 20000 && session.State.Phase != MatchPhase.Finished; i++)
            {
                if (!session.IsHumanTurn) Assert.That(session.StepBot(), Is.True);
                else if (session.State.Phase == MatchPhase.AwaitingRoll) Assert.That(session.RollHuman(), Is.True);
                else {
                    var legal = session.LegalMoves;
                    var move = BotPolicy.Choose(session.State, legal);
                    Assert.That(legal, Does.Contain(move));
                    Assert.That(session.SelectHuman(move), Is.True);
                }
            }
            Assert.That(session.State.Phase, Is.EqualTo(MatchPhase.Finished), "test watchdog, not a game draw policy");
            Assert.That(captured && completed, Is.True);
            string fingerprint = Invariants.Fingerprint(session.State);
            var finished = session.State;
            Assert.That(session.RollHuman(), Is.False);
            Assert.That(session.StepBot(), Is.False);
            Assert.That(session.SelectHuman(null), Is.False);
            Assert.That(session.State, Is.SameAs(finished));
            Assert.That(session.PlayAgain(seed), Is.True);
            Assert.That(session.State.Revision, Is.EqualTo(finished.Revision + 1));
            Assert.That(session.State.Pieces.All(p => p.Position.Kind == PieceKind.InAbyss), Is.True);
            Assert.That(session.State.Winner, Is.Null);
            Assert.That(session.RollHuman(), Is.True);
            return fingerprint;
        }
        [Test] public void PolicyUsesCompletionThenCaptureThenSummonThenProgressThenIndex()
        {
            // Ranking candidates is independent of legality; production supplies only engine candidates.
            var state = MatchState.Create(PrototypeBoard.Create());
            LegalMove Candidate(int index, PiecePosition source, PiecePosition target, MoveKind kind = MoveKind.Advance) =>
                new LegalMove(new PieceId(PlayerId.P0, index), kind, source, target, 6, 0);
            var finish = Candidate(3, PiecePosition.Path(5), PiecePosition.Completed);
            var summon = Candidate(2, PiecePosition.Abyss, PiecePosition.Track(0), MoveKind.Summon);
            var far = Candidate(1, PiecePosition.Track(20), PiecePosition.Track(26));
            var near = Candidate(0, PiecePosition.Track(10), PiecePosition.Track(16));
            Assert.That(BotPolicy.Choose(state, new[] { near, far, summon, finish }), Is.SameAs(finish));
            Assert.That(BotPolicy.Choose(state, new[] { near, far, summon }), Is.SameAs(summon));
            Assert.That(BotPolicy.Choose(state, new[] { near, far }), Is.SameAs(far));
            var tied = Candidate(0, far.Source, far.Target);
            Assert.That(BotPolicy.Choose(state, new[] { far, tied }), Is.SameAs(tied));
        }
        [Test] public void PolicyPrioritizesRealCaptureOverSummon()
        {
            var session = new LocalMatchSession(2026); session.Start();
            for (int i = 0; i < 20000 && session.State.Phase != MatchPhase.Finished; i++)
            {
                var legal = session.LegalMoves;
                if (legal.Count > 0)
                {
                    var captures = legal.Where(m => RulesEngine.Apply(session.State,
                        new SelectMove(session.State.ActivePlayer, m, session.State.Revision)).Events.Any(e => e.Kind == EventKind.PieceBanished)).ToArray();
                    if (captures.Length > 0 && legal.Any(m => m.Kind == MoveKind.Summon) && !legal.Any(m => m.Target.Kind == PieceKind.Completed))
                    { Assert.That(captures, Does.Contain(BotPolicy.Choose(session.State, legal))); return; }
                }
                if (!session.IsHumanTurn) session.StepBot();
                else if (session.State.Phase == MatchPhase.AwaitingRoll) session.RollHuman();
                else session.SelectHuman(BotPolicy.Choose(session.State, legal));
            }
            Assert.Fail("Seed did not exercise capture-versus-summon choice.");
        }
    }
}
