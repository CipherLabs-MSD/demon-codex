using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DemonCodex.Rules;

namespace DemonCodex.Simulation
{
    // TEST HARNESS ONLY. These boards/policies are synthetic, not product defaults.
    public static class SyntheticBoards
    {
        public static BoardConfiguration ForSeed(uint seed)
        {
            int stride = new[] { 3, 5, 7 }[seed % 3];
            var ids = (PlayerId[])Enum.GetValues(typeof(PlayerId));
            return new BoardConfiguration(stride * 4, ids.ToDictionary(p => p, p => (int)p * stride),
                ids.Select(p => (int)p * stride).Concat(new[] { 1 }),
                ids.ToDictionary(p => p, p => 1 + ((int)p + (int)(seed % 6)) % 6),
                new[] { PlayerId.P2, PlayerId.P0, PlayerId.P3, PlayerId.P1 },
                (PlayerId)(seed % 4), PlayerId.P0);
        }
    }

    public sealed class SeededDie
    {
        private uint state;
        public SeededDie(uint seed) { state = seed == 0 ? 0x9e3779b9U : seed; }
        // Explicit xorshift32 algorithm avoids runtime-specific Random behavior.
        private uint Next()
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            return state;
        }
        public int Roll()
        {
            uint value;
            // Generator covers 1..uint.MaxValue. Reject top 3 outcomes so each
            // die face has exactly the same number of accepted source values.
            do { value = Next(); } while (value > 4294967292U);
            return (int)((value - 1) % 6) + 1;
        }
    }

    public static class TestMovePolicy
    {
        // Deterministic completion-first/furthest-progress policy; not final bot AI.
        public static LegalMove Choose(IReadOnlyList<LegalMove> moves)
        {
            if (moves == null || moves.Count == 0) throw new ArgumentException("A nonempty legal set is required.");
            return moves.OrderByDescending(m => m.Target.Kind == PieceKind.Completed ? 3 :
                    m.Target.Kind == PieceKind.OnAscensionPath ? 2 : 1)
                .ThenByDescending(m => m.Target.Step).ThenBy(m => m.Piece.Index).First();
        }
    }

    public sealed class SimulationResult
    {
        public uint Seed { get; internal set; }
        public int Rolls { get; internal set; }
        // A turn is a player's tenure including bonus rolls; MatchWon ends last turn.
        public int Turns { get; internal set; }
        public int Transitions { get; internal set; }
        public PlayerId? Winner { get; internal set; }
        public bool Watchdog { get; internal set; }
        public string Failure { get; internal set; }
        public string TraceHash { get; internal set; }
        public IReadOnlyList<string> Trace { get; internal set; }
    }

    public static class MatchSimulator
    {
        public static SimulationResult Run(uint seed, int maxRolls = 10000)
        {
            if (maxRolls < 1) throw new ArgumentOutOfRangeException(nameof(maxRolls));
            var result = new SimulationResult { Seed = seed };
            var state = MatchState.Create(SyntheticBoards.ForSeed(seed));
            var die = new SeededDie(seed);
            var trace = new List<string> { "seed=" + seed + "; maxRolls=" + maxRolls, Invariants.Fingerprint(state) };
            try
            {
                Invariants.AssertState(state);
                while (state.Phase != MatchPhase.Finished && result.Rolls < maxRolls)
                {
                    int roll = die.Roll();
                    result.Rolls++;
                    state = Step(state, new ProvideDieRoll(state.ActivePlayer, roll, state.Revision), "roll=" + roll, trace, result);
                    if (state.Phase == MatchPhase.AwaitingSelection)
                    {
                        var move = TestMovePolicy.Choose(RulesEngine.LegalMoves(state, state.ActivePlayer, state.PendingRoll.Value));
                        state = Step(state, new SelectMove(state.ActivePlayer, move, state.Revision),
                            "select=" + move.Piece + ";" + move.Source + "->" + move.Target, trace, result);
                    }
                }
                result.Winner = state.Winner;
                result.Watchdog = state.Phase != MatchPhase.Finished;
                if (result.Watchdog) trace.Add("WATCHDOG: no gameplay draw/winner assigned");
            }
            catch (Exception ex)
            {
                result.Failure = ex.ToString();
                trace.Add("FAILURE: " + result.Failure);
                trace.Add("last valid state: " + Invariants.Fingerprint(state));
            }
            result.Trace = trace.AsReadOnly();
            using (var sha = SHA256.Create())
                result.TraceHash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", trace)))).Replace("-", "").ToLowerInvariant();
            return result;
        }

        private static MatchState Step(MatchState before, Command command, string label, List<string> trace, SimulationResult result)
        {
            trace.Add("command@" + before.Revision + ":" + before.ActivePlayer + ":" + label);
            var transition = RulesEngine.Apply(before, command);
            trace.Add(Invariants.EventFingerprint(transition.Events));
            trace.Add(Invariants.Fingerprint(transition.State));
            Invariants.AssertTransition(before, command, transition);
            if (!transition.Accepted) throw new InvalidOperationException("Harness action rejected: " + transition.Error);
            result.Transitions++;
            if (transition.Events.Any(e => e.Kind == EventKind.TurnAdvanced || e.Kind == EventKind.MatchWon)) result.Turns++;
            return transition.State;
        }
    }
}
