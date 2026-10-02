using System;
using System.IO;
using DemonCodex.Development;
using DemonCodex.Presentation;
using UnityEditor;
using UnityEngine;

namespace DemonCodex.Editor
{
    // Editor-only M001.1 playtest: start CONTROL A or VARIANT B, record one match,
    // then ask five questions. Results go to tests/evidence/dc-0037/sessions.
    public sealed class M001PlaytestWindow : EditorWindow
    {
        private static readonly string[] Testers = { "owner", "non-builder" };
        private static readonly string[] TesterLabels = { "Owner (built the game)", "Someone who did not build the game" };
        private static readonly string[] OneToFive = { "1", "2", "3", "4", "5" };
        private static readonly string[] LengthLabels = { "Too short", "Good", "Too long" };
        private static readonly string[] AgainLabels = { "Yes", "Maybe", "No" };

        [SerializeField] private int testerIndex;
        [SerializeField] private long seed = PlaytestExperiment.SuggestedSeeds[0];
        [SerializeField] private PlaytestRecord pending;
        [SerializeField] private string savedFile;
        [SerializeField] private int fun = -1, length = -1, control = -1, waiting = -1, again = -1;
        [SerializeField] private string note = "";
        private PlaytestRecorder recorder;
        private LocalMatchController controller;

        [MenuItem("Demon Codex/M001.1 Playtest")]
        public static void Open() { GetWindow<M001PlaytestWindow>("M001.1 Playtest").minSize = new Vector2(400, 560); }

        private static string SessionsDirectory => Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "..", "..", "tests", "evidence", "dc-0037", "sessions"));
        private bool HasPending => pending != null && !string.IsNullOrEmpty(pending.variant);

        private void OnEnable() { EditorApplication.playModeStateChanged += OnPlayModeChanged; }
        private void OnDisable()
        {
            // The recorder is not serializable: save before the window closes or scripts reload.
            FinishSession("playtest window closed or scripts reloaded", focus: false);
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode) FinishSession("Play Mode exited", focus: true);
        }

        // Every ending (victory, stop, Play Mode exit, window close) saves the session exactly once.
        private void FinishSession(string reasonIfStillRecording, bool focus)
        {
            if (recorder == null) return;
            if (recorder.IsRecording) recorder.Abort(reasonIfStillRecording);
            Complete(focus);
        }

        private void OnInspectorUpdate()
        {
            if (recorder != null)
            {
                if (recorder.IsRecording && (controller == null || controller.Session != recorder.Session))
                    recorder.Abort("match replaced outside the playtest window");
                if (!recorder.IsRecording) Complete(focus: true);
            }
            Repaint();
        }

        private void Start(PlaytestVariant variant)
        {
            controller = FindAnyObjectByType<LocalMatchController>();
            recorder = PlaytestLauncher.Start(controller, variant, (uint)seed, Testers[testerIndex],
                () => EditorApplication.timeSinceStartup, () => DateTimeOffset.UtcNow);
            pending = null; savedFile = null;
        }

        private void Complete(bool focus)
        {
            pending = recorder.Record;
            recorder = null;
            PlaytestLauncher.RestoreControlPacing(controller);
            fun = length = control = waiting = again = -1; note = "";
            savedFile = Path.GetFileName(PlaytestFiles.Save(pending, SessionsDirectory)); // timing is kept even if questions are skipped
            if (focus) Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("M001.1 PLAYTEST", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Editor only. Records one timed match, then five questions. A and B use identical " +
                "rules, board, dice and bots; only bot speed differs.", MessageType.Info);
            if (recorder != null && recorder.IsRecording) DrawRecording();
            else if (HasPending && !pending.answered) DrawQuestions();
            else DrawStart();
        }

        private void DrawStart()
        {
            testerIndex = EditorGUILayout.Popup("Who is playing?", testerIndex, TesterLabels);
            seed = Math.Max(0, Math.Min(uint.MaxValue, EditorGUILayout.LongField("Seed", seed)));
            EditorGUILayout.LabelField("Use the same seed for A and B (same dice; choices still change the match). Suggested: " +
                string.Join(", ", PlaytestExperiment.SuggestedSeeds) + ".", EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrEmpty(savedFile)) EditorGUILayout.HelpBox("Saved: " + savedFile, MessageType.None);
            bool ready = Application.isPlaying && !EditorApplication.isPaused && FindAnyObjectByType<LocalMatchController>() != null;
            using (new EditorGUI.DisabledScope(!ready))
                foreach (PlaytestVariant variant in Enum.GetValues(typeof(PlaytestVariant)))
                    if (GUILayout.Button(PlaytestExperiment.Label(variant), GUILayout.Height(34))) Start(variant);
            if (!ready) EditorGUILayout.HelpBox("Open LocalMatch.unity and press Play first.", MessageType.Warning);
        }

        private void DrawRecording()
        {
            var r = recorder.Record; // no running clock on screen: it could bias felt length
            EditorGUILayout.HelpBox("RECORDING  " + PlaytestExperiment.Label((PlaytestVariant)Enum.Parse(typeof(PlaytestVariant), r.variant)) +
                "\nSeed " + r.seed + " · started " + r.startUtc + "\nPlay in the Game view until someone wins.", MessageType.None);
            if (GUILayout.Button("STOP MATCH — I WANT TO STOP", GUILayout.Height(34))) FinishSession("player stopped", focus: true);
        }

        private void DrawQuestions()
        {
            string result = pending.naturalVictory ? "Match finished: " + pending.winner + " won"
                : "Match stopped after " + TimeSpan.FromSeconds(pending.elapsedSeconds).ToString(@"m\:ss");
            EditorGUILayout.HelpBox(result + ". Timing saved. Now answer five quick questions.", MessageType.None);
            fun = Question(0, "1 = not fun, 5 = very fun", fun, OneToFive);
            length = Question(1, null, length, LengthLabels);
            control = Question(2, "1 = no control, 5 = full control", control, OneToFive);
            waiting = Question(3, "1 = almost none, 5 = mostly waiting", waiting, OneToFive);
            again = Question(4, null, again, AgainLabels);
            EditorGUILayout.LabelField("Optional note", EditorStyles.boldLabel);
            note = EditorGUILayout.TextField(note);
            using (new EditorGUI.DisabledScope(fun < 0 || length < 0 || control < 0 || waiting < 0 || again < 0))
                if (GUILayout.Button("SAVE ANSWERS", GUILayout.Height(34)))
                {
                    pending.SetAnswers(fun + 1, PlaytestExperiment.LengthAnswers[length], control + 1, waiting + 1,
                        PlaytestExperiment.PlayAgainAnswers[again], note);
                    savedFile = Path.GetFileName(PlaytestFiles.Save(pending, SessionsDirectory));
                }
        }

        private static int Question(int index, string hint, int selected, string[] options)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField((index + 1) + ". " + PlaytestExperiment.Questions[index], EditorStyles.boldLabel);
            if (hint != null) EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);
            return GUILayout.Toolbar(selected, options);
        }
    }
}
