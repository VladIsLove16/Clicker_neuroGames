using UnityEngine;

namespace Clicker.Game
{
    [AddComponentMenu("Clicker/Targets/Canvas Target Board")]
    public sealed class CanvasTargetBoard : TargetBoard
    {
        [SerializeField] private RectTransform targetRoot;
        [SerializeField] private GameObject targetPrefab;

        protected override ClickTargetView CreateTarget(int index)
        {
            GameObject instance = Instantiate(targetPrefab, targetRoot, false);
            if (!instance.TryGetComponent(out CanvasClickTargetView target))
            {
                Destroy(instance);
                throw new MissingComponentException(
                    $"Prefab '{targetPrefab.name}' must contain {nameof(CanvasClickTargetView)} on its root.");
            }

            return target;
        }

        protected override void ArrangeTargets()
        {
            if (targetRoot.TryGetComponent(out ResponsiveGridLayout grid))
            {
                grid.Configure(Columns, Targets.Count);
            }
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
            if (targetRoot == null || targetPrefab == null)
            {
                throw new MissingReferenceException($"{nameof(CanvasTargetBoard)} on '{name}' is not configured.");
            }
        }
    }
}
