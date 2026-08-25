using UnityEngine;

namespace Clicker.Game
{
    [AddComponentMenu("Clicker/Targets/World Click Target")]
    public sealed class WorldClickTargetView : ClickTargetView
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Transform visual;
        [SerializeField] private Color idleColor = new(0.08f, 0.22f, 0.42f, 1f);
        [SerializeField] private Color currentColor = new(0.12f, 1f, 0.72f, 1f);
        [SerializeField] private Color focusColor = new(1f, 0.72f, 0.18f, 1f);

        private MaterialPropertyBlock propertyBlock;
        private Vector3 baseScale = Vector3.one;
        private bool initializedScale;

        protected override void Awake()
        {
            base.Awake();
            propertyBlock = new MaterialPropertyBlock();
            transition = Transition.None;
            CacheBaseScale();
        }

        protected override void ApplyVisualState(bool current, bool focused)
        {
            if (targetRenderer == null)
            {
                return;
            }

            CacheBaseScale();
            propertyBlock ??= new MaterialPropertyBlock();
            Color color = focused ? focusColor : current ? currentColor : idleColor;
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            propertyBlock.SetColor(EmissionColorId, current ? color * 1.8f : Color.black);
            targetRenderer.SetPropertyBlock(propertyBlock);

            if (visual != null)
            {
                visual.localScale = baseScale * (current ? 1.18f : focused ? 1.08f : 1f);
            }
        }

        private void Update()
        {
            if (visual != null && isActiveAndEnabled)
            {
                visual.Rotate(Vector3.up, 28f * Time.unscaledDeltaTime, Space.Self);
            }
        }

        private void CacheBaseScale()
        {
            if (!initializedScale && visual != null)
            {
                baseScale = visual.localScale;
                initializedScale = true;
            }
        }
    }
}
