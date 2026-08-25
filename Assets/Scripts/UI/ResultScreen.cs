using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clicker.Game
{
    public sealed class ResultScreen : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private Action restartAction;
        private Action mainMenuAction;

        public void Initialize(Action onRestart, Action onMainMenu)
        {
            if (root == null || scoreText == null || restartButton == null || mainMenuButton == null)
            {
                throw new MissingReferenceException($"{nameof(ResultScreen)} on '{name}' is not configured.");
            }

            restartAction = onRestart ?? throw new ArgumentNullException(nameof(onRestart));
            mainMenuAction = onMainMenu ?? throw new ArgumentNullException(nameof(onMainMenu));
            restartButton.onClick.RemoveListener(Restart);
            mainMenuButton.onClick.RemoveListener(MainMenu);
            restartButton.onClick.AddListener(Restart);
            mainMenuButton.onClick.AddListener(MainMenu);
        }

        public void Show(int score)
        {
            scoreText.SetText("{0}", score);
            root.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(restartButton.gameObject);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(Restart);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(MainMenu);
            }
        }

        private void Restart()
        {
            restartAction?.Invoke();
        }

        private void MainMenu()
        {
            mainMenuAction?.Invoke();
        }
    }
}
