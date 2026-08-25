using System.Globalization;
using TMPro;
using UnityEngine;

namespace Clicker.Game
{
    public sealed class GameHudView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup root;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text penaltyText;
        [SerializeField, Min(0.1f)] private float penaltyDisplayDuration = 0.65f;

        private float hidePenaltyAt;

        private void Awake()
        {
            if (root == null || timerText == null || scoreText == null || penaltyText == null)
            {
                throw new MissingReferenceException($"{nameof(GameHudView)} on '{name}' is not configured.");
            }

            penaltyText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (penaltyText.gameObject.activeSelf && Time.unscaledTime >= hidePenaltyAt)
            {
                penaltyText.gameObject.SetActive(false);
            }
        }

        public void Show()
        {
            root.alpha = 1f;
            root.interactable = false;
            root.blocksRaycasts = false;
        }

        public void Render(float remainingSeconds, int score)
        {
            timerText.SetText(remainingSeconds.ToString("0.0", CultureInfo.InvariantCulture));
            scoreText.SetText("SCORE  {0}", score);
        }

        public void ShowPenalty(float penaltySeconds)
        {
            penaltyText.text = $"-{penaltySeconds.ToString("0.0", CultureInfo.InvariantCulture)}s";
            penaltyText.gameObject.SetActive(true);
            hidePenaltyAt = Time.unscaledTime + penaltyDisplayDuration;
        }
    }
}
