using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DemonCodex.Rules
{
    // Diagnostics are executable contracts, not game outcomes. Hosts/tests decide
    // whether to throw/log. No state changes, RNG, time or external services here.
    public static class Invariants
    {
        public static IReadOnlyList<string> State(MatchState state)
        {
            var errors = new List<string>();
            if (state == null) { errors.Add("I02: null snapshot"); return errors; }
            var ids = (PlayerId[])Enum.GetValues(typeof(PlayerId));
            Check(errors, state.Players.Count == 4 && state.Players.Select(p => p.Id).OrderBy(x => x).SequenceEqual(ids)
                && state.Players.Count(p => p.Control == ControlType.Human) == 1
                && state.Players.Count(p => p.Control == ControlType.Bot) == 3, "I01: four players, one human/three bots required");
            Check(errors, state.Pieces.Count == 16 && state.Pieces.All(p => p != null)
                && state.Pieces.Select(p => p.Id).Distinct().Count() == 16
                && ids.All(id => state.Pieces.Count(p => p.Owner == id) == 4), "I01: 16 unique pieces, four per owner required");
            if (errors.Count > 0) return errors.AsReadOnly();
            foreach (var piece in state.Pieces)
            {
                var pos = piece.Position;
                bool valid = (pos.Kind == PieceKind.InAbyss || pos.Kind == PieceKind.Completed) ? pos.Step == 0 :
                    pos.Kind == PieceKind.OnMainTrack ? pos.Step >= 0 && pos.Step < state.Board.MainTrackLength :
                    pos.Kind == PieceKind.OnAscensionPath && pos.Step >= 1 && pos.Step < state.Board.FinalPathLength[piece.Owner];
                Check(errors, valid, "I02/I04: invalid owner-qualified position for " + piece.Id);
            }
            var occupied = state.Pieces.Where(p => p.Position.Kind == PieceKind.OnMainTrack || p.Position.Kind == PieceKind.OnAscensionPath)
                .Select(p => p.Position.Kind == PieceKind.OnMainTrack ? "track:" + state.Board.TrackIndex(p.Owner, p.Position.Step) :
                    "path:" + p.Owner + ":" + p.Position.Step).ToArray();
            Check(errors, occupied.Distinct().Count() == occupied.Length, "I05: multiple pieces occupy a square");
            Check(errors, ids.Contains(state.ActivePlayer) && state.Revision >= 0 && Enum.IsDefined(typeof(MatchPhase), state.Phase),
                "I02: invalid active player, revision or phase");
            var winners = ids.Where(id => state.Pieces.Where(p => p.Owner == id).All(p => p.Position.Kind == PieceKind.Completed)).ToArray();
            Check(errors, state.Phase == MatchPhase.Finished ? state.Winner.HasValue && winners.Length == 1 &&
                winners[0] == state.Winner && state.ActivePlayer == state.Winner : !state.Winner.HasValue && winners.Length == 0,
                "I08: winner/Finished must agree with exactly four completed pieces");
            Check(errors, state.Phase == MatchPhase.AwaitingSelection ? state.PendingRoll >= 1 && state.PendingRoll <= 6 :
                !state.PendingRoll.HasValue, "I09: pending roll must match phase");
            if (errors.Count == 0 && state.Phase == MatchPhase.AwaitingSelection)
                Check(errors, RulesEngine.LegalMoves(state, state.ActivePlayer, state.PendingRoll.Value).Count > 0,
                    "I09: selection phase needs a legal move");
            return errors.AsReadOnly();
        }

        public static IReadOnlyList<string> Transition(MatchState before, Command command, Transition result)
        {
            var errors = State(before).ToList();
            errors.AddRange(State(result.State));
            if (errors.Count > 0) return errors.AsReadOnly();
            var after = result.State;
            // Deterministic replay is evaluated on every transition, including rejects.
            var replay = RulesEngine.Apply(before, command);
            Check(errors, Fingerprint(replay.State) == Fingerprint(after) && replay.Error == result.Error &&
                EventFingerprint(replay.Events) == EventFingerprint(result.Events), "I12: identical input produced different output/events");
            if (!result.Accepted)
            {
                Check(errors, ReferenceEquals(before, after) && Fingerprint(before) == Fingerprint(after) && result.Events.Count == 0,
                    "I10: rejected action mutated state or emitted events");
                return errors.AsReadOnly();
            }
            Check(errors, ReferenceEquals(before.Board, after.Board), "I02: configuration changed");
            Check(errors, before.Pieces.Select(p => p.Id).SequenceEqual(after.Pieces.Select(p => p.Id)), "I01/I04: identity/ownership changed");
            Check(errors, after.Revision == before.Revision + 1, "I12: accepted command must advance revision");
            if (command is Restart)
            {
                Check(errors, after.Pieces.All(p => p.Position.Equals(PiecePosition.Abyss)) && after.Phase == MatchPhase.AwaitingRoll
                    && after.ActivePlayer == before.Board.InitialPlayer && !after.Winner.HasValue && !after.PendingRoll.HasValue,
                    "I12: restart did not restore initial gameplay values");
                Check(errors, result.Events.Count == 1 && result.Events[0].Kind == EventKind.MatchRestarted, "I12: restart events invalid");
                return errors.AsReadOnly();
            }
            Check(errors, before.Phase != MatchPhase.Finished, "I08: normal command accepted after victory");
            foreach (var p in before.Pieces.Where(p => p.Position.Kind == PieceKind.Completed))
                Check(errors, after.Piece(p.Id).Position.Equals(p.Position), "I03: completed piece moved: " + p.Id);
            var changes = before.Pieces.Where(p => !after.Piece(p.Id).Position.Equals(p.Position)).ToArray();
            int roll;
            if (command is ProvideDieRoll supplied)
            {
                roll = supplied.Roll;
                Check(errors, changes.Length == 0, "I07/I11: supplying roll moved a piece");
                Check(errors, result.Events[0].Kind == EventKind.DieRolled && result.Events[0].Roll == roll, "I11: roll must be first event");
                bool noMoves = RulesEngine.LegalMoves(before, before.ActivePlayer, roll).Count == 0;
                if (!noMoves)
                {
                    Check(errors, after.Phase == MatchPhase.AwaitingSelection && after.PendingRoll == roll &&
                        after.ActivePlayer == before.ActivePlayer && result.Events.Count == 1, "I09/I11: roll selection transition invalid");
                    return errors.AsReadOnly();
                }
                Check(errors, result.Events.Count == 3 && result.Events[1].Kind == EventKind.NoLegalMoves,
                    "I11: no-move roll event order invalid");
            }
            else if (command is SelectMove selection)
            {
                roll = before.PendingRoll.Value;
                var move = selection.Move;
                var moving = before.Piece(move.Piece);
                var destination = after.Piece(move.Piece).Position;
                Check(errors, changes.Length >= 1 && changes.Length <= 2 && moving.Owner == before.ActivePlayer,
                    "I07/I11: only selected piece and at most one victim may change");
                Check(errors, destination.Equals(move.Target) && move.Source.Equals(moving.Position), "I07: submitted endpoints differ from state");
                if (moving.Position.Kind == PieceKind.InAbyss)
                    Check(errors, roll == 6 && destination.Equals(PiecePosition.Track(0)), "I07: invalid summon");
                else
                    Check(errors, RouteDistance(after.Board, moving.Owner, destination) - RouteDistance(before.Board, moving.Owner, moving.Position) == roll,
                        "I07: movement did not consume exact roll");
                var victims = changes.Where(p => !p.Id.Equals(move.Piece)).ToArray();
                foreach (var victim in victims)
                    Check(errors, victim.Owner != moving.Owner && victim.Position.Kind == PieceKind.OnMainTrack &&
                        destination.Kind == PieceKind.OnMainTrack &&
                        before.Board.TrackIndex(victim.Owner, victim.Position.Step) == before.Board.TrackIndex(moving.Owner, destination.Step) &&
                        !before.Board.IsSafe(before.Board.TrackIndex(victim.Owner, victim.Position.Step)) &&
                        after.Piece(victim.Id).Position.Equals(PiecePosition.Abyss), "I06: unsafe banishment or retained victim progress");
                var expected = new List<EventKind>();
                if (victims.Length > 0) expected.Add(EventKind.PieceBanished);
                expected.Add(moving.Position.Kind == PieceKind.InAbyss ? EventKind.PieceSummoned : EventKind.PieceMoved);
                if (destination.Kind == PieceKind.Completed) expected.Add(EventKind.PieceCompleted);
                expected.Add(after.Phase == MatchPhase.Finished ? EventKind.MatchWon : roll == 6 ? EventKind.BonusRollGranted : EventKind.TurnAdvanced);
                Check(errors, expected.SequenceEqual(result.Events.Select(e => e.Kind)), "I06/I07/I08/I11: move events missing or out of order");
                foreach (var e in result.Events.Where(e => e.Piece.HasValue))
                    Check(errors, e.From == null || e.From.Value.Equals(before.Piece(e.Piece.Value).Position), "I07: event source mismatch");
                foreach (var e in result.Events.Where(e => e.Piece.HasValue))
                    Check(errors, e.To == null || e.To.Value.Equals(after.Piece(e.Piece.Value).Position), "I07: event destination mismatch");
            }
            else { errors.Add("I11: unknown accepted command"); return errors.AsReadOnly(); }
            if (after.Phase == MatchPhase.Finished)
                Check(errors, after.Winner == before.ActivePlayer && result.Events.Last().Kind == EventKind.MatchWon &&
                    !result.Events.Any(e => e.Kind == EventKind.BonusRollGranted || e.Kind == EventKind.TurnAdvanced), "I08/I11: victory must override bonus/advance");
            else
            {
                var expected = roll == 6 ? before.ActivePlayer : before.Board.NextPlayer(before.ActivePlayer);
                Check(errors, after.ActivePlayer == expected && after.Phase == MatchPhase.AwaitingRoll && !after.PendingRoll.HasValue,
                    "I11: wrong next player/phase after resolution");
                var last = result.Events.Last();
                Check(errors, last.Player == before.ActivePlayer && (roll == 6 ? last.Kind == EventKind.BonusRollGranted :
                    last.Kind == EventKind.TurnAdvanced && last.NextPlayer == expected), "I11: incorrect bonus/advance event");
            }
            return errors.AsReadOnly();
        }

        private static long RouteDistance(BoardConfiguration board, PlayerId owner, PiecePosition position)
        {
            if (position.Kind == PieceKind.OnMainTrack) return position.Step;
            if (position.Kind == PieceKind.OnAscensionPath) return (long)board.MainTrackLength + position.Step - 1;
            if (position.Kind == PieceKind.Completed) return (long)board.MainTrackLength + board.FinalPathLength[owner] - 1;
            return -1;
        }
        private static void Check(List<string> errors, bool valid, string message) { if (!valid) errors.Add(message); }
        public static void AssertState(MatchState state)
        {
            var errors = State(state);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("; ", errors));
        }
        public static void AssertTransition(MatchState before, Command command, Transition result)
        {
            var errors = Transition(before, command, result);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("; ", errors));
        }
        // Stable diagnostic representation, not a persisted save-game contract.
        public static string Fingerprint(MatchState s) => string.Join("|", new[] {
            s.Board.MainTrackLength.ToString(CultureInfo.InvariantCulture),
            string.Join(",", s.Board.PlayerOrder), s.Board.InitialPlayer.ToString(),
            string.Join(",", s.Board.Players.Select(p => p.Id + ":" + p.Control + ":" + s.Board.StartIndex[p.Id] + ":" + s.Board.FinalPathLength[p.Id])),
            string.Join(",", s.Board.SafeSpaceIndices), s.Phase.ToString(), s.ActivePlayer.ToString(),
            s.PendingRoll?.ToString(CultureInfo.InvariantCulture) ?? "-", s.Winner?.ToString() ?? "-",
            s.Revision.ToString(CultureInfo.InvariantCulture), string.Join(",", s.Pieces.Select(p => p.Id + "=" + p.Position)) });
        public static string EventFingerprint(IEnumerable<DomainEvent> events) => string.Join("|", events.Select(e =>
            e.Kind + ":" + e.Player + ":" + e.Piece + ":" + e.From + ":" + e.To + ":" + e.Roll + ":" + e.NextPlayer));
    }
}
