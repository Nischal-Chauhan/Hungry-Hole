using HungryHole.Core;
using UnityEngine;

namespace HungryHole.Environment
{
    /// <summary>
    /// Milestone 4 town environment (Phase 6). Builds the road-grid ground look as a
    /// single runtime texture on the existing World/Ground renderer — the port of the
    /// reference's createCityTexture/buildGround: green base with dark road strips in a
    /// grid, dashed white center markings and gray-outlined intersection plots.
    /// M7 Stage D adds texture-only readability pass detail on the same 512 buffer:
    /// subtle plot tonal variation, light sidewalk edging beside the roads, and
    /// sparse crosswalk markings at two of the four intersections.
    /// Texture-only on purpose: no extra geometry, lights or draw calls (Intel HD 5500
    /// budget). Authored art/prefabs replace this in the Phase 9 visual-polish phase.
    /// </summary>
    public class TownEnvironment : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        [Tooltip("The town ground mesh (World/Ground). Left empty, looked up by name.")]
        [SerializeField] private Renderer groundRenderer;

        [Tooltip("Pixels per side of the generated tile texture (reference: 512).")]
        [SerializeField] private int tileResolution = 512;

        private Texture2D townTile;

        private void Start()
        {
            if (groundRenderer == null)
            {
                GameObject groundObject = GameObject.Find("World/Ground");
                if (groundObject != null)
                {
                    groundRenderer = groundObject.GetComponent<Renderer>();
                }
            }

            if (groundRenderer == null)
            {
                Debug.LogWarning("TownEnvironment has no ground renderer assigned; town texture skipped.", this);
                return;
            }

            ApplyTownTexture();
        }

        private void OnDestroy()
        {
            // The generated texture belongs to this run only; the material instance
            // dies with the renderer it was created from.
            if (townTile != null)
            {
                Destroy(townTile);
                townTile = null;
            }
        }

        /// <summary>
        /// Reference createCityTexture: a 512 canvas painted with the road grid —
        /// green base, two vertical/two horizontal dark road strips, dashed white
        /// center lines, gray-outlined intersection plots — tiled groundSize/cellSize
        /// times. Painted in texture pixels; road anchors are cell fractions so the
        /// pattern wraps seamlessly.
        /// </summary>
        private void ApplyTownTexture()
        {
            float cellSize = config != null ? config.townCellSize : 48f;
            float roadWidth = config != null ? config.townRoadWidth : 7.5f;
            float groundSize = config != null ? config.groundSize : 240f;

            int roadPx = Mathf.Max(1, Mathf.RoundToInt(tileResolution * (roadWidth / cellSize)));
            float tiles = Mathf.Max(1f, groundSize / cellSize);

            Color32 baseGreen = HexColor(0x4fa85b);
            Color32 roadDark = HexColor(0x343a40);
            Color32 dashWhite = HexColor(0xf8f9fa);
            Color32 plotGray = HexColor(0xadb5bd);

            // M7 Stage D tones: near-identical plot green, restrained light
            // sidewalk edging, and a slightly dimmed white for crosswalk bars so
            // they read as paint on the road rather than a second bright layer.
            Color32 plotAltGreen = HexColor(0x4a9c55);
            Color32 sidewalkGray = HexColor(0xa8b3a3);
            Color32 crosswalkWhite = HexColor(0xdfe4e8);
            int sidewalkPx = Mathf.Max(2, Mathf.RoundToInt(tileResolution * (1.125f / cellSize))); // 12px at 512

            townTile = new Texture2D(tileResolution, tileResolution, TextureFormat.RGBA32, true)
            {
                name = "TownTileTexture",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            Color32[] pixels = new Color32[tileResolution * tileResolution];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = baseGreen;
            }

            // Road anchors mirror the reference layout: first road's left/top edge at
            // 80/512 of the cell, second at 340/512.
            int road1 = Mathf.RoundToInt(0.15625f * tileResolution);
            int road2 = Mathf.RoundToInt(0.6640625f * tileResolution);

            // M7 Stage D: subtle tonal variation between plot areas — a parity
            // checkerboard over the 3x3 plot regions so edge plots keep the same
            // tone across tile seams (regions A and C always share a parity).
            int[] plotCols = { 0, road1 + roadPx, road2 + roadPx };
            int[] plotRows = { 0, road1 + roadPx, road2 + roadPx };
            int[] plotWidths = { road1, road2 - (road1 + roadPx), tileResolution - (road2 + roadPx) };
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (((row + col) & 1) == 1)
                    {
                        FillRect(pixels, plotCols[col], plotRows[row], plotWidths[col], plotWidths[row], plotAltGreen);
                    }
                }
            }

            // M7 Stage D: light sidewalk edging immediately beside each road,
            // drawn before the roads so crossings stay clean road surface.
            FillRect(pixels, road1 - sidewalkPx, 0, sidewalkPx, tileResolution, sidewalkGray);
            FillRect(pixels, road1 + roadPx, 0, sidewalkPx, tileResolution, sidewalkGray);
            FillRect(pixels, road2 - sidewalkPx, 0, sidewalkPx, tileResolution, sidewalkGray);
            FillRect(pixels, road2 + roadPx, 0, sidewalkPx, tileResolution, sidewalkGray);
            FillRect(pixels, 0, road1 - sidewalkPx, tileResolution, sidewalkPx, sidewalkGray);
            FillRect(pixels, 0, road1 + roadPx, tileResolution, sidewalkPx, sidewalkGray);
            FillRect(pixels, 0, road2 - sidewalkPx, tileResolution, sidewalkPx, sidewalkGray);
            FillRect(pixels, 0, road2 + roadPx, tileResolution, sidewalkPx, sidewalkGray);

            FillRect(pixels, road1, 0, roadPx, tileResolution, roadDark);
            FillRect(pixels, road2, 0, roadPx, tileResolution, roadDark);
            FillRect(pixels, 0, road1, tileResolution, roadPx, roadDark);
            FillRect(pixels, 0, road2, tileResolution, roadPx, roadDark);

            // Reference markings: dashed lines along both road centers (16 on, 16 off).
            int dash = Mathf.Max(1, tileResolution / 32); // 16px at 512
            int lineWidth = Mathf.Max(1, dash / 4);       // 4px at 512
            for (int offset = 0; offset < tileResolution; offset += dash * 2)
            {
                int length = Mathf.Min(dash, tileResolution - offset);

                // Vertical roads: dashes run top to bottom.
                FillRect(pixels, road1 - lineWidth / 2, offset, lineWidth, length, dashWhite);
                FillRect(pixels, road2 - lineWidth / 2, offset, lineWidth, length, dashWhite);
                // Horizontal roads: dashes run left to right.
                FillRect(pixels, offset, road1 - lineWidth / 2, length, lineWidth, dashWhite);
                FillRect(pixels, offset, road2 - lineWidth / 2, length, lineWidth, dashWhite);
            }

            // Gray-outlined intersection plots at the four road crossings.
            int stroke = Mathf.Max(1, Mathf.RoundToInt(tileResolution * 0.01953125f)); // 10px at 512
            StrokeRect(pixels, road1, road1, roadPx, roadPx, stroke, plotGray);
            StrokeRect(pixels, road2, road1, roadPx, roadPx, stroke, plotGray);
            StrokeRect(pixels, road1, road2, roadPx, roadPx, stroke, plotGray);
            StrokeRect(pixels, road2, road2, roadPx, roadPx, stroke, plotGray);

            // M7 Stage D: sparse crosswalks — only the two diagonal intersections,
            // south and west approaches only, drawn last as surface paint. The gap
            // keeps the bands clear of the gray plot outlines; bars sit inside the
            // road width so nothing bleeds onto plots or sidewalks.
            int barPx = Mathf.Max(2, roadPx / 16);   // 5px at 512
            int bandPx = Mathf.Max(2, roadPx / 8);   // 10px at 512
            int crossGap = Mathf.Max(2, roadPx / 8); // 10px at 512
            DrawCrosswalkSouth(pixels, road1, road1, roadPx, barPx, bandPx, crossGap, crosswalkWhite);
            DrawCrosswalkSouth(pixels, road2, road2, roadPx, barPx, bandPx, crossGap, crosswalkWhite);
            DrawCrosswalkWest(pixels, road1, road1, roadPx, barPx, bandPx, crossGap, crosswalkWhite);
            DrawCrosswalkWest(pixels, road2, road2, roadPx, barPx, bandPx, crossGap, crosswalkWhite);

            townTile.SetPixels32(pixels);
            townTile.Apply(true, true);

            // Runtime material instance so the on-disk Ground.mat stays untouched.
            Material material = groundRenderer.material;
            material.mainTexture = townTile;
            material.mainTextureScale = new Vector2(tiles, tiles);
            material.mainTextureOffset = Vector2.zero;
        }

        // --- Pixel drawing helpers (into a Color32 buffer). ---

        private static void FillRect(Color32[] pixels, int x, int y, int w, int h, Color32 color)
        {
            int size = Mathf.RoundToInt(Mathf.Sqrt(pixels.Length));
            for (int row = y; row < y + h; row++)
            {
                if (row < 0 || row >= size)
                {
                    continue;
                }

                for (int col = x; col < x + w; col++)
                {
                    if (col < 0 || col >= size)
                    {
                        continue;
                    }

                    pixels[row * size + col] = color;
                }
            }
        }

        private static void StrokeRect(Color32[] pixels, int x, int y, int w, int h, int stroke, Color32 color)
        {
            FillRect(pixels, x, y, w, stroke, color);
            FillRect(pixels, x, y + h - stroke, w, stroke, color);
            FillRect(pixels, x, y, stroke, h, color);
            FillRect(pixels, x + w - stroke, y, stroke, h, color);
        }

        // --- M7 Stage D crosswalk helpers (zebra bars parallel to traffic). ---

        /// <summary>Vertical-road crossing just south of an intersection: thin
        /// vertical bars spread across the road width, with margins at the curbs.</summary>
        private static void DrawCrosswalkSouth(
            Color32[] pixels, int roadX, int roadY, int roadPx, int barPx, int bandPx, int gap, Color32 color)
        {
            int y = roadY + roadPx + gap;
            for (int x = roadX + barPx; x + barPx <= roadX + roadPx - barPx; x += barPx * 2)
            {
                FillRect(pixels, x, y, barPx, bandPx, color);
            }
        }

        /// <summary>Horizontal-road crossing just west of an intersection: thin
        /// horizontal bars spread along the road height, curbs inset.</summary>
        private static void DrawCrosswalkWest(
            Color32[] pixels, int roadX, int roadY, int roadPx, int barPx, int bandPx, int gap, Color32 color)
        {
            int x = roadX - gap - bandPx;
            for (int y = roadY + barPx; y + barPx <= roadY + roadPx - barPx; y += barPx * 2)
            {
                FillRect(pixels, x, y, bandPx, barPx, color);
            }
        }

        private static Color32 HexColor(int hex)
        {
            return new Color32(
                (byte)((hex >> 16) & 0xFF),
                (byte)((hex >> 8) & 0xFF),
                (byte)(hex & 0xFF),
                0xFF);
        }
    }
}