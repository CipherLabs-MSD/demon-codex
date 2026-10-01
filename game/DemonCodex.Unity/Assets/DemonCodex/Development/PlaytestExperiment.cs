using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DemonCodex.LocalMatch;
using DemonCodex.Rules;

namespace DemonCodex.Development
{
    public enum PlaytestVariant { ControlA, VariantB }

    // M001.1 experiment contract: docs/product/M001_1_PACING_FUN_EXPERIMENT.md.
    // The variants differ ONLY in bot presentation delay. Board, rules, dice, seeds
    // and bot policy are the unchanged M001 session. Editor-only assembly.
    public static class PlaytestExperiment
    {
        public const string Name = "M001.1";
        public const float ControlBotDelaySeconds = 0.65f; // M001 scene value (LocalMatchController)
        public const float FastBotDelaySeconds = 0.25f;
        // Seeds whose simulated length is closest to the DC-0006 median; use one per A/B pair.
        public static readonly uint[] SuggestedSeeds = { 82, 150, 364 };
        public static readonly string[] Questions = {
            "How fun was the match?",
            "How did the match length feel?",
            "How much meaningful control did you feel you had?",
            "How much of the time felt like waiting?",
            "Would you immediately play another match?",
        };
        public static readonly string[] LengthAnswers = { "too short", "good", "too long" };
        public static readonly string[] PlayAgainAnswers = { "yes", "maybe", "no" };
        public static float BotDelaySeconds(PlaytestVariant variant) =>
            variant == PlaytestVariant.ControlA ? ControlBotDelaySeconds : FastBotDelaySeconds;
        public static string Label(PlaytestVariant variant) =>
            variant == PlaytestVariant.ControlA ? "CONTROL A — ORIGINAL PACING" : "VARIANT B — FAST BOTS";
    }

    // One measured session. Plain public fields so the Editor window can keep it across reloads.
    [Serializable]
    public sealed class PlaytestRecord
    {
        public int schemaVersion = 1;
        public string experiment = PlaytestExperiment.Name;
        public string variant, tester, startUtc, endUtc, winner, abortReason;
        public float botDelaySeconds;
        public long seed;
        public double elapsedSeconds, humanTurnSeconds, botTurnSeconds, abortSeconds = -1;
        public int humanActions, botActions, rolls, humanRolls, turns = 1, sixes, humanNoMoveRolls;
        public int meaningfulChoices, forcedChoices, knockouts, humanKnockoutsSuffered, summons;
        public int ascensionEntries, completedPieces, humanCompletedPieces;
        public double rounds, longestSecondsWithoutMeaningfulChoice, secondsPerMeaningfulChoice = -1, meaningfulChoicesPerMinute;
        public bool naturalVictory, aborted, answered;
        public int fun, control, waiting;
        public string matchLength, playAgain, note;

        public void SetAnswers(int fun, string matchLength, int control, int waiting, string playAgain, string note)
        {
            if (fun < 1 || fun > 5 || control < 1 || control > 5 || waiting < 1 || waiting > 5)
                throw new ArgumentOutOfRangeException(nameof(fun), "Ratings are 1–5.");
            if (!PlaytestExperiment.LengthAnswers.Contains(matchLength) || !PlaytestExperiment.PlayAgainAnswers.Contains(playAgain))
                throw new ArgumentException("Unknown answer option.");
            this.fun = fun; this.matchLength = matchLength; this.control = control; this.waiting = waiting;
            this.playAgain = playAgain; this.note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            answered = true;
        }

        public string ToJson()
        {
            var json = new StringBuilder("{\n");
            void Field(string key, string value, bool last = false) =>
                json.Append("  \"").Append(key).Append("\": ").Append(value).Append(last ? "\n" : ",\n");
            Field("schemaVersion", schemaVersion.ToString(CultureInfo.InvariantCulture));
            Field("experiment", Text(experiment)); Field("variant", Text(variant));
            Field("botDelaySeconds", Number(botDelaySeconds)); Field("tester", Text(tester));
            Field("seed", seed.ToString(CultureInfo.InvariantCulture));
            Field("startUtc", Text(startUtc)); Field("endUtc", Text(endUtc));
            Field("elapsedSeconds", Number(elapsedSeconds)); Field("humanTurnSeconds", Number(humanTurnSeconds));
            Field("botTurnSeconds", Number(botTurnSeconds));
            Field("naturalVictory", Bool(naturalVictory)); Field("winner", Text(winner));
            Field("aborted", Bool(aborted)); Field("abortSeconds", aborted ? Number(abortSeconds) : "null");
            Field("abortReason", Text(abortReason));
            Field("rolls", Int(rolls)); Field("humanRolls", Int(humanRolls)); Field("turns", Int(turns));
            Field("rounds", Number(rounds)); Field("humanActions", Int(humanActions)); Field("botActions", Int(botActions));
            Field("meaningfulChoices", Int(meaningfulChoices)); Field("forcedChoices", Int(forcedChoices));
            Field("humanNoMoveRolls", Int(humanNoMoveRolls)); Field("sixes", Int(sixes));
            Field("knockouts", Int(knockouts)); Field("humanKnockoutsSuffered", Int(humanKnockoutsSuffered));
            Field("summons", Int(summons)); Field("ascensionEntries", Int(ascensionEntries));
            Field("completedPieces", Int(completedPieces)); Field("humanCompletedPieces", Int(humanCompletedPieces));
            Field("longestSecondsWithoutMeaningfulChoice", Number(longestSecondsWithoutMeaningfulChoice));
            Field("secondsPerMeaningfulChoice", secondsPerMeaningfulChoice < 0 ? "null" : Number(secondsPerMeaningfulChoice));
            Field("meaningfulChoicesPerMinute", Number(meaningfulChoicesPerMinute));
            Field("answers", !answered ? "null" : "{ \"fun\": " + Int(fun) + ", \"matchLength\": " + Text(matchLength) +
                ", \"control\": " + Int(control) + ", \"waiting\": " + Int(waiting) + ", \"playAgain\": " + Text(playAgain) +
                ", \"note\": " + Text(note) + " }", last: true);
            return json.Append("}\n").ToString();
        }
        private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);
        private static string Bool(bool value) => value ? "true" : "false";
        private static string Number(double value) => Math.Round(value, 3).ToString("0.###", CultureInfo.InvariantCulture);
        private static string Text(string value)
        {
            if (value == null) return "null";
            var text = new StringBuilder("\"");
            foreach (char c in value)
            {
                if (c == '"' || c == '\\') text.Append('\\').Append(c);
                else if (c == '\n') text.Append("\\n");
                else if (c == '\r') text.Append("\\r");
                else if (c == '\t') text.Append("\\t");
                else if (c < ' ') text.Append("\\u").Append(((int)c).ToString("x4"));
                else text.Append(c);
            }
            return text.Append('"').ToString();
        }
    }

    // Observes an ordinary session through its Transitioned events; never issues commands.
    // Time between transitions is attributed to whoever had to act: the human seat or a bot.
    public sealed class PlaytestRecorder
    {
        private readonly LocalMatchSession session;
        private readonly Func<double> clock;
        private readonly Func<DateTimeOffset> utcNow;
        private readonly PlayerId human;
        private readonly double startedAt;
        private double markAt, lastChoiceAt;
        private MatchState previous;
        public PlaytestRecord Record { get; }
        public bool IsRecording { get; private set; }
        public LocalMatchSession Session => session;

        public PlaytestRecorder(LocalMatchSession session, PlaytestVariant variant, string tester,
            Func<double> clock, Func<DateTimeOffset> utcNow)
        {
            if (session == null || !session.Started || session.State.Phase == MatchPhase.Finished)
                throw new ArgumentException("Record a started, unfinished match.");
            this.session = session; this.clock = clock; this.utcNow = utcNow;
            human = session.State.Players.Single(p => p.Control == ControlType.Human).Id;
            Record = new PlaytestRecord
            {
                variant = variant.ToString(), botDelaySeconds = PlaytestExperiment.BotDelaySeconds(variant),
                tester = tester, seed = session.Seed, startUtc = Iso(utcNow()),
            };
            startedAt = markAt = lastChoiceAt = clock();
            previous = session.State;
            IsRecording = true;
            session.Transitioned += OnTransition;
        }

        public void Abort(string reason)
        {
            if (!IsRecording) return;
            double now = clock();
            Accrue(now);
            Finish(now, reason);
        }

        private bool HumanToAct(MatchState state) => state.Phase != MatchPhase.Finished && state.ActivePlayer == human;

        private void Accrue(double now)
        {
            if (HumanToAct(previous)) Record.humanTurnSeconds += now - markAt; else Record.botTurnSeconds += now - markAt;
            markAt = now;
        }

        private void OnTransition(Transition transition)
        {
            if (!IsRecording) return;
            double now = clock();
            Accrue(now);
            if (transition.Events.Any(e => e.Kind == EventKind.MatchRestarted)) { Finish(now, "match restarted before victory"); return; }
            var before = previous;
            bool humanActed = HumanToAct(before);
            if (humanActed)
            {
                Record.humanActions++;
                if (before.Phase == MatchPhase.AwaitingSelection)
                {
                    if (RulesEngine.LegalMoves(before, human, before.PendingRoll.Value).Count > 1)
                    {
                        Record.meaningfulChoices++;
                        Record.longestSecondsWithoutMeaningfulChoice = Math.Max(Record.longestSecondsWithoutMeaningfulChoice, now - lastChoiceAt);
                        lastChoiceAt = now;
                    }
                    else Record.forcedChoices++;
                }
            }
            else Record.botActions++;
            foreach (var e in transition.Events)
            {
                switch (e.Kind)
                {
                    case EventKind.DieRolled:
                        Record.rolls++; if (humanActed) Record.humanRolls++;
                        if (e.Roll == 6) Record.sixes++;
                        break;
                    case EventKind.NoLegalMoves: if (humanActed) Record.humanNoMoveRolls++; break;
                    case EventKind.PieceBanished:
                        Record.knockouts++; if (e.Piece.Value.Owner == human) Record.humanKnockoutsSuffered++;
                        break;
                    case EventKind.PieceSummoned: Record.summons++; break;
                    case EventKind.PieceMoved:
                        if (e.From?.Kind == PieceKind.OnMainTrack && e.To?.Kind != PieceKind.OnMainTrack) Record.ascensionEntries++;
                        break;
                    case EventKind.PieceCompleted:
                        Record.completedPieces++; if (e.Piece.Value.Owner == human) Record.humanCompletedPieces++;
                        break;
                    case EventKind.TurnAdvanced: Record.turns++; break;
                    case EventKind.MatchWon: Record.winner = e.Player.ToString(); Record.naturalVictory = true; break;
                }
            }
            previous = transition.State;
            if (Record.naturalVictory) Finish(now, null);
        }

        private void Finish(double now, string abortReason)
        {
            IsRecording = false;
            session.Transitioned -= OnTransition;
            var r = Record;
            r.elapsedSeconds = now - startedAt;
            r.endUtc = Iso(utcNow());
            r.aborted = abortReason != null;
            r.abortReason = abortReason;
            r.abortSeconds = r.aborted ? r.elapsedSeconds : -1;
            r.rounds = r.turns / 4.0;
            r.longestSecondsWithoutMeaningfulChoice = Math.Max(r.longestSecondsWithoutMeaningfulChoice, now - lastChoiceAt);
            r.secondsPerMeaningfulChoice = r.meaningfulChoices > 0 ? r.elapsedSeconds / r.meaningfulChoices : -1;
            r.meaningfulChoicesPerMinute = r.elapsedSeconds > 0 ? r.meaningfulChoices / (r.elapsedSeconds / 60) : 0;
        }

        private static string Iso(DateTimeOffset time) => time.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }

    public static class PlaytestFiles
    {
        // One JSON file per session; saving again (with answers) overwrites the same file.
        public static string Save(PlaytestRecord record, string directory)
        {
            Directory.CreateDirectory(directory);
            string stamp = record.startUtc.Replace("-", "").Replace(":", "");
            string path = Path.Combine(directory, stamp + "-" + record.variant + "-seed" + record.seed + ".json");
            File.WriteAllText(path, record.ToJson().Replace("\r\n", "\n"));
            return path;
        }
    }
}
