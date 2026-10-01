using System;
using System.Collections.Generic;
using System.Linq;
using DemonCodex.Rules;

namespace DemonCodex.LocalMatch
{
    // M001 PROTOTYPE CONFIGURATION, not permanent product canon.
    public static class PrototypeBoard
    {
        public static BoardConfiguration Create()
        {
            var players = new[] { PlayerId.P0, PlayerId.P1, PlayerId.P2, PlayerId.P3 };
            return new BoardConfiguration(40, players.ToDictionary(p => p, p => (int)p * 10),
                new[] { 0, 5, 10, 15, 20, 25, 30, 35 }, players.ToDictionary(p => p, p => 6),
                players, PlayerId.P0, PlayerId.P0);
        }
    }

    public interface IDieSource { int Next(); }

    // Session randomness is deliberately outside the authoritative reducer.
    public sealed class SeededSessionDie : IDieSource
    {
        private uint state;
        public SeededSessionDie(uint seed) { state = seed == 0 ? 0x6d2b79f5u : seed; }
        public int Next()
        {
            uint value;
            do { state ^= state << 13; state ^= state >> 17; state ^= state << 5; value = state; }
            while (value >= uint.MaxValue - uint.MaxValue % 6);
            return (int)(value % 6) + 1;
        }
    }

    public static class BotPolicy
    {
        // Rank ONLY domain-supplied legal candidates. No move legality is recreated here.
        public static LegalMove Choose(MatchState state, IReadOnlyList<LegalMove> legal)
        {
            if (legal == null || legal.Count == 0) throw new ArgumentException("Legal candidates required.");
            return legal.OrderByDescending(m => m.Target.Kind == PieceKind.Completed)
                .ThenByDescending(m => IsCapture(state, m))
                .ThenByDescending(m => m.Kind == MoveKind.Summon)
                .ThenByDescending(m => Progress(state.Board, m.Source))
                .ThenBy(m => m.Piece.Index).First();
        }
        private static bool IsCapture(MatchState state, LegalMove move) =>
            move.Target.Kind == PieceKind.OnMainTrack && state.Pieces.Any(p =>
                p.Owner != move.Piece.Owner && p.Position.Kind == PieceKind.OnMainTrack &&
                state.Board.TrackIndex(p.Owner, p.Position.Step) ==
                state.Board.TrackIndex(move.Piece.Owner, move.Target.Step));
        private static int Progress(BoardConfiguration board, PiecePosition position) =>
            position.Kind == PieceKind.OnAscensionPath ? board.MainTrackLength + position.Step - 1 :
            position.Kind == PieceKind.OnMainTrack ? position.Step : -1;
    }

    public sealed class LocalMatchSession
    {
        private IDieSource die;
        public MatchState State { get; private set; }
        public uint Seed { get; private set; }
        public bool Started { get; private set; }
        public event Action<Transition> Transitioned;
        public LocalMatchSession(uint seed = 2026, IDieSource dieSource = null)
        { Seed = seed; die = dieSource ?? new SeededSessionDie(seed); State = MatchState.Create(PrototypeBoard.Create()); }
        public bool IsHumanTurn => Started && State.Phase != MatchPhase.Finished &&
            State.Players.Single(p => p.Id == State.ActivePlayer).Control == ControlType.Human;
        public IReadOnlyList<LegalMove> LegalMoves => State.Phase == MatchPhase.AwaitingSelection
            ? RulesEngine.LegalMoves(State, State.ActivePlayer, State.PendingRoll.Value) : Array.Empty<LegalMove>();
        public IReadOnlyList<LegalMove> SelectableMoves => IsHumanTurn ? LegalMoves : Array.Empty<LegalMove>();
        public void Start() { Started = true; }
        public bool RollHuman() => IsHumanTurn && State.Phase == MatchPhase.AwaitingRoll && Roll();
        public bool SelectHuman(LegalMove move) => IsHumanTurn && Apply(new SelectMove(State.ActivePlayer, move, State.Revision));
        public bool StepBot()
        {
            if (!Started || IsHumanTurn || State.Phase == MatchPhase.Finished) return false;
            return State.Phase == MatchPhase.AwaitingRoll ? Roll() :
                Apply(new SelectMove(State.ActivePlayer, BotPolicy.Choose(State, LegalMoves), State.Revision));
        }
        private bool Roll() => Apply(new ProvideDieRoll(State.ActivePlayer, die.Next(), State.Revision));
        private bool Apply(Command command)
        {
            var result = RulesEngine.Apply(State, command);
            if (!result.Accepted) return false;
            State = result.State;
            Transitioned?.Invoke(result);
            return true;
        }
        public bool PlayAgain(uint seed)
        {
            var result = RulesEngine.Apply(State, new Restart(State.Revision));
            if (!result.Accepted) return false;
            State = result.State; Seed = seed; die = new SeededSessionDie(seed); Started = true;
            Transitioned?.Invoke(result);
            return true;
        }
    }
}
