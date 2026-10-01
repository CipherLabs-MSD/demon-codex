using System.Collections.Generic;
using DemonCodex.LocalMatch;
using DemonCodex.Rules;
using UnityEngine;

namespace DemonCodex.Presentation
{
    public sealed class LocalMatchController : MonoBehaviour
    {
        public float BotDelaySeconds = 0.65f;
        public LocalMatchSession Session { get; private set; }
        public MatchState PresentedState { get; private set; }
        public int? LatestRoll { get; private set; }
        public IReadOnlyList<string> Log => log.AsReadOnly();
        public IReadOnlyList<DomainEvent> LastEvents { get; private set; }
        private readonly List<string> log = new List<string>();
        private float nextBotAt;

#if UNITY_EDITOR
        // Editor tooling only: absent from every player build, including development builds.
        public string DevelopmentScenario { get; private set; }
        // Bots stay paused after the scenario move, so the view offers the normal PLAY AGAIN exit.
        public bool DevelopmentScenarioAwaitingExit => DevelopmentScenario != null && !Session.IsHumanTurn;
        public void LoadDevelopmentSession(LocalMatchSession session, string instruction)
        {
            Invariants.AssertState(session.State);
            if (!session.IsHumanTurn || session.State.Phase != MatchPhase.AwaitingSelection)
                throw new System.ArgumentException("Scenario must stop at a legal human selection.");
            Session.Transitioned -= Present;
            Session = session;
            Session.Transitioned += Present;
            PresentedState = Session.State;
            LatestRoll = Session.State.PendingRoll;
            LastEvents = new DomainEvent[0]; log.Clear();
            DevelopmentScenario = instruction;
            // Pause only bot scheduling; UI still uses normal Roll/Select/PlayAgain.
            enabled = false;
        }
        private void ClearDevelopmentScenario()
        {
            if (DevelopmentScenario != null) enabled = true;
            DevelopmentScenario = null;
        }
#endif

        private void Awake() { Initialize(2026); }
        public void Initialize(uint seed, IDieSource die = null)
        {
#if UNITY_EDITOR
            ClearDevelopmentScenario();
#endif
            if (Session != null) Session.Transitioned -= Present;
            Session = new LocalMatchSession(seed, die);
            Session.Transitioned += Present;
            PresentedState = Session.State; LatestRoll = null; log.Clear();
            LastEvents = new DomainEvent[0]; nextBotAt = Time.unscaledTime + BotDelaySeconds;
        }
        public void StartMatch(uint seed) { Initialize(seed); Session.Start(); }
        public bool Roll() => Session.RollHuman();
        public bool Select(LegalMove move) => Session.SelectHuman(move);
        public bool PlayAgain(uint seed) => Session.PlayAgain(seed);
        private void Update()
        {
            if (Time.unscaledTime < nextBotAt) return;
            if (Session.StepBot()) nextBotAt = Time.unscaledTime + BotDelaySeconds;
        }
        private void Present(Transition transition)
        {
            // Consume events in reducer order, then snap visuals to its immutable snapshot.
            foreach (var e in transition.Events)
            {
                if (e.Kind == EventKind.MatchRestarted) {
                    log.Clear(); LatestRoll = null;
#if UNITY_EDITOR
                    ClearDevelopmentScenario();
#endif
                }
                if (e.Kind == EventKind.DieRolled) LatestRoll = e.Roll;
                log.Add(Describe(e));
                if (log.Count > 12) log.RemoveAt(0);
            }
            LastEvents = transition.Events;
            PresentedState = transition.State;
            nextBotAt = Time.unscaledTime + BotDelaySeconds;
        }
        private void OnDestroy() { if (Session != null) Session.Transitioned -= Present; }
        public static string Name(PlayerId player) => new[] { "Human", "Bot I", "Bot II", "Bot III" }[(int)player];
        private static string Describe(DomainEvent e)
        {
            string who = Name(e.Player), piece = e.Piece.HasValue ? " #" + (e.Piece.Value.Index + 1) : "";
            switch (e.Kind)
            {
                case EventKind.DieRolled: return who + " rolled " + e.Roll;
                case EventKind.NoLegalMoves: return who + ": no legal moves";
                case EventKind.BonusRollGranted: return who + ": six grants another roll";
                case EventKind.TurnAdvanced: return "Turn: " + Name(e.NextPlayer.Value);
                case EventKind.PieceBanished: return who + piece + " banished to the Abyss";
                case EventKind.PieceCompleted: return who + piece + " reached home";
                case EventKind.MatchWon: return who + " wins!";
                case EventKind.MatchRestarted: return "New match started";
                case EventKind.PieceSummoned: return who + piece + " summoned";
                default: return who + piece + " moved";
            }
        }
    }
}
