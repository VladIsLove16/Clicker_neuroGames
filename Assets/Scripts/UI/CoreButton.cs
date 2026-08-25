using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clicker.UI
{
    /// <summary>
    /// Shared button behaviour with a small device-independent press response.
    /// </summary>
    [AddComponentMenu("Clicker/UI/Core Button")]
    public sealed class CoreButton : Button
    {
        [SerializeField, Range(0.85f, 1f)] private float pressedScale = 0.96f;

        private Vector3 initialScale;

        protected override void Awake()
        {
            base.Awake();
            initialScale = transform.localScale;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (IsInteractable())
            {
                transform.localScale = initialScale * pressedScale;
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            RestoreScale();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            RestoreScale();
        }

        protected override void OnDisable()
        {
            RestoreScale();
            base.OnDisable();
        }

        private void RestoreScale()
        {
            transform.localScale = initialScale;
        }
    }
}
