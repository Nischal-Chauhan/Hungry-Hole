using UnityEngine;

namespace HungryHole.Targets
{
    /// <summary>
    /// M7 Stage C: builds the stylized low-poly visual children under each spawned
    /// target root. The root primitive stays the gameplay object — its collider,
    /// TargetController, rigidbody, position and eat shrink-and-sink animation are
    /// untouched; children are pure visuals (their auto-created colliders are
    /// destroyed immediately, they carry no scripts or physics). Child sizes are
    /// given in WORLD units and compensated against the root's (possibly
    /// non-uniform) scale, so silhouettes keep the TargetData size hierarchy and
    /// the root scaling controls the complete composite during eating.
    /// All detail materials/textures are shared and created once per session —
    /// no per-target material instances, no per-frame work, no asset files.
    /// </summary>
    public static class TargetVisualFactory
    {
        private static Material darkDetail;
        private static Material woodBrown;
        private static Material foliageGreen;
        private static Material windowGlass;
        private static Material buildingRoof;
        private static Material melonStripes;
        private static Texture2D melonTexture;
        private static Material buildingWindows;
        private static Texture2D buildingTexture;

        /// <summary>
        /// Adds the category's visual children under the target root. Unknown ids
        /// (or future categories) simply keep the plain root primitive.
        /// </summary>
        public static void Build(Transform root, TargetData data, float size, Material categoryMaterial)
        {
            switch (data != null ? data.id : null)
            {
                case "apple": BuildApple(root, size); break;
                case "melon": root.GetComponent<MeshRenderer>().sharedMaterial = GetMelonMaterial(categoryMaterial.color); break;
                case "person": BuildCitizen(root, size, categoryMaterial); break;
                case "bench": BuildBench(root, data, size, categoryMaterial); break;
                case "tree": BuildTree(root, size, categoryMaterial); break;
                case "car": BuildCar(root, data, size, categoryMaterial); break;
                case "house": BuildHouse(root, data, size, categoryMaterial); break;
                case "building": BuildBuilding(root, size, categoryMaterial.color); break;
            }
        }

        // Reference silhouettes. s = resolved world size; all child sizes/offsets
        // are fractions of the existing root footprint so the hierarchy is preserved.

        private static void BuildApple(Transform root, float s)
        {
            AddChild(root, PrimitiveType.Cylinder, "Stem",
                new Vector3(0.05f * s, 0.14f * s, 0.05f * s), new Vector3(0f, 0.53f * s, 0f), GetWood(), false);
            AddChild(root, PrimitiveType.Cube, "Leaf",
                new Vector3(0.16f * s, 0.03f * s, 0.08f * s), new Vector3(0.08f * s, 0.55f * s, 0f), GetFoliage(), false,
                new Vector3(0f, 0f, 25f));
        }

        private static void BuildCitizen(Transform root, float s, Material material)
        {
            AddChild(root, PrimitiveType.Sphere, "Head",
                new Vector3(0.32f * s, 0.32f * s, 0.32f * s), new Vector3(0f, 0.66f * s, 0f), material, true);
            AddChild(root, PrimitiveType.Sphere, "Hair",
                new Vector3(0.36f * s, 0.18f * s, 0.36f * s), new Vector3(0f, 0.78f * s, 0f), GetDark(), false);
        }

        /// <summary>
        /// Applies the category's spawn-time root yaw. Bench/car/house get a
        /// random full-turn yaw at every placement (fresh build and pooled reuse
        /// alike); all other categories keep the primitive's identity rotation.
        /// </summary>
        public static void ApplySpawnRotation(Transform root, TargetData data)
        {
            switch (data != null ? data.id : null)
            {
                case "bench":
                case "car":
                case "house":
                    root.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    break;
            }
        }

        private static void BuildBench(Transform root, TargetData data, float s, Material material)
        {
            ApplySpawnRotation(root, data);
            AddChild(root, PrimitiveType.Cube, "Backrest",
                new Vector3(1.5f * s, 0.5f * s, 0.12f * s), new Vector3(0f, 0.4f * s, -0.24f * s), material, true);
            AddChild(root, PrimitiveType.Cube, "EndPanelLeft",
                new Vector3(0.16f * s, 0.3f * s, 0.55f * s), new Vector3(-0.63f * s, 0f, 0f), material, true);
            AddChild(root, PrimitiveType.Cube, "EndPanelRight",
                new Vector3(0.16f * s, 0.3f * s, 0.55f * s), new Vector3(0.63f * s, 0f, 0f), material, true);
        }

        private static void BuildTree(Transform root, float s, Material material)
        {
            // The root cylinder becomes the trunk (spans 0..s above ground); a
            // narrow stacked crown keeps the pine silhouette close to the root
            // footprint — only slightly wider than the trunk, ~1.2s at the top.
            root.GetComponent<MeshRenderer>().sharedMaterial = GetWood();
            AddChild(root, PrimitiveType.Sphere, "FoliageLower",
                new Vector3(0.62f * s, 0.55f * s, 0.62f * s), new Vector3(0f, 0.33f * s, 0f), material, true);
            AddChild(root, PrimitiveType.Sphere, "FoliageUpper",
                new Vector3(0.42f * s, 0.4f * s, 0.42f * s), new Vector3(0f, 0.58f * s, 0f), material, true);
        }

        private static void BuildCar(Transform root, TargetData data, float s, Material material)
        {
            ApplySpawnRotation(root, data);
            AddChild(root, PrimitiveType.Cube, "Cabin",
                new Vector3(0.42f * s, 0.26f * s, 0.45f * s), new Vector3(0f, 0.3f * s, -0.08f * s), material, true);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? 1f : -1f) * 0.26f * s;
                float z = (i < 2 ? 1f : -1f) * 0.32f * s;
                // Sizes are in the wheel's pre-rotation frame: the cylinder's long
                // axis (its local Y) becomes the axle width after the 90-degree
                // Z rotation, so the width is passed as the Y extent.
                AddChild(root, PrimitiveType.Cylinder, "Wheel",
                    new Vector3(0.22f * s, 0.1f * s, 0.22f * s), new Vector3(x, -0.065f * s, z), GetDark(), false,
                    new Vector3(0f, 0f, 90f));
            }
        }

        private static void BuildHouse(Transform root, TargetData data, float s, Material material)
        {
            ApplySpawnRotation(root, data);
            AddChild(root, PrimitiveType.Cube, "Roof",
                new Vector3(1.0f * s, 0.55f * s, 1.06f * s), new Vector3(0f, 0.375f * s, 0f), material, true,
                new Vector3(0f, 0f, 45f));
            AddChild(root, PrimitiveType.Cube, "Door",
                new Vector3(0.18f * s, 0.28f * s, 0.05f * s), new Vector3(0f, -0.235f * s, 0.51f * s), GetDark(), false);
            AddChild(root, PrimitiveType.Cube, "WindowLeft",
                new Vector3(0.15f * s, 0.15f * s, 0.05f * s), new Vector3(-0.25f * s, 0.02f * s, 0.51f * s), GetGlass(), false);
            AddChild(root, PrimitiveType.Cube, "WindowRight",
                new Vector3(0.15f * s, 0.15f * s, 0.05f * s), new Vector3(0.25f * s, 0.02f * s, 0.51f * s), GetGlass(), false);
        }

        private static void BuildBuilding(Transform root, float s, Color baseColor)
        {
            // Window pattern is a baked texture on the shared tower material; the
            // small roof cap hides the textured top face from the top-down camera.
            root.GetComponent<MeshRenderer>().sharedMaterial = GetBuildingMaterial(baseColor);
            AddChild(root, PrimitiveType.Cube, "RoofCap",
                new Vector3(0.62f * s, 0.05f * s, 0.62f * s), new Vector3(0f, 0.525f * s, 0f), GetBuildingRoof(), false);
        }

        // ----- Shared detail children -----

        private static GameObject AddChild(
            Transform root, PrimitiveType type, string name,
            Vector3 worldSize, Vector3 worldCenter, Material material, bool castShadows, Vector3 localEuler = default)
        {
            GameObject child = GameObject.CreatePrimitive(type);
            child.name = name;

            // Visual-only: no colliders, no physics on children.
            Collider visualCollider = child.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Object.Destroy(visualCollider);
            }

            child.transform.SetParent(root, false);
            Vector3 rootScale = root.localScale;
            bool tallPrimitive = type == PrimitiveType.Cylinder || type == PrimitiveType.Capsule;
            child.transform.localScale = tallPrimitive
                ? new Vector3(worldSize.x / rootScale.x, worldSize.y / (2f * rootScale.y), worldSize.z / rootScale.z)
                : new Vector3(worldSize.x / rootScale.x, worldSize.y / rootScale.y, worldSize.z / rootScale.z);
            child.transform.localPosition = new Vector3(
                worldCenter.x / rootScale.x, worldCenter.y / rootScale.y, worldCenter.z / rootScale.z);
            if (localEuler != default)
            {
                child.transform.localRotation = Quaternion.Euler(localEuler);
            }

            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            if (!castShadows)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            return child;
        }

        // ----- Shared materials/textures (created once per session) -----

        private static Material GetDark()
        {
            if (darkDetail == null)
            {
                darkDetail = CreateLit(new Color(0.15f, 0.16f, 0.19f), "TargetDarkDetail");
            }

            return darkDetail;
        }

        private static Material GetWood()
        {
            if (woodBrown == null)
            {
                woodBrown = CreateLit(new Color(0.38f, 0.26f, 0.15f), "TargetWood");
            }

            return woodBrown;
        }

        private static Material GetFoliage()
        {
            if (foliageGreen == null)
            {
                foliageGreen = CreateLit(new Color(0.2f, 0.55f, 0.25f), "TargetFoliage");
            }

            return foliageGreen;
        }

        private static Material GetGlass()
        {
            if (windowGlass == null)
            {
                windowGlass = CreateLit(new Color(0.9f, 0.86f, 0.68f), "TargetWindowGlass");
            }

            return windowGlass;
        }

        private static Material GetBuildingRoof()
        {
            if (buildingRoof == null)
            {
                buildingRoof = CreateLit(new Color(0.25f, 0.28f, 0.33f), "TargetBuildingRoof");
            }

            return buildingRoof;
        }

        private static Material GetMelonMaterial(Color baseColor)
        {
            if (melonStripes == null)
            {
                melonTexture = CreateMelonTexture(baseColor);
                melonStripes = CreateLit(Color.white, "TargetMelonStripes");
                melonStripes.mainTexture = melonTexture;
            }

            return melonStripes;
        }

        private static Material GetBuildingMaterial(Color baseColor)
        {
            if (buildingWindows == null)
            {
                buildingTexture = CreateBuildingTexture(baseColor);
                buildingWindows = CreateLit(Color.white, "TargetBuildingWindows");
                buildingWindows.mainTexture = buildingTexture;
            }

            return buildingWindows;
        }

        private static Material CreateLit(Color color, string name)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = name;
            material.color = color;
            return material;
        }

        // 64x64 vertical stripes on the melon's green base — sphere UVs turn the
        // vertical bands into watermelon meridian stripes.
        private static Texture2D CreateMelonTexture(Color baseColor)
        {
            const int size = 64;
            Color32 stripeColor = (Color32)(baseColor * 0.65f);
            Color32 baseTint = baseColor;
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = (x / 4) % 2 == 1 ? stripeColor : baseTint;
                }
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "TargetMelonStripes",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }

        // 64x64 window grid (6 columns x 8 rows of lighter windows) baked over the
        // tower base color; every cube face shows one grid.
        private static Texture2D CreateBuildingTexture(Color baseColor)
        {
            const int size = 64;
            Color32 windowColor = new Color32(228, 216, 160, 255);
            Color32 baseTint = baseColor;
            Color32[] pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = baseTint;
            }

            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 6; column++)
                {
                    int startX = column * 10 + 3;
                    int startY = row * 8 + 2;
                    for (int y = startY; y < startY + 4 && y < size; y++)
                    {
                        for (int x = startX; x < startX + 6 && x < size; x++)
                        {
                            pixels[y * size + x] = windowColor;
                        }
                    }
                }
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "TargetBuildingWindows",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }
    }
}