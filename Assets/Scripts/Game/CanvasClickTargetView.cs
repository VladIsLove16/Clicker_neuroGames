using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Game
{
    [AddComponentMenu("Clicker/Targets/Canvas Click Target")]
    public sealed class CanvasClickTargetView : ClickTargetView
    {
        [SerializeField] private Image surface;
        [SerializeField] private TMP_Text numberLabel;
        [SerializeField] private Color idleColor = new(0.10f, 0.16f, 0.29f, 1f);
        [SerializeField] private Color currentColor = new(0.22f, 0.95f, 0.78f, 1f);
        [SerializeField] private Color focusColor = new(1f, 0.82f, 0.25f, 1f);

        protected override void Awake()
        {
            base.Awake();
            transition = Transition.None;
        }

        protected override void OnInitialized(int index)
        {
            if (numberLabel != null)
            {
                numberLabel.SetText("{0}", index + 1);
            }
        }

        protected override void ApplyVisualState(bool current, bool focused)
        {
            if (surface == null)
            {
                return;
            }

            surface.color = focused ? focusColor : current ? currentColor : idleColor;
            transform.localScale = current ? Vector3.one * 1.06f : Vector3.one;
        }
    }
}
