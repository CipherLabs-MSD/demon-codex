using DemonCodex.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemonCodex.Editor
{
    public static class PrototypeSceneBuilder
    {
        [MenuItem("Demon Codex/Rebuild prototype scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.035f, .043f, .065f);
            camera.orthographic = true;
            var match = new GameObject("Local Match");
            match.AddComponent<LocalMatchController>();
            match.AddComponent<LocalMatchView>();
            const string path = "Assets/DemonCodex/Scenes/LocalMatch.unity";
            System.IO.Directory.CreateDirectory("Assets/DemonCodex/Scenes");
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            PlayerSettings.companyName = "CipherLabs";
            PlayerSettings.productName = "Demon Codex M001 Prototype";
            PlayerSettings.defaultScreenWidth = 1200;
            PlayerSettings.defaultScreenHeight = 850;
            AssetDatabase.SaveAssets();
        }
    }
}
