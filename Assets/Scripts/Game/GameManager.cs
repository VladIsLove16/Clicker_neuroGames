using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clicker.Game
{
    /// <summary>
    /// Coordinates the domain session and scene presentation. All scene dependencies are explicit.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        [Header("Round Rules")]
        [SerializeField, Min(1f)] private float roundDurationSeconds = 30f;
        [SerializeField, Range(2, 24)] private int targetCount = 9;
        [SerializeField, Min(0f)] private float wrongTargetPenaltySeconds = 1f;

        [Header("Scene References")]
        [SerializeField] private TargetBoard targetBoard;
        [SerializeField] private GameHudView hud;
        [SerializeField] private ResultScreen resultScreen;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private ClickerSession session;
        private bool isInitialized;

        private void Awake()
        {
            ValidateDependencies();

            targetBoard.Build(targetCount);
            resultScreen.Initialize(RestartGame, ReturnToMainMenu);
            resultScreen.Hide();
            isInitialized = true;
        }

        private void OnEnable()
        {
            if (targetBoard != null)
            {
                targetBoard.TargetPressed += HandleTargetPressed;
            }
        }

        private void Start()
        {
            StartGame();
        }

        private void Update()
        {
            if (!isInitialized || session == null || !session.IsPlaying)
            {
                return;
            }

            bool finished = session.Advance(Time.unscaledDeltaTime);
            hud.Render(session.RemainingTime, session.Score);

            if (finished)
            {
                FinishGame();
            }
        }

        private void OnDisable()
        {
            if (targetBoard != null)
            {
                targetBoard.TargetPressed -= HandleTargetPressed;
            }
        }

        private void StartGame()
        {
            session = new ClickerSession(
                roundDurationSeconds,
                wrongTargetPenaltySeconds,
                targetCount,
                new UnityRandomTargetSequence());

            session.Start();
            resultScreen.Hide();
            targetBoard.Show();
            targetBoard.SetCurrentTarget(session.CurrentTargetIndex);
            hud.Show();
            hud.Render(session.RemainingTime, session.Score);
        }

        private void RestartGame()
        {
            StartGame();
        }

        private void HandleTargetPressed(int targetIndex)
        {
            ClickResult clickResult = session.RegisterClick(targetIndex);
            if (!clickResult.Accepted)
            {
                return;
            }

            if (clickResult.WasCorrect)
            {
                targetBoard.SetCurrentTarget(clickResult.CurrentTargetIndex);
            }
            else
            {
                hud.ShowPenalty(wrongTargetPenaltySeconds);
            }

            hud.Render(clickResult.RemainingTime, clickResult.Score);

            if (clickResult.Finished)
            {
                FinishGame();
            }
        }

        private void FinishGame()
        {
            if (session == null || session.State != SessionState.Finished)
            {
                return;
            }

            targetBoard.Hide();
            hud.Render(0f, session.Score);
            resultScreen.Show(session.Score);
        }

        private void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }

        private void ValidateDependencies()
        {
            if (targetBoard == null || hud == null || resultScreen == null)
            {
                throw new MissingReferenceException(
                    $"{nameof(GameManager)} on '{name}' is not fully configured. " +
                    "Assign target board, HUD, and result screen in the scene.");
            }
        }
    }
}
