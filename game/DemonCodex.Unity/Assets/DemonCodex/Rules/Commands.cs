using System;
using System.Collections.Generic;
using System.Linq;

namespace DemonCodex.Rules
{
    public abstract class Command
    {
        public long ExpectedRevision { get; }
        protected Command(long expectedRevision) { ExpectedRevision = expectedRevision; }
    }
    public sealed class ProvideDieRoll : Command
    {
        public PlayerId Player { get; }
        public int Roll { get; }
        public ProvideDieRoll(PlayerId player, int roll, long expectedRevision) : base(expectedRevision)
        { Player = player; Roll = roll; }
    }
    public sealed class SelectMove : Command
    {
        public PlayerId Player { get; }
        public LegalMove Move { get; }
        public SelectMove(PlayerId player, LegalMove move, long expectedRevision) : base(expectedRevision)
        { Player = player; Move = move; }
    }
    public sealed class Restart : Command
    {
        public Restart(long expectedRevision) : base(expectedRevision) { }
    }

    public enum EventKind
    {
        DieRolled, NoLegalMoves, PieceSummoned, PieceMoved, PieceBanished,
        PieceCompleted, BonusRollGranted, TurnAdvanced, MatchWon, MatchRestarted
    }

    public sealed class DomainEvent
    {
        public EventKind Kind { get; }
        public PlayerId Player { get; }
        public PieceId? Piece { get; }
        public PiecePosition? From { get; }
        public PiecePosition? To { get; }
        public int? Roll { get; }
        public PlayerId? NextPlayer { get; }
        public DomainEvent(EventKind kind, PlayerId player, PieceId? piece = null,
            PiecePosition? from = null, PiecePosition? to = null, int? roll = null, PlayerId? nextPlayer = null)
        { Kind = kind; Player = player; Piece = piece; From = from; To = to; Roll = roll; NextPlayer = nextPlayer; }
    }

    public enum Rejection { None, InvalidCommand, StaleRevision, WrongPlayer, WrongPhase, InvalidRoll, IllegalMove, RevisionExhausted }

    public sealed class Transition
    {
        public MatchState State { get; }
        public IReadOnlyList<DomainEvent> Events { get; }
        public Rejection Error { get; }
        public bool Accepted => Error == Rejection.None;
        internal Transition(MatchState state, IEnumerable<DomainEvent> events, Rejection error = Rejection.None)
        { State = state; Events = Array.AsReadOnly(events.ToArray()); Error = error; }
        internal static Transition Reject(MatchState state, Rejection reason) => new Transition(state, Array.Empty<DomainEvent>(), reason);
    }
}
