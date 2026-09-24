using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using EchoOfAncients.Unity;

namespace EchoOfAncients.EditorTools
{
    public static class DemoSceneSetup
    {
        [MenuItem("Эхо Древних/Создать демо-сцену")]
        public static void CreateDemoScene()
        {
            var scene = SceneManager.GetActiveScene();

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(55f, -30f, 0f);
            lightGo.tag = "Untagged";

            var cameraGo = new GameObject("Main Camera");
            var cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.13f, 0.15f);
            cam.transform.position = new Vector3(3.5f, 5f, -10f);
            cam.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            cam.orthographic = true;
            cam.orthographicSize = 9f;
            cam.tag = "MainCamera";

            var managerGo = new GameObject("GameManager");
            managerGo.AddComponent<BoardView>();
            managerGo.AddComponent<GameManager>();
            var manager = managerGo.GetComponent<GameManager>();
            manager.BoardView = managerGo.GetComponent<BoardView>();

            Selection.activeObject = managerGo;
            Debug.Log("[Эхо Древних] Демо-сцена создана. Нажмите Play.");
        }
    }
}