using UnityEngine;

namespace HungryHole.Gameplay
{
    /// <summary>
    /// One pooled floating '+points' label (Phase 5, floating score feedback).
    /// Minimal world-space stand-in for the reference screen-space floatUp
    /// animation: pops in, drifts upward and fades over the reference 0.9 s.
    /// Big variant (heavy targets) matches the reference size/color ratio.
    /// </summary>
    public class FloatingScorePopup : MonoBehaviour
    {
        private const float Lifetime = 0.9f;
        private const float RiseDistance = 1.6f;

        private static readonly Color NormalColor = new Color(1f, 0.823f, 0.247f); // #ffd23f (reference accent)
        private static readonly Color BigColor = new Color(1f, 0.941f, 0.537f);    // #fff089 (reference big variant)

        private TextMesh textMesh;
        private Color baseColor;
        private Vector3 basePosition;
        private float age;
        private bool active;
        private Camera mainCamera;

        private void Awake()
        {
            // The spawner creates the pooled objects with only this script, so the
            // TextMesh is added here (its RequireComponent brings the MeshRenderer).
            textMesh = gameObject.AddComponent<TextMesh>();
            // Unity's built-in font, assigned at runtime so no font asset/GUID is involved.
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textMesh.font = font;
            textMesh.fontSize = 64;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.35f;
            GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        public void Show(Vector3 position, string text, bool big)
        {
            basePosition = position;
            baseColor = big ? BigColor : NormalColor;
            age = 0f;
            active = true;
            mainCamera = Camera.main;
            textMesh.text = text;
            textMesh.characterSize = big ? 0.56f : 0.35f;
        }

        private void Update()
        {
            if (!active)
            {
                return;
            }

            age += Time.deltaTime;
            float t = age / Lifetime;
            if (t >= 1f)
            {
                active = false;
                textMesh.text = string.Empty;
                return;
            }

            // Reference floatUp curve: pop to 1.15x within the first 20%,
            // settle back to 1x, rise, fade out.
            float pop = t < 0.2f ? Mathf.Lerp(0.6f, 1.15f, t / 0.2f) : Mathf.Lerp(1.15f, 1f, (t - 0.2f) / 0.8f);
            float alpha = t < 0.2f ? t / 0.2f : 1f - (t - 0.2f) / 0.8f;

            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }

            transform.localScale = Vector3.one * pop;
            transform.position = basePosition + Vector3.up * (RiseDistance * t);
            Color color = baseColor;
            color.a = alpha;
            textMesh.color = color;
        }
    }
}