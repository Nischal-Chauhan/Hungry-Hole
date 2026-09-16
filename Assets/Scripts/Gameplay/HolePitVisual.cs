using HungryHole.UI;
using UnityEngine;

namespace HungryHole.Gameplay
{
    /// <summary>
    /// M7 Stage B: replaces the pit's flat black disc look with a runtime-generated
    /// opaque radial gradient (near-black center warming toward the rim accent at
    /// the edge), so the hole reads as a stylized pit consistent with the menu hole
    /// mark. Purely visual: the texture is generated once in Start, there is no
    /// per-frame work or allocation, no transparency, and no gameplay contact —
    /// the shared HolePit.mat stays untouched because the gradient runs on a
    /// runtime material instance.
    /// </summary>
    public class HolePitVisual : MonoBehaviour
    {
        [Tooltip("Pit renderer. Left empty, the renderer on this object is used.")]
        [SerializeField] private Renderer pitRenderer;

        private Texture2D gradient;
        private Material pitMaterial;

        private void Start()
        {
            if (pitRenderer == null)
            {
                pitRenderer = GetComponent<Renderer>();
            }

            if (pitRenderer == null)
            {
                Debug.LogWarning("HolePitVisual found no pit renderer; pit gradient disabled.", this);
                return;
            }

            gradient = CreateGradientTexture();

            // Unlit so the pit stays dark under any light angle and still fades
            // with scene fog, exactly like the emissive-lit rim's surroundings.
            pitMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            pitMaterial.SetTexture("_BaseMap", gradient);
            pitRenderer.material = pitMaterial;
        }

        private void OnDestroy()
        {
            if (gradient != null)
            {
                Destroy(gradient);
            }

            if (pitMaterial != null)
            {
                Destroy(pitMaterial);
            }
        }

        /// <summary>
        /// Paints the 256x256 gradient once: near-black center matching the menu
        /// hole mark, smoothly warming toward a dark accent tone at the edge so
        /// the pit visually meets the emissive rim ring. Fully opaque — no
        /// transparency or sorting involved.
        /// </summary>
        private static Texture2D CreateGradientTexture()
        {
            const int size = 256;
            Color32 deep = UiTheme.HoleMarkCenter;
            Color32 warmEdge = (Color32)(UiTheme.Accent * 0.42f);
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - (size - 1) * 0.5f;
                    float dy = y - (size - 1) * 0.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) / (size * 0.5f);
                    float t = Mathf.Clamp01((distance - 0.55f) / 0.45f);
                    t = t * t * (3f - 2f * t); // smoothstep: soft blend, no banding
                    pixels[y * size + x] = Color32.Lerp(deep, warmEdge, t);
                }
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "HolePitGradient",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }
    }
}