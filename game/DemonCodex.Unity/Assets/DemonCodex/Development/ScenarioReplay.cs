using System;
using System.Linq;
using DemonCodex.LocalMatch;
using DemonCodex.Rules;

namespace DemonCodex.Development
{
    public enum ScenarioKind { Knockout, Ascension, Completion, Victory }

    public sealed class PreparedScenario
    {
        public ScenarioKind Kind { get; }
        public LocalMatchSession Session { get; }
        public LegalMove IntendedMove { get; }
        public string Instruction => "TEST " + Kind.ToString().ToUpperInvariant() +
            " - click Piece " + (IntendedMove.Piece.Index + 1) +
            " (die " + IntendedMove.Roll + "); bots paused for inspection";
        internal PreparedScenario(ScenarioKind kind, LocalMatchSession session, LegalMove move)
        { Kind = kind; Session = session; IntendedMove = move; }
    }

    // Editor-only assembly. No snapshots fabricated, reflection, fixture constructors,
    // rule overrides or special dice. Fast-forward an ordinary seeded session and
    // stop BEFORE the human's intended legal command. Never called by START MATCH.
    public static class ScenarioReplay
    {
        public static PreparedScenario Prepare(ScenarioKind kind)
        {
            if (!Enum.IsDefined(typeof(ScenarioKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            for (uint seed = 1; seed <= 32; seed++)
            {
                var session = new LocalMatchSession(seed);
                session.Transitioned += t => Invariants.AssertState(t.State);
                session.Start();
                for (int step = 0; step < 20000 && session.State.Phase != MatchPhase.Finished; step++)
                {
                    if (session.IsHumanTurn && session.State.Phase == MatchPhase.AwaitingSelection)
                    {
                        foreach (var move in session.SelectableMoves)
                        {
                            var command = new SelectMove(PlayerId.P0, move, session.State.Revision);
                            var preview = RulesEngine.Apply(session.State, command);
                            Invariants.AssertTransition(session.State, command, preview);
                            if (Matches(kind, move, preview))
                                return new PreparedScenario(kind, session, move);
                        }
                    }
                    bool accepted = !session.IsHumanTurn ? session.StepBot() :
                        session.State.Phase == MatchPhase.AwaitingRoll ? session.RollHuman() :
                        session.SelectHuman(BotPolicy.Choose(session.State, session.LegalMoves));
                    if (!accepted) throw new InvalidOperationException("Scenario replay issued a rejected command.");
                }
            }
            throw new InvalidOperationException("Scenario replay watchdog: no matching human move found.");
        }
        public static bool Matches(ScenarioKind kind, LegalMove move, Transition transition)
        {
            if (!transition.Accepted) return false;
            switch (kind)
            {
                case ScenarioKind.Knockout: return transition.Events.Any(e => e.Kind == EventKind.PieceBanished);
                case ScenarioKind.Ascension: return move.Source.Kind == PieceKind.OnMainTrack &&
                    move.Target.Kind == PieceKind.OnAscensionPath;
                case ScenarioKind.Completion: return transition.Events.Any(e => e.Kind == EventKind.PieceCompleted) &&
                    !transition.State.Winner.HasValue;
                case ScenarioKind.Victory: return transition.State.Winner == PlayerId.P0 &&
                    transition.Events.Any(e => e.Kind == EventKind.MatchWon);
                default: return false;
            }
        }
    }
}
