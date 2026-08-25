using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clicker.Game
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
    public sealed class ResponsiveGridLayout : UIBehaviour
    {
        [SerializeField, Min(1)] private int columns = 3;
        [SerializeField, Min(1)] private int itemCount = 9;
        [SerializeField, Range(0f, 0.5f)] private float spacingRatio = 0.18f;
        [SerializeField, Min(0f)] private float edgePadding = 24f;

        private RectTransform rectTransform;
        private GridLayoutGroup grid;

        public void Configure(int columnCount, int targetCount)
        {
            columns = Mathf.Max(1, columnCount);
            itemCount = Mathf.Max(1, targetCount);
            Refresh();
        }

        protected override void Awake()
        {
            base.Awake();
            CacheComponents();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            Refresh();
        }

        private void CacheComponents()
        {
            rectTransform ??= (RectTransform)transform;
            grid ??= GetComponent<GridLayoutGroup>();
        }

        private void Refresh()
        {
            CacheComponents();
            if (rectTransform == null || grid == null)
            {
                return;
            }

            int rows = Mathf.CeilToInt((float)itemCount / columns);
            float width = Mathf.Max(1f, rectTransform.rect.width - edgePadding * 2f);
            float height = Mathf.Max(1f, rectTransform.rect.height - edgePadding * 2f);
            float widthUnits = columns + Mathf.Max(0, columns - 1) * spacingRatio;
            float heightUnits = rows + Mathf.Max(0, rows - 1) * spacingRatio;
            float cellSize = Mathf.Min(width / widthUnits, height / heightUnits);
            float spacing = cellSize * spacingRatio;

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.padding = new RectOffset(
                Mathf.RoundToInt(edgePadding),
                Mathf.RoundToInt(edgePadding),
                Mathf.RoundToInt(edgePadding),
                Mathf.RoundToInt(edgePadding));
            grid.cellSize = Vector2.one * cellSize;
            grid.spacing = Vector2.one * spacing;
        }
    }
}
