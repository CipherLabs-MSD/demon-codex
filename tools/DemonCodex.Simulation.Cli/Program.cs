using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using DemonCodex.Simulation;

// Positional arguments keep reproduction simple: count, first seed, output, max rolls.
if (args.Length > 4) throw new ArgumentException("Usage: [count] [first-seed] [output-dir] [max-rolls]");
int count = args.Length > 0 ? int.Parse(args[0], CultureInfo.InvariantCulture) : 1000;
uint start = args.Length > 1 ? uint.Parse(args[1], CultureInfo.InvariantCulture) : 1;
string output = args.Length > 2 ? args[2] : "artifacts/simulation";
int maxRolls = args.Length > 3 ? int.Parse(args[3], CultureInfo.InvariantCulture) : 10000;
if (count < 1 || (ulong)start + (ulong)count - 1 > uint.MaxValue || maxRolls < 1)
    throw new ArgumentOutOfRangeException(nameof(args));
Directory.CreateDirectory(output);
var failures = new List<uint>();
var watchdogs = new List<uint>();
long rolls = 0, turns = 0, transitions = 0;
using var csv = new StreamWriter(Path.Combine(output, "matches.csv"));
csv.NewLine = "\n";
csv.WriteLine("seed,rolls,turns,transitions,winner,invariant_or_harness_failure,watchdog,trace_sha256");
for (int i = 0; i < count; i++)
{
    uint seed = start + (uint)i;
    var match = MatchSimulator.Run(seed, maxRolls);
    rolls += match.Rolls; turns += match.Turns; transitions += match.Transitions;
    if (match.Failure != null) failures.Add(seed);
    if (match.Watchdog) watchdogs.Add(seed);
    if (match.Failure != null || match.Watchdog)
        File.WriteAllLines(Path.Combine(output, "seed-" + seed + ".trace.txt"), match.Trace);
    csv.WriteLine(string.Join(",", seed, match.Rolls, match.Turns, match.Transitions, match.Winner?.ToString() ?? "",
        match.Failure != null ? 1 : 0, match.Watchdog ? 1 : 0, match.TraceHash));
}
var summary = new { matches = count, firstSeed = start, lastSeed = start + (uint)count - 1,
    maxRolls, completed = count - failures.Count - watchdogs.Count, rolls, turns, transitions,
    invariantOrHarnessFailures = failures.Count, failureSeeds = failures, watchdogCases = watchdogs.Count,
    watchdogSeeds = watchdogs, policy = "completion-first/furthest-progress/piece-index (test only)",
    rng = "xorshift32 with rejection-sampled 1d6", configuration = "SyntheticBoards.ForSeed; three track sizes and per-owner path lengths; not product canon" };
string json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(Path.Combine(output, "summary.json"), json + Environment.NewLine);
Console.WriteLine(json);
return failures.Count == 0 && watchdogs.Count == 0 ? 0 : 1;
