using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Clicker.UI
{
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button canvasModeButton;
        [SerializeField] private Button worldModeButton;
        [SerializeField] private string canvasGameSceneName = "Game";
        [SerializeField] private string worldGameSceneName = "Game3D";

        private bool isLoading;

        private void Awake()
        {
            if (canvasModeButton == null || worldModeButton == null)
            {
                throw new MissingReferenceException($"{nameof(MainMenu)} on '{name}' is not fully configured.");
            }

            canvasModeButton.onClick.AddListener(LoadCanvasGame);
            worldModeButton.onClick.AddListener(LoadWorldGame);
        }

        private void Start()
        {
            EventSystem.current?.SetSelectedGameObject(canvasModeButton.gameObject);
        }

        private void OnDestroy()
        {
            canvasModeButton.onClick.RemoveListener(LoadCanvasGame);
            worldModeButton.onClick.RemoveListener(LoadWorldGame);
        }

        private void LoadCanvasGame()
        {
            BeginLoad(canvasGameSceneName);
        }

        private void LoadWorldGame()
        {
            BeginLoad(worldGameSceneName);
        }

        private void BeginLoad(string sceneName)
        {
            if (!isLoading)
            {
                StartCoroutine(LoadScene(sceneName));
            }
        }

        private IEnumerator LoadScene(string sceneName)
        {
            isLoading = true;
            canvasModeButton.interactable = false;
            worldModeButton.interactable = false;
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}
