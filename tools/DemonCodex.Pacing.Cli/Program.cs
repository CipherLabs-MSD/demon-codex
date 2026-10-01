using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using DemonCodex.LocalMatch;
using DemonCodex.Rules;

// DC-0006 pacing evidence for the M001 PROTOTYPE CONFIGURATION. Drives the shipped
// LocalMatchSession and BotPolicy; the human seat (P0) uses BotPolicy as a proxy for
// real choices. Invariants are checked after every transition. Read-only analysis:
// no rule, session or presentation behavior changes.
// Usage: [count] [first-seed] [output-dir] [max-rolls] [bot-delay-seconds]
if (args.Length > 5) throw new ArgumentException("Usage: [count] [first-seed] [output-dir] [max-rolls] [bot-delay-seconds]");
int count = args.Length > 0 ? int.Parse(args[0], CultureInfo.InvariantCulture) : 1000;
uint start = args.Length > 1 ? uint.Parse(args[1], CultureInfo.InvariantCulture) : 1;
string output = args.Length > 2 ? args[2] : "artifacts/pacing";
int maxRolls = args.Length > 3 ? int.Parse(args[3], CultureInfo.InvariantCulture) : 10000;
// Default mirrors LocalMatchController.BotDelaySeconds; each bot command waits this long.
double botDelay = args.Length > 4 ? double.Parse(args[4], CultureInfo.InvariantCulture) : 0.65;
if (count < 1 || (ulong)start + (ulong)count - 1 > uint.MaxValue || maxRolls < 1 || botDelay < 0)
    throw new ArgumentOutOfRangeException(nameof(args));
// ASSUMPTION, not measured: seconds a person spends per ROLL or piece click.
double[] humanSeconds = { 1.5, 3.0 };

Directory.CreateDirectory(output);
var matches = Enumerable.Range(0, count).Select(i => Pacing.Run(start + (uint)i, maxRolls)).ToList();
using (var csv = new StreamWriter(Path.Combine(output, "matches.csv")))
{
    csv.NewLine = "\n";
    csv.WriteLine("seed,winner,invariant_failure,watchdog,commands,bot_commands,human_commands,rolls,human_rolls," +
        "turns,human_turns,sixes,human_sixes,no_move_rolls,human_no_move_rolls,human_selections," +
        "human_choice_selections,human_forced_selections,knockouts,knockouts_by_human,human_pieces_banished," +
        "summons,human_first_summon_turn,first_completion_turn,human_completed,human_completion_turns");
    foreach (var m in matches)
        csv.WriteLine(string.Join(",", m.Seed, m.Winner, m.Failure ? 1 : 0, m.Watchdog ? 1 : 0, m.Commands,
            m.BotCommands, m.HumanCommands, m.Rolls, m.HumanRolls, m.Turns, m.HumanTurns, m.Sixes, m.HumanSixes,
            m.NoMoveRolls, m.HumanNoMoveRolls, m.HumanSelections, m.HumanChoices, m.HumanForced, m.Knockouts,
            m.KnockoutsByHuman, m.HumanBanished, m.Summons, m.HumanFirstSummonTurn, m.FirstCompletionTurn,
            m.HumanCompleted, string.Join(";", m.HumanCompletionTurns)));
}

var done = matches.Where(m => !m.Failure && !m.Watchdog).ToList();
var summoned = done.Where(m => m.HumanFirstSummonTurn >= 0).ToList();
object Dist(Func<MatchStats, double> f) => Stats.Describe(done.Select(f));
double Minutes(MatchStats m, double h) => (m.BotCommands * botDelay + m.HumanCommands * h) / 60;
var defaultSeed = Pacing.Run(2026, maxRolls);
var summary = new
{
    configuration = "M001 PROTOTYPE CONFIGURATION (PrototypeBoard): L=40, starts 0/10/20/30, safe every 5, F=6, P0 human seat",
    policy = "BotPolicy for all four seats; P0 is a proxy for human choices",
    rng = "SeededSessionDie (xorshift32, rejection-sampled 1d6), same as the Unity prototype",
    matches = count, firstSeed = start, lastSeed = start + (uint)count - 1, maxRolls,
    completed = done.Count,
    invariantFailures = matches.Count(m => m.Failure), failureSeeds = matches.Where(m => m.Failure).Select(m => m.Seed),
    watchdogCases = matches.Count(m => m.Watchdog), watchdogSeeds = matches.Where(m => m.Watchdog).Select(m => m.Seed),
    winsBySeat = done.GroupBy(m => m.Winner).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.Count()),
    perMatch = new
    {
        rolls = Dist(m => m.Rolls), turns = Dist(m => m.Turns), rounds = Dist(m => m.Turns / 4.0),
        commands = Dist(m => m.Commands), botCommands = Dist(m => m.BotCommands),
        humanRolls = Dist(m => m.HumanRolls), humanCommands = Dist(m => m.HumanCommands),
        humanChoiceSelections = Dist(m => m.HumanChoices), humanForcedSelections = Dist(m => m.HumanForced),
        humanNoMoveRolls = Dist(m => m.HumanNoMoveRolls), sixes = Dist(m => m.Sixes),
        knockouts = Dist(m => m.Knockouts), knockoutsByHuman = Dist(m => m.KnockoutsByHuman),
        humanPiecesBanished = Dist(m => m.HumanBanished),
        matchesWhereHumanNeverSummoned = done.Count - summoned.Count,
        humanTurnOfFirstSummon = Stats.Describe(summoned.Select(m => (double)m.HumanFirstSummonTurn)),
        roundOfFirstCompletionAnySeat = Dist(m => m.FirstCompletionTurn / 4.0),
        humanPiecesCompletedAtEnd = Dist(m => m.HumanCompleted),
    },
    ratios = new
    {
        humanRollsWithNoLegalMove = Stats.Ratio(done.Sum(m => m.HumanNoMoveRolls), done.Sum(m => m.HumanRolls)),
        humanSelectionsWithRealChoice = Stats.Ratio(done.Sum(m => m.HumanChoices), done.Sum(m => m.HumanSelections)),
        humanCommandShareOfAllCommands = Stats.Ratio(done.Sum(m => m.HumanCommands), done.Sum(m => m.Commands)),
    },
    humanCompletionRound = Enumerable.Range(0, 4).Select(k => new
    {
        piece = k + 1,
        matchesReached = done.Count(m => m.HumanCompletionTurns.Count > k),
        round = Stats.Describe(done.Where(m => m.HumanCompletionTurns.Count > k).Select(m => m.HumanCompletionTurns[k] / 4.0)),
    }),
    timing = new
    {
        botDelaySeconds = botDelay,
        botWaitMinutes = Dist(m => m.BotCommands * botDelay / 60),
        assumption = "Estimated minutes = bot commands x delay + human commands x assumed seconds per human action; human seconds are NOT measured",
        estimatedMinutes = humanSeconds.ToDictionary(h => h.ToString("0.0", CultureInfo.InvariantCulture) + "sPerHumanAction",
            h => Dist(m => Minutes(m, h))),
        estimatedMinutesBeforeHumanFirstSummon = humanSeconds.ToDictionary(h => h.ToString("0.0", CultureInfo.InvariantCulture) + "sPerHumanAction",
            h => Stats.Describe(summoned.Select(m => (m.BotCommandsBeforeFirstSummon * botDelay + m.HumanCommandsBeforeFirstSummon * h) / 60))),
    },
    defaultUiSeed2026 = new
    {
        note = "Seed 2026 is the prototype default; real human choices diverge from this proxy after the first different selection",
        defaultSeed.Winner, defaultSeed.Rolls, defaultSeed.Turns, defaultSeed.HumanCommands, defaultSeed.BotCommands,
        estimatedMinutes = humanSeconds.ToDictionary(h => h.ToString("0.0", CultureInfo.InvariantCulture) + "sPerHumanAction",
            h => Math.Round(Minutes(defaultSeed, h), 2)),
    },
};
string json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(Path.Combine(output, "summary.json"), json.Replace("\r\n", "\n") + "\n");
Console.WriteLine(json);
return matches.Any(m => m.Failure || m.Watchdog) || defaultSeed.Failure || defaultSeed.Watchdog ? 1 : 0;

sealed class MatchStats
{
    public uint Seed; public string Winner = ""; public bool Failure, Watchdog;
    public int Commands, BotCommands, HumanCommands, Rolls, HumanRolls, Turns = 1, HumanTurns, Sixes, HumanSixes;
    public int NoMoveRolls, HumanNoMoveRolls, HumanSelections, HumanChoices, HumanForced;
    public int Knockouts, KnockoutsByHuman, HumanBanished, Summons, HumanCompleted;
    public int HumanFirstSummonTurn = -1, FirstCompletionTurn = -1;
    public int BotCommandsBeforeFirstSummon, HumanCommandsBeforeFirstSummon;
    public List<int> HumanCompletionTurns = new List<int>();
}

static class Pacing
{
    public static MatchStats Run(uint seed, int maxRolls)
    {
        var m = new MatchStats { Seed = seed };
        var session = new LocalMatchSession(seed);
        PlayerId actor = PlayerId.P0;
        session.Transitioned += t => Record(m, actor, t);
        if (session.State.ActivePlayer == PlayerId.P0) m.HumanTurns = 1;
        session.Start();
        try
        {
            while (session.State.Phase != MatchPhase.Finished)
            {
                if (m.Rolls >= maxRolls) { m.Watchdog = true; break; }
                actor = session.State.ActivePlayer;
                bool accepted;
                if (!session.IsHumanTurn) accepted = session.StepBot();
                else if (session.State.Phase == MatchPhase.AwaitingRoll) accepted = session.RollHuman();
                else
                {
                    var legal = session.LegalMoves;
                    m.HumanSelections++;
                    if (legal.Count > 1) m.HumanChoices++; else m.HumanForced++;
                    accepted = session.SelectHuman(BotPolicy.Choose(session.State, legal));
                }
                if (!accepted) throw new InvalidOperationException("Rejected command at revision " + session.State.Revision);
            }
        }
        catch (Exception) { m.Failure = true; }
        return m;
    }

    private static void Record(MatchStats m, PlayerId actor, Transition t)
    {
        Invariants.AssertState(t.State);
        bool human = actor == PlayerId.P0;
        m.Commands++;
        if (human) m.HumanCommands++; else m.BotCommands++;
        foreach (var e in t.Events)
        {
            switch (e.Kind)
            {
                case EventKind.DieRolled:
                    m.Rolls++; if (human) m.HumanRolls++;
                    if (e.Roll == 6) { m.Sixes++; if (human) m.HumanSixes++; }
                    break;
                case EventKind.NoLegalMoves: m.NoMoveRolls++; if (human) m.HumanNoMoveRolls++; break;
                case EventKind.PieceBanished:
                    m.Knockouts++; if (human) m.KnockoutsByHuman++;
                    if (e.Piece.Value.Owner == PlayerId.P0) m.HumanBanished++;
                    break;
                case EventKind.PieceSummoned:
                    m.Summons++;
                    if (human && m.HumanFirstSummonTurn < 0)
                    {
                        m.HumanFirstSummonTurn = m.HumanTurns;
                        m.BotCommandsBeforeFirstSummon = m.BotCommands; m.HumanCommandsBeforeFirstSummon = m.HumanCommands;
                    }
                    break;
                case EventKind.PieceCompleted:
                    if (m.FirstCompletionTurn < 0) m.FirstCompletionTurn = m.Turns;
                    if (e.Piece.Value.Owner == PlayerId.P0) { m.HumanCompleted++; m.HumanCompletionTurns.Add(m.Turns); }
                    break;
                case EventKind.TurnAdvanced:
                    m.Turns++; if (e.NextPlayer == PlayerId.P0) m.HumanTurns++;
                    break;
                case EventKind.MatchWon: m.Winner = e.Player.ToString(); break;
            }
        }
    }
}

static class Stats
{
    public static object Describe(IEnumerable<double> values)
    {
        var v = values.OrderBy(x => x).ToArray();
        if (v.Length == 0) return null;
        double At(double q) => v[Math.Max(0, (int)Math.Ceiling(q * v.Length) - 1)]; // nearest rank
        double R(double x) => Math.Round(x, 2);
        return new { mean = R(v.Average()), p10 = R(At(.1)), median = R(At(.5)), p90 = R(At(.9)), min = R(v[0]), max = R(v[^1]) };
    }
    public static double Ratio(long part, long whole) => whole == 0 ? 0 : Math.Round((double)part / whole, 4);
}
