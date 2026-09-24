using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using EchoOfAncients.Unity;

namespace EchoOfAncients.EditorTools
{
    public static class WebGLBuilder
    {
        private const string MenuRoot = "Эхо Древних/";

        [MenuItem(MenuRoot + "Собрать WebGL (Яндекс Игры)")]
        public static void BuildForYandex()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                Debug.LogError("Модуль WebGL Build Support не установлен. Установите его: Unity Hub → Installs → ⚙ → Add modules → WebGL Build Support.");
                return;
            }

            var scenes = EnsureBuildScenes();
            if (scenes.Count == 0)
            {
                Debug.LogError("Не удалось подготовить сцену для сборки.");
                return;
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                Debug.LogError("Не удалось переключить цель сборки на WebGL.");
                return;
            }

            var productName = PlayerSettings.productName;
            var outputDir = Path.Combine("Build", "WebGL");
            var zipPath = Path.Combine("Build", "WebGL.zip");

            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = outputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            try
            {
                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"[Эхо Древних] Сборка WebGL не удалась: {report.summary.result} ({report.summary.totalErrors} ошибок). Подробности в Console/Log.");
                    return;
                }

                if (File.Exists(zipPath)) File.Delete(zipPath);
                if (Directory.Exists(outputDir))
                {
                    ZipFile.CreateFromDirectory(outputDir, zipPath, System.IO.Compression.CompressionLevel.Optimal, includeBaseDirectory: false);
                }

                Debug.Log($"[Эхо Древних] Готово: {Path.GetFullPath(outputDir)}  →  {Path.GetFullPath(zipPath)}. Загрузите {zipPath} в консоль Яндекс.Игр.");
                EditorUtility.RevealInFinder(zipPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void BuildForCI()
        {
            var scenes = EnsureBuildScenes();
            if (scenes.Count == 0)
            {
                throw new Exception("[Эхо Древних] Нет ни одной сцены для сборки в Build Settings.");
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = Path.Combine("build", "WebGL"),
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"[Эхо Древних] WebGL build failed: {report.summary.result}, errors: {report.summary.totalErrors}");
            }

            Debug.Log("[Эхо Древних] CI WebGL build OK: build/WebGL");
        }

        private static List<string> EnsureBuildScenes()
        {
            var scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && File.Exists(scene.path)) scenes.Add(scene.path);
            }
            if (scenes.Count > 0) return scenes;

            var active = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(active.path))
            {
                if (UnityEngine.Object.FindFirstObjectByType<GameManager>() == null)
                {
                    DemoSceneSetup.CreateDemoScene();
                }
            }

            const string sceneFolder = "Assets/Scenes";
            if (!Directory.Exists(sceneFolder)) Directory.CreateDirectory(sceneFolder);
            const string scenePath = sceneFolder + "/Main.unity";

            var current = SceneManager.GetActiveScene();
            bool saved = EditorSceneManager.SaveScene(current, scenePath);
            if (!saved)
            {
                Debug.LogError("[Эхо Древних] Не удалось сохранить сцену.");
                return scenes;
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            scenes.Add(scenePath);
            return scenes;
        }
    }
}