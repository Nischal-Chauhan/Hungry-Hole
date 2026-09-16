using UnityEngine;

namespace HungryHole.UI
{
    /// <summary>
    /// Centralized arcade UI palette and shared runtime widgets (UI/UX brief
    /// section 3: exact colors centralized so they can be changed globally).
    /// Values mirror the reference game's CSS tokens. Everything is generated at
    /// runtime from code — no font/texture/sprite assets, so no GUIDs are involved.
    /// </summary>
    public static class UiTheme
    {
        public static readonly Color BgDeep = Hex(0x0b0e14);
        public static readonly Color Accent = Hex(0xff5338);
        public static readonly Color Accent2 = Hex(0xffd23f);
        public static readonly Color Ink = Hex(0x0b0e14);
        public static readonly Color Paper = Hex(0xf4f1e8);

        public static readonly Color PaperStrong = WithAlpha(Paper, 0.75f);
        public static readonly Color PaperSoft = WithAlpha(Paper, 0.65f);
        public static readonly Color PaperFaint = WithAlpha(Paper, 0.6f);

        public static readonly Color Pill = new Color(11f / 255f, 14f / 255f, 20f / 255f, 0.72f);
        public static readonly Color PillBorder = new Color(1f, 1f, 1f, 0.14f);
        public static readonly Color Dim = new Color(6f / 255f, 7f / 255f, 10f / 255f, 0.75f);
        public static readonly Color Card = Hex(0x162432);
        public static readonly Color ButtonGhost = new Color(1f, 1f, 1f, 0.15f);

        public static readonly Color ScreenInner = Hex(0x152230);
        public static readonly Color ScreenOuter = Hex(0x0a0d13);

        public static readonly Color HoleMarkInner = Hex(0x2c121e);
        public static readonly Color HoleMarkCenter = Hex(0x06070a);

        private static Sprite roundedSprite;
        private static Sprite backdropSprite;
        private static Sprite holeMarkSprite;
        private static Sprite fillGradientSprite;
        private static Font gameFont;

        /// <summary>Unity's built-in font, resolved at runtime (no font asset).</summary>
        public static Font GameFont
        {
            get
            {
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        /// <summary>White rounded-rect sprite used by pills, cards and buttons (9-sliced).</summary>
        public static Sprite RoundedSprite
        {
            get
            {
                if (roundedSprite == null)
                {
                    const int size = 96;
                    const int radius = 24;
                    Color32[] pixels = new Color32[size * size];
                    float center = (size - 1) * 0.5f;
                    float inner = center - radius;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float qx = Mathf.Max(Mathf.Abs(x - center) - inner, 0f);
                            float qy = Mathf.Max(Mathf.Abs(y - center) - inner, 0f);
                            float distance = Mathf.Sqrt(qx * qx + qy * qy);
                            byte alpha = distance <= radius
                                ? (byte)255
                                : distance < radius + 1f ? (byte)(255f * (1f - (distance - radius))) : (byte)0;
                            pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                        }
                    }

                    Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "UiRounded" };
                    texture.SetPixels32(pixels);
                    texture.Apply(false, false);
                    roundedSprite = Sprite.Create(
                        texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 96f,
                        0, SpriteMeshType.FullRect, new Vector4(24, 24, 24, 24));
                }

                return roundedSprite;
            }
        }

        /// <summary>Full-screen menu/results backdrop: dark radial gradient like the reference screens.</summary>
        public static Sprite BackdropSprite
        {
            get
            {
                if (backdropSprite == null)
                {
                    const int size = 256;
                    Color32[] pixels = new Color32[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = (x / (float)(size - 1) - 0.5f) * 2f;
                            float dy = (y / (float)(size - 1) - 0.2f) * 2f;
                            float t = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) / 0.84f);
                            pixels[y * size + x] = Color32.Lerp(ScreenInner, ScreenOuter, t);
                        }
                    }

                    Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "UiBackdrop" };
                    texture.SetPixels32(pixels);
                    texture.Apply(false, false);
                    backdropSprite = Sprite.Create(
                        texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 256f, 0, SpriteMeshType.FullRect);
                }

                return backdropSprite;
            }
        }

        /// <summary>The menu's stylized hole mark: dark pit with a soft orange rim glow.</summary>
        public static Sprite HoleMarkSprite
        {
            get
            {
                if (holeMarkSprite == null)
                {
                    const int size = 128;
                    float radius = size * 0.5f;
                    Color32[] pixels = new Color32[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = x - (size - 1) * 0.5f;
                            float dy = y - (size - 1) * 0.5f;
                            float d = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                            Color color;
                            if (d < 0.6f)
                            {
                                color = Color.Lerp(HoleMarkInner, HoleMarkCenter, Mathf.Clamp01(d / 0.6f));
                            }
                            else if (d < 1f)
                            {
                                color = Accent;
                                color.a = 0.45f * (1f - (d - 0.6f) / 0.4f);
                            }
                            else
                            {
                                color = Color.clear;
                            }

                            pixels[y * size + x] = color;
                        }
                    }

                    Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "UiHoleMark" };
                    texture.SetPixels32(pixels);
                    texture.Apply(false, false);
                    holeMarkSprite = Sprite.Create(
                        texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 128f, 0, SpriteMeshType.FullRect);
                }

                return holeMarkSprite;
            }
        }

        /// <summary>Horizontal accent gradient for the hole-radius progress fill.</summary>
        public static Sprite FillGradientSprite
        {
            get
            {
                if (fillGradientSprite == null)
                {
                    const int width = 32;
                    const int height = 4;
                    Color32[] pixels = new Color32[width * height];
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            pixels[y * width + x] = Color32.Lerp(Accent, Accent2, x / (float)(width - 1));
                        }
                    }

                    Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "UiFillGradient" };
                    texture.SetPixels32(pixels);
                    texture.Apply(false, false);
                    fillGradientSprite = Sprite.Create(
                        texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
                }

                return fillGradientSprite;
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color Hex(int hex)
        {
            return new Color(
                ((hex >> 16) & 0xFF) / 255f,
                ((hex >> 8) & 0xFF) / 255f,
                (hex & 0xFF) / 255f,
                1f);
        }
    }
}