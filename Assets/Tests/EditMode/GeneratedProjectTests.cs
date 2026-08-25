using Clicker.Game;
using Clicker.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Clicker.Tests
{
    public sealed class GeneratedProjectTests
    {
        [Test]
        public void GeneratedPrefabs_ContainExpectedTargetImplementations()
        {
            GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Clicker/Prefabs/CanvasClickTarget.prefab");
            GameObject worldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Clicker/Prefabs/WorldClickTarget.prefab");

            Assert.That(canvasPrefab, Is.Not.Null);
            Assert.That(worldPrefab, Is.Not.Null);
            Assert.That(canvasPrefab.GetComponent<CanvasClickTargetView>(), Is.Not.Null);
            Assert.That(worldPrefab.GetComponent<WorldClickTargetView>(), Is.Not.Null);
            Assert.That(worldPrefab.GetComponent<Collider>(), Is.Not.Null);
        }

        [TestCase("Assets/Scenes/Game.unity", typeof(CanvasTargetBoard))]
        [TestCase("Assets/Scenes/Game3D.unity", typeof(WorldTargetBoard))]
        public void GeneratedGameScene_HasCompleteSerializedComposition(string scenePath, System.Type boardType)
        {
            Scene scene = GetOrOpenScene(scenePath, out bool openedForTest);
            try
            {
                GameManager manager = FindInScene<GameManager>(scene);
                TargetBoard board = FindInScene<TargetBoard>(scene);

                Assert.That(manager, Is.Not.Null);
                Assert.That(board.GetType(), Is.EqualTo(boardType));
                Assert.That(FindInScene<GameHudView>(scene), Is.Not.Null);
                Assert.That(FindInScene<ResultScreen>(scene), Is.Not.Null);
                Assert.That(FindInScene<EventSystem>(scene), Is.Not.Null);

                AssertReferenceAssigned(manager, "targetBoard");
                AssertReferenceAssigned(manager, "hud");
                AssertReferenceAssigned(manager, "resultScreen");
                Assert.That(new SerializedObject(manager).FindProperty("targetCount").intValue, Is.EqualTo(9));
                AssertReferenceAssigned(board, "targetPrefab");
                AssertReferenceAssigned(board, "targetRoot");
            }
            finally
            {
                if (openedForTest)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void MainMenuScene_HasBothPresentationChoices()
        {
            Scene scene = GetOrOpenScene("Assets/Scenes/MainMenu.unity", out bool openedForTest);
            try
            {
                MainMenu menu = FindInScene<MainMenu>(scene);
                Assert.That(menu, Is.Not.Null);
                AssertReferenceAssigned(menu, "canvasModeButton");
                AssertReferenceAssigned(menu, "worldModeButton");
                Assert.That(FindInScene<EventSystem>(scene), Is.Not.Null);
            }
            finally
            {
                if (openedForTest)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static Scene GetOrOpenScene(string scenePath, out bool openedForTest)
        {
            Scene loadedScene = SceneManager.GetSceneByPath(scenePath);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                openedForTest = false;
                return loadedScene;
            }

            openedForTest = true;
            return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static void AssertReferenceAssigned(Object target, string propertyName)
        {
            SerializedProperty property = new SerializedObject(target).FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, $"Serialized field '{propertyName}' was not found on {target.GetType().Name}.");
            Assert.That(
                property.objectReferenceValue,
                Is.Not.Null,
                $"Serialized field '{propertyName}' is not assigned on {target.GetType().Name}.");
        }
    }
}
