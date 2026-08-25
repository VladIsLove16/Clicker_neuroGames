using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clicker.Game
{
    /// <summary>
    /// Common interaction contract for Canvas and world-space targets.
    /// </summary>
    public abstract class ClickTargetView : Selectable, IPointerClickHandler, ISubmitHandler
    {
        private Action<int> onPressed;
        private bool isCurrent;
        private bool isHovered;
        private bool isSelected;

        public int Index { get; private set; } = -1;

        public void Initialize(int index, Action<int> pressedCallback)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Index = index;
            onPressed = pressedCallback ?? throw new ArgumentNullException(nameof(pressedCallback));
            interactable = true;
            SetCurrent(false);
            OnInitialized(index);
        }

        public void SetCurrent(bool value)
        {
            isCurrent = value;
            RefreshVisuals();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Press();
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Press();
            eventData.Use();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            isHovered = true;
            RefreshVisuals();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            isHovered = false;
            RefreshVisuals();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            isSelected = true;
            RefreshVisuals();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            isSelected = false;
            RefreshVisuals();
        }

        protected override void OnDisable()
        {
            isHovered = false;
            isSelected = false;
            base.OnDisable();
        }

        protected virtual void OnInitialized(int index)
        {
        }

        protected abstract void ApplyVisualState(bool current, bool focused);

        private void Press()
        {
            if (IsInteractable() && Index >= 0)
            {
                onPressed?.Invoke(Index);
            }
        }

        private void RefreshVisuals()
        {
            ApplyVisualState(isCurrent, isHovered || isSelected);
        }
    }
}
