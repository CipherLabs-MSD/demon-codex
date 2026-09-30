using System;
using System.Collections.Generic;
using System.Linq;
using DemonCodex.Rules;
using DemonCodex.Simulation;
using NUnit.Framework;

namespace DemonCodex.Rules.Tests
{
    public sealed class RulesTests
    {
        private static readonly PlayerId[] Ids = { PlayerId.P0, PlayerId.P1, PlayerId.P2, PlayerId.P3 };
        // Synthetic test fixture. These numbers are NOT the product board.
        private static BoardConfiguration Board(int length = 20, int path = 4, PlayerId initial = PlayerId.P0,
            PlayerId[] order = null, bool extraSafe = true)
        {
            var starts = Ids.ToDictionary(p => p, p => (int)p * (length / 4));
            return new BoardConfiguration(length, starts, starts.Values.Concat(extraSafe ? new[] { 3 } : Array.Empty<int>()),
                Ids.ToDictionary(p => p, p => path), order ?? Ids, initial, PlayerId.P0);
        }
        private static PieceId Id(int index = 0, PlayerId owner = PlayerId.P0) => new PieceId(owner, index);
        private static PieceState At(int index, PiecePosition pos, PlayerId owner = PlayerId.P0) => new PieceState(Id(index, owner), pos);
        private static MatchState Fixture(BoardConfiguration board = null, PlayerId active = PlayerId.P0, params PieceState[] placements)
        {
            var initial = MatchState.Create(board ?? Board());
            var state = new MatchState(initial.Board, initial.Pieces.Select(p => placements.FirstOrDefault(x => x.Id.Equals(p.Id)) ?? p),
                MatchPhase.AwaitingRoll, active, null, null, 40);
            Invariants.AssertState(state);
            return state;
        }
        private static Transition Act(MatchState state, Command command)
        {
            string original = Invariants.Fingerprint(state);
            var result = RulesEngine.Apply(state, command);
            Assert.That(Invariants.Transition(state, command, result), Is.Empty);
            Assert.That(Invariants.Fingerprint(state), Is.EqualTo(original), "Input snapshot mutated");
            return result;
        }
        private static MatchState Roll(MatchState state, int roll)
        {
            var result = Act(state, new ProvideDieRoll(state.ActivePlayer, roll, state.Revision));
            Assert.That(result.Accepted, Is.True);
            return result.State;
        }
        private static Transition Select(MatchState state, int index = 0)
        {
            var move = RulesEngine.LegalMoves(state, state.ActivePlayer, state.PendingRoll.Value).Single(m => m.Piece.Index == index);
            var result = Act(state, new SelectMove(state.ActivePlayer, move, state.Revision));
            Assert.That(result.Accepted, Is.True);
            return result;
        }
        private static void Events(Transition result, params EventKind[] kinds) => Assert.That(result.Events.Select(e => e.Kind), Is.EqualTo(kinds));

        [Test] public void T01_Initialization()
        {
            var state = MatchState.Create(Board(initial: PlayerId.P2));
            Invariants.AssertState(state);
            Assert.That(state.Pieces.Count, Is.EqualTo(16));
            Assert.That(state.Pieces.All(p => p.Position.Equals(PiecePosition.Abyss)), Is.True);
            Assert.That(state.ActivePlayer, Is.EqualTo(PlayerId.P2));
            Assert.That(state.Players.Count(p => p.Control == ControlType.Human), Is.EqualTo(1));
            Assert.That(state.Phase, Is.EqualTo(MatchPhase.AwaitingRoll));
            Assert.That(state.Revision, Is.Zero);
        }

        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void T02_SummonOnlyOnSix(int roll)
        {
            var state = MatchState.Create(Board());
            var moves = RulesEngine.LegalMoves(state, state.ActivePlayer, roll);
            Assert.That(moves.Count, Is.EqualTo(roll == 6 ? 4 : 0));
            Assert.That(moves.All(m => m.Kind == MoveKind.Summon && m.Target.Equals(PiecePosition.Track(0))), Is.True);
            if (roll == 6)
            {
                var result = Select(Roll(state, roll), 2);
                Assert.That(result.State.Piece(Id(2)).Position, Is.EqualTo(PiecePosition.Track(0)));
                Events(result, EventKind.PieceSummoned, EventKind.BonusRollGranted);
            }
        }

        [Test] public void T03_FriendlyStartBlocksSummonButAllowsAdvance()
        {
            var state = Fixture(placements: new[] { At(0, PiecePosition.Track(0)) });
            var moves = RulesEngine.LegalMoves(state, PlayerId.P0, 6);
            Assert.That(moves.Count, Is.EqualTo(1));
            Assert.That(moves[0].Kind, Is.EqualTo(MoveKind.Advance));
            Assert.That(moves[0].Target, Is.EqualTo(PiecePosition.Track(6)));
        }

        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void T04_ExactMovementAndSharedIndexWrap(int roll)
        {
            var state = Fixture(active: PlayerId.P3, placements: new[] { At(0, PiecePosition.Track(4), PlayerId.P3) });
            var result = Select(Roll(state, roll));
            var position = result.State.Piece(Id(0, PlayerId.P3)).Position;
            Assert.That(position, Is.EqualTo(PiecePosition.Track(4 + roll)));
            Assert.That(state.Board.TrackIndex(PlayerId.P3, position.Step), Is.EqualTo((19 + roll) % 20));
        }

        [Test] public void T05_CompleteMoveSetIncludesSummonAndAdvance()
        {
            var state = Fixture(placements: new[] { At(0, PiecePosition.Track(1)), At(1, PiecePosition.Track(2)) });
            var ready = Roll(state, 6);
            var moves = RulesEngine.LegalMoves(ready, PlayerId.P0, 6);
            Assert.That(moves.Select(m => m.Piece.Index), Is.EqualTo(new[] { 0, 1, 2, 3 }));
            Assert.That(moves.Select(m => m.Kind), Is.EqualTo(new[] { MoveKind.Advance, MoveKind.Advance, MoveKind.Summon, MoveKind.Summon }));
            Assert.That(moves.Select(m => m.Target.Step), Is.EqualTo(new[] { 7, 8, 0, 0 }));
            var result = Select(ready, 1);
            Assert.That(result.State.Piece(Id(0)).Position, Is.EqualTo(state.Piece(Id(0)).Position));
            Assert.That(result.State.Piece(Id(1)).Position, Is.EqualTo(PiecePosition.Track(8)));
        }

        [Test] public void T06_TestPolicyIsDeterministicAndLegal()
        {
            var state = Roll(MatchState.Create(Board()), 6);
            var moves = RulesEngine.LegalMoves(state, PlayerId.P0, 6);
            Assert.That(TestMovePolicy.Choose(moves), Is.EqualTo(TestMovePolicy.Choose(moves.Reverse().ToArray())));
            Assert.That(moves.Contains(TestMovePolicy.Choose(moves)), Is.True);
            Assert.That(TestMovePolicy.Choose(moves).Piece.Index, Is.Zero);
        }

        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void T07_NoLegalMoveAdvancesTurn(int roll)
        {
            var state = MatchState.Create(Board());
            var result = Act(state, new ProvideDieRoll(PlayerId.P0, roll, 0));
            Events(result, EventKind.DieRolled, EventKind.NoLegalMoves, EventKind.TurnAdvanced);
            Assert.That(result.State.ActivePlayer, Is.EqualTo(PlayerId.P1));
            Assert.That(result.State.PendingRoll, Is.Null);
        }

        [Test] public void T08_SixBonusAfterMove()
        {
            var result = Select(Roll(Fixture(placements: new[] { At(0, PiecePosition.Track(1)) }), 6));
            Events(result, EventKind.PieceMoved, EventKind.BonusRollGranted);
            Assert.That(result.State.ActivePlayer, Is.EqualTo(PlayerId.P0));
        }

        [Test] public void T09_NoMoveSixStillGrantsBonus()
        {
            var state = Fixture(placements: new[] { At(0, PiecePosition.Track(15), PlayerId.P1) });
            var result = Act(state, new ProvideDieRoll(PlayerId.P0, 6, state.Revision));
            Events(result, EventKind.DieRolled, EventKind.NoLegalMoves, EventKind.BonusRollGranted);
            Assert.That(result.State.ActivePlayer, Is.EqualTo(PlayerId.P0));
            Assert.That(result.State.Phase, Is.EqualTo(MatchPhase.AwaitingRoll));
        }

        [Test] public void T10_KnockoutIsAtomicAndResetsVictim()
        {
            var state = Fixture(placements: new[] { At(0, PiecePosition.Track(0)), At(0, PiecePosition.Track(16), PlayerId.P1) });
            var result = Select(Roll(state, 1));
            Events(result, EventKind.PieceBanished, EventKind.PieceMoved, EventKind.TurnAdvanced);
            Assert.That(result.State.Piece(Id(0, PlayerId.P1)).Position, Is.EqualTo(PiecePosition.Abyss));
            Assert.That(result.State.Piece(Id()).Position, Is.EqualTo(PiecePosition.Track(1)));
            Assert.That(result.Events[0].Piece, Is.EqualTo(Id(0, PlayerId.P1)));
            Assert.That(result.Events[0].From, Is.EqualTo(PiecePosition.Track(16)));
        }

        [Test] public void T11_AllSafeDestinationsBlockOpponentsButAllowEmptyEntry()
        {
            var board = Board();
            foreach (int square in board.SafeSpaceIndices)
            {
                var owner = Ids.First(p => board.StartIndex[p] != square);
                var opponent = Ids.First(p => p != owner);
                int target = (square - board.StartIndex[owner] + 20) % 20;
                var moving = At(0, PiecePosition.Track(target - 1), owner);
                var empty = Fixture(board, owner, moving);
                Assert.That(RulesEngine.LegalMoves(empty, owner, 1).Count, Is.EqualTo(1));
                var blocked = Fixture(board, owner, moving,
                    At(0, PiecePosition.Track((square - board.StartIndex[opponent] + 20) % 20), opponent));
                Assert.That(RulesEngine.LegalMoves(blocked, owner, 1), Is.Empty);
                var result = Act(blocked, new ProvideDieRoll(owner, 1, blocked.Revision));
                Assert.That(result.Events.Any(e => e.Kind == EventKind.PieceBanished), Is.False);
            }
        }

        [Test] public void T12_FriendlyTrackAndPathOccupancyRejectDestinations()
        {
            foreach (var pair in new[] {
                new[] { At(0, PiecePosition.Track(1)), At(1, PiecePosition.Track(3)) },
                new[] { At(0, PiecePosition.Path(1)), At(1, PiecePosition.Path(3)) } })
            {
                var state = Fixture(placements: pair.Concat(new[] { At(2, PiecePosition.Track(6)) }).ToArray());
                Assert.That(RulesEngine.LegalMoves(state, PlayerId.P0, 2).Any(m => m.Piece.Equals(Id())), Is.False);
                var ready = Roll(state, 2);
                var forged = new LegalMove(Id(), MoveKind.Advance, pair[0].Position, pair[1].Position, 2, ready.Revision);
                var result = Act(ready, new SelectMove(PlayerId.P0, forged, ready.Revision));
                Assert.That(result.Error, Is.EqualTo(Rejection.IllegalMove));
            }
        }

        [Test] public void T13_IntermediateOccupancyNeverBlocksOrCaptures()
        {
            var state = Fixture(placements: new[] { At(0, PiecePosition.Track(0)), At(1, PiecePosition.Track(1)),
                At(0, PiecePosition.Track(17), PlayerId.P1), At(0, PiecePosition.Track(13), PlayerId.P2) });
            var result = Select(Roll(state, 4));
            Assert.That(result.State.Piece(Id()).Position, Is.EqualTo(PiecePosition.Track(4)));
            Assert.That(result.Events.Any(e => e.Kind == EventKind.PieceBanished), Is.False);
            var path = Fixture(placements: new[] { At(0, PiecePosition.Path(1)), At(1, PiecePosition.Path(2)) });
            Assert.That(Select(Roll(path, 2)).State.Piece(Id()).Position, Is.EqualTo(PiecePosition.Path(3)));
        }

        [TestCase(1)] [TestCase(4)]
        public void T14_TrackToPrivatePathAndMinimalPath(int length)
        {
            var state = Fixture(Board(path: length), placements: new[] { At(0, PiecePosition.Track(19)) });
            var result = Select(Roll(state, 1));
            Assert.That(result.State.Piece(Id()).Position, Is.EqualTo(length == 1 ? PiecePosition.Completed : PiecePosition.Path(1)));
            if (length == 4)
            {
                var cross = Fixture(Board(path: length), placements: new[] { At(0, PiecePosition.Track(18)) });
                Assert.That(Select(Roll(cross, 3)).State.Piece(Id()).Position, Is.EqualTo(PiecePosition.Path(2)));
            }
        }

        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void T15_OvershootExcludedButOtherPiecesCanMove(int remaining)
        {
            var state = Fixture(Board(path: 7), placements: new[] { At(0, PiecePosition.Path(7 - remaining)), At(1, PiecePosition.Track(1)) });
            var moves = RulesEngine.LegalMoves(state, PlayerId.P0, remaining + 1);
            Assert.That(moves.Any(m => m.Piece.Equals(Id())), Is.False);
            Assert.That(moves.Any(m => m.Piece.Equals(Id(1))), Is.True);
        }

        [Test] public void T16_ExactCompletionIsNonOccupyingAndImmobile()
        {
            var state = Fixture(placements: new[] { At(0, PiecePosition.Path(3)), At(1, PiecePosition.Completed) });
            var result = Select(Roll(state, 1));
            Events(result, EventKind.PieceMoved, EventKind.PieceCompleted, EventKind.TurnAdvanced);
            Assert.That(result.State.Piece(Id()).Position, Is.EqualTo(PiecePosition.Completed));
            var direct = Fixture(placements: new[] { At(0, PiecePosition.Track(18)) });
            Assert.That(Select(Roll(direct, 5)).State.Piece(Id()).Position, Is.EqualTo(PiecePosition.Completed));
            var completed = Fixture(placements: new[] { At(0, PiecePosition.Completed) });
            Assert.That(RulesEngine.LegalMoves(completed, PlayerId.P0, 6).Any(m => m.Piece.Equals(Id())), Is.False);
        }

        [Test] public void T17_VictoryOnSixOverridesBonusAndFreezesNormalCommands()
        {
            var state = Fixture(Board(path: 7), placements: new[] { At(0, PiecePosition.Path(1)),
                At(1, PiecePosition.Completed), At(2, PiecePosition.Completed), At(3, PiecePosition.Completed) });
            var ready = Roll(state, 6);
            var move = RulesEngine.LegalMoves(ready, PlayerId.P0, 6).Single();
            var result = Act(ready, new SelectMove(PlayerId.P0, move, ready.Revision));
            Events(result, EventKind.PieceMoved, EventKind.PieceCompleted, EventKind.MatchWon);
            Assert.That(result.State.Winner, Is.EqualTo(PlayerId.P0));
            Assert.That(result.State.Phase, Is.EqualTo(MatchPhase.Finished));
            Assert.That(Act(result.State, new ProvideDieRoll(PlayerId.P0, 6, result.State.Revision)).Error, Is.EqualTo(Rejection.WrongPhase));
            Assert.That(Act(result.State, new SelectMove(PlayerId.P0, move, result.State.Revision)).Error, Is.EqualTo(Rejection.WrongPhase));
            Assert.That(Act(result.State, new Restart(result.State.Revision)).State.Phase, Is.EqualTo(MatchPhase.AwaitingRoll));
        }

        [TestCase(1)] [TestCase(6)]
        public void T18_KnockoutAndCompletionDoNotCreateTheirOwnBonus(int roll)
        {
            var capture = Fixture(placements: new[] { At(0, PiecePosition.Track(0)), At(0, PiecePosition.Track((roll + 15) % 20), PlayerId.P1) });
            var captured = Select(Roll(capture, roll));
            var completion = Fixture(Board(path: 7), placements: new[] { At(0, PiecePosition.Path(7 - roll)) });
            var completed = Select(Roll(completion, roll));
            foreach (var result in new[] { captured, completed })
            {
                Assert.That(result.Events.Count(e => e.Kind == EventKind.BonusRollGranted), Is.EqualTo(roll == 6 ? 1 : 0));
                Assert.That(result.State.ActivePlayer, Is.EqualTo(roll == 6 ? PlayerId.P0 : PlayerId.P1));
            }
        }

        [Test] public void T19_RepeatedSixesHaveNoCapWithLegalAndEmptyMoves()
        {
            var state = Fixture(placements: new[] { At(0, PiecePosition.Track(15), PlayerId.P1) });
            for (int i = 0; i < 10; i++) { state = Roll(state, 6); Assert.That(state.ActivePlayer, Is.EqualTo(PlayerId.P0)); }
            state = MatchState.Create(Board());
            for (int i = 0; i < 10; i++)
            {
                state = Roll(state, 6);
                if (state.Phase == MatchPhase.AwaitingSelection)
                    state = Select(state, TestMovePolicy.Choose(RulesEngine.LegalMoves(state, PlayerId.P0, 6)).Piece.Index).State;
                Assert.That(state.ActivePlayer, Is.EqualTo(PlayerId.P0));
            }
        }

        [Test] public void T20_ConfiguredOrderWrapsAndInitialPlayerIsExplicit()
        {
            var order = new[] { PlayerId.P2, PlayerId.P0, PlayerId.P3, PlayerId.P1 };
            var state = MatchState.Create(Board(initial: PlayerId.P1, order: order));
            foreach (var expected in new[] { PlayerId.P2, PlayerId.P0, PlayerId.P3, PlayerId.P1 })
            { state = Roll(state, 1); Assert.That(state.ActivePlayer, Is.EqualTo(expected)); }
        }

        [Test] public void T21_RestartClearsStateAndInvalidatesPreviousMoveTokens()
        {
            var initial = MatchState.Create(Board(initial: PlayerId.P2));
            var ready = Roll(initial, 6);
            var old = RulesEngine.LegalMoves(ready, PlayerId.P2, 6)[0];
            var afterBonus = Select(ready).State;
            foreach (var before in new[] { ready, afterBonus })
            {
                var reset = Act(before, new Restart(before.Revision));
                Events(reset, EventKind.MatchRestarted);
                Assert.That(reset.State.ActivePlayer, Is.EqualTo(PlayerId.P2));
                Assert.That(reset.State.Pieces.All(p => p.Position.Kind == PieceKind.InAbyss), Is.True);
                Assert.That(reset.State.Revision, Is.GreaterThan(before.Revision));
                var newReady = Roll(reset.State, 6);
                Assert.That(Act(newReady, new SelectMove(PlayerId.P2, old, newReady.Revision)).Error, Is.EqualTo(Rejection.StaleRevision));
            }
        }

        [Test] public void T22_InvalidAndDuplicateActionsPreserveSnapshot()
        {
            var initial = MatchState.Create(Board());
            Assert.That(Act(initial, null).Error, Is.EqualTo(Rejection.InvalidCommand));
            Assert.That(Act(initial, new ProvideDieRoll(PlayerId.P1, 6, 0)).Error, Is.EqualTo(Rejection.WrongPlayer));
            Assert.That(Act(initial, new SelectMove(PlayerId.P0, null, 0)).Error, Is.EqualTo(Rejection.WrongPhase));
            Assert.That(Act(initial, new Restart(99)).Error, Is.EqualTo(Rejection.StaleRevision));
            foreach (int invalid in new[] { -1, 0, 7, int.MaxValue })
                Assert.That(Act(initial, new ProvideDieRoll(PlayerId.P0, invalid, 0)).Error, Is.EqualTo(Rejection.InvalidRoll));
            var ready = Roll(initial, 6);
            var move = RulesEngine.LegalMoves(ready, PlayerId.P0, 6)[0];
            Assert.That(Act(ready, new ProvideDieRoll(PlayerId.P0, 6, ready.Revision)).Error, Is.EqualTo(Rejection.WrongPhase));
            Assert.That(Act(ready, new SelectMove(PlayerId.P1, move, ready.Revision)).Error, Is.EqualTo(Rejection.WrongPlayer));
            Assert.That(Act(ready, new SelectMove(PlayerId.P0, null, ready.Revision)).Error, Is.EqualTo(Rejection.IllegalMove));
            var forged = new LegalMove(move.Piece, move.Kind, move.Source, PiecePosition.Completed, 6, ready.Revision);
            Assert.That(Act(ready, new SelectMove(PlayerId.P0, forged, ready.Revision)).Error, Is.EqualTo(Rejection.IllegalMove));
            var wrongRoll = new LegalMove(move.Piece, move.Kind, move.Source, move.Target, 5, ready.Revision);
            Assert.That(Act(ready, new SelectMove(PlayerId.P0, wrongRoll, ready.Revision)).Error, Is.EqualTo(Rejection.InvalidRoll));
            var command = new SelectMove(PlayerId.P0, move, ready.Revision);
            var moved = Act(ready, command);
            Assert.That(Act(moved.State, command).Error, Is.EqualTo(Rejection.StaleRevision));
            var badSource = new LegalMove(move.Piece, MoveKind.Advance, PiecePosition.Track(9), move.Target, 6, ready.Revision);
            Assert.That(Act(ready, new SelectMove(PlayerId.P0, badSource, ready.Revision)).Accepted, Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => RulesEngine.LegalMoves(initial, PlayerId.P0, 7));
            Assert.Throws<ArgumentException>(() => RulesEngine.LegalMoves(initial, PlayerId.P1, 6));
            Assert.Throws<InvalidOperationException>(() => RulesEngine.LegalMoves(ready, PlayerId.P0, 5));
        }

        [Test] public void T23_InvalidConfigurationRejectedAndCollectionsDefensivelyCopied()
        {
            var starts = Ids.ToDictionary(p => p, p => (int)p * 5);
            var paths = Ids.ToDictionary(p => p, p => 4);
            Assert.Throws<ArgumentOutOfRangeException>(() => Board(length: 3));
            Assert.Throws<ArgumentException>(() => Board(path: 0));
            Assert.Throws<ArgumentException>(() => Board(initial: (PlayerId)77));
            Assert.Throws<ArgumentException>(() => Board(order: new[] { PlayerId.P0, PlayerId.P0, PlayerId.P2, PlayerId.P3 }));
            Assert.Throws<ArgumentNullException>(() => new BoardConfiguration(20, null, starts.Values, paths, Ids, PlayerId.P0, PlayerId.P0));
            Assert.Throws<ArgumentException>(() => new BoardConfiguration(20, starts, new[] { 0, 5, 10 }, paths, Ids, PlayerId.P0, PlayerId.P0));
            Assert.Throws<ArgumentException>(() => new BoardConfiguration(20, starts, new[] { 0, 5, 10, 15, 20 }, paths, Ids, PlayerId.P0, PlayerId.P0));
            var invalid = new Dictionary<PlayerId, int>(starts) { [PlayerId.P1] = 0 };
            Assert.Throws<ArgumentException>(() => new BoardConfiguration(20, invalid, starts.Values, paths, Ids, PlayerId.P0, PlayerId.P0));
            invalid = new Dictionary<PlayerId, int>(starts); invalid.Remove(PlayerId.P1);
            Assert.Throws<ArgumentException>(() => new BoardConfiguration(20, invalid, starts.Values, paths, Ids, PlayerId.P0, PlayerId.P0));
            var safe = starts.Values.ToList(); var order = Ids.ToArray();
            var board = new BoardConfiguration(20, starts, safe, paths, order, PlayerId.P0, PlayerId.P0);
            starts[PlayerId.P0] = 19; paths[PlayerId.P0] = 99; safe.Clear(); order[0] = PlayerId.P3;
            Assert.That(board.StartIndex[PlayerId.P0], Is.Zero);
            Assert.That(board.FinalPathLength[PlayerId.P0], Is.EqualTo(4));
            Assert.That(board.SafeSpaceIndices.Count, Is.EqualTo(4));
            Assert.That(board.PlayerOrder[0], Is.EqualTo(PlayerId.P0));
            Invariants.AssertState(MatchState.Create(Board(length: 4, path: 1, extraSafe: false)));
        }

        [Test] public void T24_DeterministicReplayIncludesEventOrder()
        {
            var first = MatchSimulator.Run(123);
            var replay = MatchSimulator.Run(123);
            Assert.That(first.Failure, Is.Null);
            Assert.That(first.Winner, Is.Not.Null);
            Assert.That(first.Trace, Is.EqualTo(replay.Trace));
            Assert.That(first.TraceHash, Is.EqualTo(replay.TraceHash));
        }

        [Test] public void T25_SeededMatchesEnforceAllInvariants()
        {
            for (uint seed = 1; seed <= 12; seed++)
            {
                var result = MatchSimulator.Run(seed);
                Assert.That(result.Failure, Is.Null, "seed=" + seed);
                Assert.That(result.Watchdog, Is.False, "seed=" + seed);
                Assert.That(result.Winner, Is.Not.Null, "seed=" + seed);
            }
        }

        [Test] public void WatchdogIsEvidenceNotADrawAndPreservesTrace()
        {
            var result = MatchSimulator.Run(7, 1);
            Assert.That(result.Watchdog, Is.True);
            Assert.That(result.Winner, Is.Null);
            Assert.That(result.Failure, Is.Null);
            Assert.That(result.Trace.Last(), Does.StartWith("WATCHDOG"));
            Assert.That(result.Rolls, Is.EqualTo(1));
        }

        [Test] public void InvariantDiagnosticsDetectCorruption()
        {
            var initial = MatchState.Create(Board());
            var duplicate = new MatchState(initial.Board, initial.Pieces.Take(15).Concat(new[] { initial.Pieces[0] }),
                initial.Phase, initial.ActivePlayer, null, null, 0);
            Assert.That(Invariants.State(duplicate).Any(s => s.StartsWith("I01")), Is.True);
            var stacked = new MatchState(initial.Board, initial.Pieces.Select(p => p.Owner == PlayerId.P0 ?
                new PieceState(p.Id, PiecePosition.Track(0)) : p), initial.Phase, initial.ActivePlayer, null, null, 0);
            Assert.That(Invariants.State(stacked).Any(s => s.StartsWith("I05")), Is.True);
            var falseWinner = new MatchState(initial.Board, initial.Pieces, MatchPhase.Finished, PlayerId.P0, null, PlayerId.P0, 0);
            Assert.That(Invariants.State(falseWinner).Any(s => s.StartsWith("I08")), Is.True);
            var badPath = new MatchState(initial.Board, initial.Pieces.Select(p => p.Id.Equals(Id()) ?
                At(0, PiecePosition.Path(4)) : p), initial.Phase, initial.ActivePlayer, null, null, 0);
            Assert.That(Invariants.State(badPath).Any(s => s.StartsWith("I02/I04")), Is.True);
        }

        [Test] public void LargeRouteArithmeticAndRevisionDoNotOverflow()
        {
            var board = Board(int.MaxValue, int.MaxValue, extraSafe: false);
            var state = Fixture(board, PlayerId.P3, At(0, PiecePosition.Track(int.MaxValue - 1), PlayerId.P3));
            var result = Select(Roll(state, 6));
            Assert.That(result.State.Piece(Id(0, PlayerId.P3)).Position, Is.EqualTo(PiecePosition.Path(6)));
            var exhausted = new MatchState(board, state.Pieces, state.Phase, state.ActivePlayer, null, null, long.MaxValue);
            Assert.That(Act(exhausted, new Restart(long.MaxValue)).Error, Is.EqualTo(Rejection.RevisionExhausted));
        }
    }
}
