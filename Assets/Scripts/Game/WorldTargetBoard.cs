using UnityEngine;

namespace Clicker.Game
{
    [AddComponentMenu("Clicker/Targets/World Target Board")]
    public sealed class WorldTargetBoard : TargetBoard
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform targetRoot;
        [SerializeField] private GameObject targetPrefab;
        [SerializeField, Min(1f)] private float distanceFromCamera = 12f;
        [SerializeField] private Vector2 viewportHorizontalRange = new(0.16f, 0.84f);
        [SerializeField] private Vector2 viewportVerticalRange = new(0.16f, 0.78f);

        private int screenWidth;
        private int screenHeight;

        protected override ClickTargetView CreateTarget(int index)
        {
            GameObject instance = Instantiate(targetPrefab, targetRoot, false);
            if (!instance.TryGetComponent(out WorldClickTargetView target))
            {
                Destroy(instance);
                throw new MissingComponentException(
                    $"Prefab '{targetPrefab.name}' must contain {nameof(WorldClickTargetView)} on its root.");
            }

            return target;
        }

        protected override void ArrangeTargets()
        {
            int rowCount = Mathf.CeilToInt((float)Targets.Count / Columns);
            float viewportWidth = viewportHorizontalRange.y - viewportHorizontalRange.x;
            float viewportHeight = viewportVerticalRange.y - viewportVerticalRange.x;
            float worldHeight = 2f * distanceFromCamera * Mathf.Tan(worldCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float worldWidth = worldHeight * worldCamera.aspect;
            float targetSize = Mathf.Min(worldWidth * viewportWidth / Columns, worldHeight * viewportHeight / rowCount) * 0.48f;

            for (int index = 0; index < Targets.Count; index++)
            {
                int row = index / Columns;
                int column = index % Columns;
                int targetsInRow = Mathf.Min(Columns, Targets.Count - row * Columns);
                float horizontalStep = viewportWidth / targetsInRow;
                float verticalStep = viewportHeight / rowCount;
                float viewportX = viewportHorizontalRange.x + horizontalStep * (column + 0.5f);
                float viewportY = viewportVerticalRange.y - verticalStep * (row + 0.5f);

                Transform target = Targets[index].transform;
                target.position = worldCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, distanceFromCamera));
                target.rotation = Quaternion.LookRotation(worldCamera.transform.forward, worldCamera.transform.up);
                target.localScale = Vector3.one * targetSize;
            }

            screenWidth = Screen.width;
            screenHeight = Screen.height;
        }

        protected override void SetBoardVisible(bool visible)
        {
            targetRoot.gameObject.SetActive(visible);
        }

        protected override void DestroyTarget(ClickTargetView target)
        {
            Destroy(target.gameObject);
        }

        private void Awake()
        {
            if (worldCamera == null || targetRoot == null || targetPrefab == null)
            {
                throw new MissingReferenceException($"{nameof(WorldTargetBoard)} on '{name}' is not configured.");
            }
        }

        private void LateUpdate()
        {
            if (Targets.Count > 0 && (screenWidth != Screen.width || screenHeight != Screen.height))
            {
                ArrangeTargets();
            }
        }
    }
}
