using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clicker.Game
{
    public abstract class TargetBoard : MonoBehaviour
    {
        [SerializeField, Min(1)] private int columns = 3;

        private readonly List<ClickTargetView> targets = new();
        private bool isBuilt;

        public event Action<int> TargetPressed;

        protected IReadOnlyList<ClickTargetView> Targets => targets;
        protected int Columns => columns;

        public void Build(int targetCount)
        {
            if (targetCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(targetCount));
            }

            ClearTargets();
            for (int index = 0; index < targetCount; index++)
            {
                ClickTargetView target = CreateTarget(index);
                if (target == null)
                {
                    throw new InvalidOperationException($"{GetType().Name} failed to create target {index}.");
                }

                target.Initialize(index, HandleTargetPressed);
                targets.Add(target);
            }

            isBuilt = true;
            ArrangeTargets();
            ConfigureNavigation();
        }

        public void SetCurrentTarget(int currentIndex)
        {
            EnsureBuilt();
            for (int index = 0; index < targets.Count; index++)
            {
                targets[index].SetCurrent(index == currentIndex);
            }
        }

        public void Show()
        {
            EnsureBuilt();
            SetBoardVisible(true);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(targets[0].gameObject);
            }
        }

        public void Hide()
        {
            if (!isBuilt)
            {
                return;
            }

            if (EventSystem.current != null && IsBoardSelection(EventSystem.current.currentSelectedGameObject))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            SetBoardVisible(false);
        }

        protected abstract ClickTargetView CreateTarget(int index);
        protected abstract void ArrangeTargets();
        protected abstract void SetBoardVisible(bool visible);
        protected abstract void DestroyTarget(ClickTargetView target);

        private void ConfigureNavigation()
        {
            int rows = Mathf.CeilToInt((float)targets.Count / columns);
            for (int index = 0; index < targets.Count; index++)
            {
                int row = index / columns;
                int column = index % columns;
                Navigation navigation = targets[index].navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnLeft = GetTarget(row, column - 1, rows);
                navigation.selectOnRight = GetTarget(row, column + 1, rows);
                navigation.selectOnUp = GetTarget(row - 1, column, rows);
                navigation.selectOnDown = GetTarget(row + 1, column, rows);
                targets[index].navigation = navigation;
            }
        }

        private Selectable GetTarget(int row, int column, int rowCount)
        {
            if (row < 0 || row >= rowCount || column < 0 || column >= columns)
            {
                return null;
            }

            int index = row * columns + column;
            return index >= 0 && index < targets.Count ? targets[index] : null;
        }

        private void ClearTargets()
        {
            foreach (ClickTargetView target in targets)
            {
                if (target != null)
                {
                    DestroyTarget(target);
                }
            }

            targets.Clear();
            isBuilt = false;
        }

        private void HandleTargetPressed(int targetIndex)
        {
            TargetPressed?.Invoke(targetIndex);
        }

        private bool IsBoardSelection(GameObject selected)
        {
            return selected != null && selected.TryGetComponent(out ClickTargetView target) && targets.Contains(target);
        }

        private void EnsureBuilt()
        {
            if (!isBuilt)
            {
                throw new InvalidOperationException($"{GetType().Name} must be built before it can be used.");
            }
        }
    }
}
