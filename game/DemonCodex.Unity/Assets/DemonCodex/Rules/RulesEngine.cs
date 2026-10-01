using System;
using System.Collections.Generic;
using System.Linq;

namespace DemonCodex.Rules
{
    public static class RulesEngine
    {
        // Pure query. Invalid requests are programming/input errors, never an empty
        // legal set (which has a different gameplay meaning).
        public static IReadOnlyList<LegalMove> LegalMoves(MatchState state, PlayerId player, int roll)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (roll < 1 || roll > 6) throw new ArgumentOutOfRangeException(nameof(roll));
            if (player != state.ActivePlayer) throw new ArgumentException("Wrong active player.", nameof(player));
            if (state.Phase == MatchPhase.Finished ||
                (state.Phase == MatchPhase.AwaitingSelection && state.PendingRoll != roll))
                throw new InvalidOperationException("Roll query is incompatible with match phase/pending roll.");
            var moves = new List<LegalMove>();
            foreach (var piece in state.Pieces.Where(p => p.Owner == player).OrderBy(p => p.Id.Index))
            {
                PiecePosition? target = Destination(state.Board, piece, roll);
                if (!target.HasValue) continue;
                var occupant = Occupant(state, player, target.Value);
                if (occupant != null && (occupant.Owner == player || state.Board.IsSafe(
                    state.Board.TrackIndex(player, target.Value.Step)))) continue;
                moves.Add(new LegalMove(piece.Id, piece.Position.Kind == PieceKind.InAbyss ? MoveKind.Summon : MoveKind.Advance,
                    piece.Position, target.Value, roll, state.Revision));
            }
            return moves.AsReadOnly();
        }

        private static PiecePosition? Destination(BoardConfiguration board, PieceState piece, int roll)
        {
            var position = piece.Position;
            if (position.Kind == PieceKind.Completed) return null;
            if (position.Kind == PieceKind.InAbyss) return roll == 6 ? PiecePosition.Track(0) : (PiecePosition?)null;
            // Widen before arithmetic: even extreme valid synthetic configurations
            // must not overflow at a route boundary or modulo operation.
            long finish = (long)board.MainTrackLength + board.FinalPathLength[piece.Owner] - 1;
            long current = position.Kind == PieceKind.OnMainTrack ? position.Step :
                (long)board.MainTrackLength + position.Step - 1;
            long next = current + roll;
            if (next > finish) return null;
            if (next == finish) return PiecePosition.Completed;
            if (next < board.MainTrackLength) return PiecePosition.Track((int)next);
            return PiecePosition.Path((int)(next - board.MainTrackLength + 1));
        }

        internal static PieceState Occupant(MatchState state, PlayerId owner, PiecePosition target)
        {
            if (target.Kind == PieceKind.OnMainTrack)
            {
                int index = state.Board.TrackIndex(owner, target.Step);
                return state.Pieces.SingleOrDefault(p => p.Position.Kind == PieceKind.OnMainTrack &&
                    state.Board.TrackIndex(p.Owner, p.Position.Step) == index);
            }
            if (target.Kind == PieceKind.OnAscensionPath)
                return state.Pieces.SingleOrDefault(p => p.Owner == owner && p.Position.Equals(target));
            return null;
        }

        public static Transition Apply(MatchState state, Command command)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (command == null) return Transition.Reject(state, Rejection.InvalidCommand);
            if (command.ExpectedRevision != state.Revision) return Transition.Reject(state, Rejection.StaleRevision);
            if (state.Revision == long.MaxValue) return Transition.Reject(state, Rejection.RevisionExhausted);
            if (command is Restart)
                return new Transition(MatchState.Initial(state.Board, state.Revision + 1),
                    new[] { new DomainEvent(EventKind.MatchRestarted, state.Board.InitialPlayer) });
            if (command is ProvideDieRoll roll) return Roll(state, roll);
            if (command is SelectMove selection) return Move(state, selection);
            return Transition.Reject(state, Rejection.InvalidCommand);
        }

        private static Transition Roll(MatchState state, ProvideDieRoll command)
        {
            if (command.Player != state.ActivePlayer) return Transition.Reject(state, Rejection.WrongPlayer);
            if (state.Phase != MatchPhase.AwaitingRoll) return Transition.Reject(state, Rejection.WrongPhase);
            if (command.Roll < 1 || command.Roll > 6) return Transition.Reject(state, Rejection.InvalidRoll);
            var events = new List<DomainEvent> { new DomainEvent(EventKind.DieRolled, command.Player, roll: command.Roll) };
            if (LegalMoves(state, command.Player, command.Roll).Count != 0)
                return new Transition(new MatchState(state.Board, state.Pieces, MatchPhase.AwaitingSelection,
                    state.ActivePlayer, command.Roll, null, state.Revision + 1), events);
            events.Add(new DomainEvent(EventKind.NoLegalMoves, command.Player, roll: command.Roll));
            return EndRoll(state, state.Pieces, command.Roll, events);
        }

        private static Transition Move(MatchState state, SelectMove command)
        {
            if (command.Player != state.ActivePlayer) return Transition.Reject(state, Rejection.WrongPlayer);
            if (state.Phase != MatchPhase.AwaitingSelection) return Transition.Reject(state, Rejection.WrongPhase);
            if (command.Move == null) return Transition.Reject(state, Rejection.IllegalMove);
            if (command.Move.Revision != state.Revision) return Transition.Reject(state, Rejection.StaleRevision);
            if (command.Move.Roll != state.PendingRoll) return Transition.Reject(state, Rejection.InvalidRoll);
            var move = LegalMoves(state, command.Player, state.PendingRoll.Value).FirstOrDefault(m => m.Equals(command.Move));
            if (move == null) return Transition.Reject(state, Rejection.IllegalMove);
            var pieces = state.Pieces.ToArray();
            var events = new List<DomainEvent>();
            var victim = Occupant(state, command.Player, move.Target);
            if (victim != null)
            {
                Replace(pieces, victim.Id, PiecePosition.Abyss);
                events.Add(new DomainEvent(EventKind.PieceBanished, victim.Owner, victim.Id, victim.Position, PiecePosition.Abyss));
            }
            Replace(pieces, move.Piece, move.Target);
            events.Add(new DomainEvent(move.Kind == MoveKind.Summon ? EventKind.PieceSummoned : EventKind.PieceMoved,
                command.Player, move.Piece, move.Source, move.Target, move.Roll));
            if (move.Target.Kind == PieceKind.Completed)
                events.Add(new DomainEvent(EventKind.PieceCompleted, command.Player, move.Piece, move.Source, move.Target));
            if (pieces.Where(p => p.Owner == command.Player).All(p => p.Position.Kind == PieceKind.Completed))
            {
                events.Add(new DomainEvent(EventKind.MatchWon, command.Player));
                return new Transition(new MatchState(state.Board, pieces, MatchPhase.Finished,
                    command.Player, null, command.Player, state.Revision + 1), events);
            }
            return EndRoll(state, pieces, move.Roll, events);
        }

        private static void Replace(PieceState[] pieces, PieceId id, PiecePosition position)
        {
            int index = Array.FindIndex(pieces, p => p.Id.Equals(id));
            pieces[index] = new PieceState(id, position);
        }

        private static Transition EndRoll(MatchState before, IEnumerable<PieceState> pieces, int roll, List<DomainEvent> events)
        {
            var next = roll == 6 ? before.ActivePlayer : before.Board.NextPlayer(before.ActivePlayer);
            events.Add(roll == 6 ? new DomainEvent(EventKind.BonusRollGranted, before.ActivePlayer, roll: roll) :
                new DomainEvent(EventKind.TurnAdvanced, before.ActivePlayer, nextPlayer: next));
            return new Transition(new MatchState(before.Board, pieces, MatchPhase.AwaitingRoll, next, null, null, before.Revision + 1), events);
        }
    }
}
