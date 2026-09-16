using System.Collections.Generic;
using HungryHole.Core;
using HungryHole.Gameplay;
using UnityEngine;

namespace HungryHole.Targets
{
    /// <summary>
    /// Populates World/Targets at run start from the configured TargetData list.
    /// Milestone 2 placeholder visuals are built at runtime from Unity primitives
    /// (no prefab assets required yet); real prefabs replace this in a later phase.
    /// Positions are rejection-sampled inside the playable area, keeping spacing
    /// between targets and clear distance from the hole's start position.
    /// Per App Flow section 8: an individual spawn failure is logged, not fatal.
    /// </summary>
    public class TargetSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        [Tooltip("Target categories to populate, in the reference progression order.")]
        [SerializeField] private TargetData[] targetTypes;

        [Tooltip("Container the spawned targets are parented to (World/Targets).")]
        [SerializeField] private Transform targetsParent;

        [Tooltip("Minimum spacing between spawned targets, in units.")]
        [Min(0.1f)] [SerializeField] private float minSpacing = 1.5f;

        [Tooltip("Clear radius kept free of targets around the hole start position.")]
        [Min(0f)] [SerializeField] private float holeStartClearance = 4f;

        [Tooltip("Placement attempts per target before it is skipped.")]
        [Min(1)] [SerializeField] private int maxSpawnAttemptsPerTarget = 20;

        private readonly Dictionary<TargetData, Material> materialCache = new Dictionary<TargetData, Material>();
        private readonly List<Vector2> placedPositions = new List<Vector2>();
        private readonly List<float> placedSizes = new List<float>();

        // M9 Stage 4 (target pooling): spawned target roots are recycled across
        // rounds instead of destroyed and recreated. Keyed by TargetData so a
        // reused instance always matches its category's shape, visuals and
        // score/size data. Spawn-time only — never touched per frame.
        private readonly Dictionary<TargetData, List<TargetController>> targetPool =
            new Dictionary<TargetData, List<TargetController>>();

        private Vector3 holePosition;

        private void Start()
        {
            SpawnAll();
        }

        public void SpawnAll()
        {
            materialCache.Clear();
            placedPositions.Clear();
            placedSizes.Clear();

            // M9 Stage 4: collect every existing spawned target into the reuse
            // pool (deactivating, never destroying) before repopulating. The
            // first call finds an empty world and simply builds fresh.
            RecycleSpawnedTargets();

            float half = (config != null ? config.groundSize : 240f) * 0.5f;
            float margin = config != null ? config.spawnMargin : 8f;
            float spawnHalf = Mathf.Max(0f, half - margin);

            holePosition = Vector3.zero;
            HoleController hole = FindFirstObjectByType<HoleController>();
            if (hole != null)
            {
                holePosition = hole.transform.position;
            }

            if (targetTypes == null || targetTypes.Length == 0)
            {
                Debug.LogWarning("TargetSpawner has no target types configured; world stays empty.", this);
                return;
            }

            if (targetsParent == null)
            {
                Debug.LogWarning("TargetSpawner has no Targets parent assigned; spawning at scene root.", this);
            }

            int skipped = 0;
            foreach (TargetData data in targetTypes)
            {
                if (data == null)
                {
                    skipped++;
                    continue;
                }

                for (int i = 0; i < data.spawnCount; i++)
                {
                    if (TryFindPlacement(data.maxSize, spawnHalf, out Vector3 position))
                    {
                        SpawnTarget(data, position);
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }

            if (skipped > 0)
            {
                Debug.LogWarning($"TargetSpawner skipped {skipped} targets (missing data or no free placement).", this);
            }
        }

        private bool TryFindPlacement(float size, float spawnHalf, out Vector3 position)
        {
            position = default;
            for (int attempt = 0; attempt < maxSpawnAttemptsPerTarget; attempt++)
            {
                float x = Random.Range(-spawnHalf, spawnHalf);
                float z = Random.Range(-spawnHalf, spawnHalf);
                Vector2 candidate = new Vector2(x, z);

                // Keep the hole's start position clear so nothing spawns on top of it.
                if (holeStartClearance > 0f &&
                    Vector2.Distance(candidate, new Vector2(holePosition.x, holePosition.z))
                        < holeStartClearance + size * 0.5f)
                {
                    continue;
                }

                bool tooClose = false;
                for (int j = 0; j < placedPositions.Count; j++)
                {
                    float required = minSpacing + (placedSizes[j] + size) * 0.5f;
                    if (Vector2.SqrMagnitude(candidate - placedPositions[j]) < required * required)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                {
                    continue;
                }

                position = new Vector3(x, 0f, z);
                return true;
            }

            return false;
        }

        // ----- M9 Stage 4: pooled reuse (spawn-time only, allocation-light) -----

        // Collects every existing spawned target root under the targets parent
        // (each owns a TargetController) into the reuse pool. Active instances —
        // including any still mid-eat-animation — are stopped and deactivated in
        // place; already-inactive (eaten) instances are just re-collected. Lists
        // are cleared first so repeated restarts never duplicate entries.
        private void RecycleSpawnedTargets()
        {
            foreach (List<TargetController> list in targetPool.Values)
            {
                list.Clear();
            }

            if (targetsParent == null)
            {
                // Degenerate configuration (warned about below): the targets sit
                // at the scene root, so sweep by component instead of by parent.
                // Keeps restarts reuse-only even in this layout.
                TargetController[] stray = FindObjectsByType<TargetController>(FindObjectsSortMode.None);
                foreach (TargetController target in stray)
                {
                    PoolInstance(target);
                }

                return;
            }

            int childCount = targetsParent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                TargetController target = targetsParent.GetChild(i).GetComponent<TargetController>();
                if (target != null)
                {
                    PoolInstance(target);
                }
            }
        }

        private void PoolInstance(TargetController target)
        {
            target.DeactivateForPool();

            if (target.Data == null)
            {
                return;
            }

            if (!targetPool.TryGetValue(target.Data, out List<TargetController> list))
            {
                list = new List<TargetController>();
                targetPool[target.Data] = list;
            }

            list.Add(target);
        }

        private bool TryTakeFromPool(TargetData data, out TargetController target)
        {
            target = null;
            if (!targetPool.TryGetValue(data, out List<TargetController> list) || list.Count == 0)
            {
                return false;
            }

            target = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return true;
        }

        // Reactivates a pooled instance at a fresh placement. Everything the
        // original build set that gameplay can mutate is re-derived here:
        // interactive state (ResetForReuse), active state, local position,
        // category yaw and local scale from a new random size. Child visuals
        // follow the root's scale, so the composite silhouette stays identical
        // to a fresh build of the same size.
        private void SpawnTarget(TargetData data, Vector3 position)
        {
            if (TryTakeFromPool(data, out TargetController target))
            {
                float size = Random.Range(data.minSize, data.maxSize);
                Vector3 scale = GetLocalScale(data, size);
                float halfHeight = GetHalfHeight(data, scale.y);

                target.ResetForReuse(data, size);
                target.gameObject.SetActive(true);
                target.transform.localPosition = new Vector3(position.x, halfHeight, position.z);
                TargetVisualFactory.ApplySpawnRotation(target.transform, data);
                target.transform.localScale = scale;

                // Same placement bookkeeping BuildTarget does, so spacing between
                // later placements is identical to the fresh-build layout.
                placedPositions.Add(new Vector2(position.x, position.z));
                placedSizes.Add(data.maxSize);
            }
            else
            {
                BuildTarget(data, position);
            }
        }

        private void BuildTarget(TargetData data, Vector3 position)
        {
            float size = Random.Range(data.minSize, data.maxSize);
            Vector3 scale = GetLocalScale(data, size);
            float halfHeight = GetHalfHeight(data, scale.y);

            GameObject primitive = CreatePrimitive(data.shape);
            primitive.name = data.displayName;
            primitive.transform.SetParent(targetsParent, false);
            primitive.transform.localPosition = new Vector3(position.x, halfHeight, position.z);
            primitive.transform.localScale = scale;
            primitive.GetComponent<Renderer>().sharedMaterial = GetMaterial(data);

            // M7 Stage C: pure visual children under the root; gameplay untouched.
            TargetVisualFactory.Build(primitive.transform, data, size, primitive.GetComponent<Renderer>().sharedMaterial);

            Rigidbody body = primitive.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.mass = data.mass;

            TargetController controller = primitive.AddComponent<TargetController>();
            controller.Initialize(data, size);

            placedPositions.Add(new Vector2(position.x, position.z));
            placedSizes.Add(data.maxSize);
        }

        private static GameObject CreatePrimitive(TargetData.Shape shape)
        {
            switch (shape)
            {
                case TargetData.Shape.Capsule: return GameObject.CreatePrimitive(PrimitiveType.Capsule);
                case TargetData.Shape.Cube: return GameObject.CreatePrimitive(PrimitiveType.Cube);
                case TargetData.Shape.Cylinder: return GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                default: return GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }
        }

        // Primitive reference dimensions at scale 1: sphere diameter 1, capsule
        // diameter 1 / height 2, cube 1x1x1, cylinder diameter 1 / height 2.
        private static Vector3 GetLocalScale(TargetData data, float size)
        {
            switch (data.shape)
            {
                case TargetData.Shape.Capsule:
                    return Vector3.Scale(data.relativeScale, new Vector3(size * 0.4f, size * 0.5f, size * 0.4f));
                case TargetData.Shape.Cylinder:
                    return Vector3.Scale(data.relativeScale, new Vector3(size * 0.5f, size * 0.5f, size * 0.5f));
                default:
                    return Vector3.Scale(data.relativeScale, new Vector3(size, size, size));
            }
        }

        private static float GetHalfHeight(TargetData data, float scaleY)
        {
            // Capsule and cylinder primitives are 2 units tall at scale 1.
            return data.shape == TargetData.Shape.Capsule || data.shape == TargetData.Shape.Cylinder
                ? scaleY
                : scaleY * 0.5f;
        }

        private Material GetMaterial(TargetData data)
        {
            if (materialCache.TryGetValue(data, out Material cached))
            {
                return cached;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader);
            material.color = data.color;
            materialCache[data] = material;
            return material;
        }
    }
}