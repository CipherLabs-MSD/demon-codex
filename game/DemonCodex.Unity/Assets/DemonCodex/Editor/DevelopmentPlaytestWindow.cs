using System;
using DemonCodex.Development;
using DemonCodex.Presentation;
using UnityEditor;
using UnityEngine;

namespace DemonCodex.Editor
{
    public sealed class DevelopmentPlaytestWindow : EditorWindow
    {
        [MenuItem("Demon Codex/Development Playtest Scenarios")]
        public static void Open()
        {
            var window = GetWindow<DevelopmentPlaytestWindow>("Development Playtests");
            window.minSize = new Vector2(370, 300);
        }
        // Keep enabled state and scenario text current as Play Mode and PLAY AGAIN change.
        private void OnInspectorUpdate() { Repaint(); }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("DEVELOPMENT TEST SCENARIO", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Editor only. Replaces the current match with a valid legal replay. " +
                "Click the indicated piece in the Game view; no extra roll is needed. " +
                "Bots pause so the outcome stays visible. PLAY AGAIN restores normal play.", MessageType.Info);
            var controller = Application.isPlaying ? FindFirstObjectByType<LocalMatchController>() : null;
            using (new EditorGUI.DisabledScope(controller == null || EditorApplication.isPaused))
                foreach (ScenarioKind kind in Enum.GetValues(typeof(ScenarioKind)))
                    if (GUILayout.Button("TEST " + kind.ToString().ToUpperInvariant(), GUILayout.Height(30)))
                    {
                        var scenario = ScenarioReplay.Prepare(kind);
                        controller.LoadDevelopmentSession(scenario.Session, scenario.Instruction);
                    }
            if (controller == null) EditorGUILayout.HelpBox("Open LocalMatch.unity and enter Play Mode first.", MessageType.Warning);
            else if (controller.DevelopmentScenario != null)
                EditorGUILayout.HelpBox(controller.DevelopmentScenario, MessageType.None);
        }
    }
}
