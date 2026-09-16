using UnityEngine;

namespace HungryHole.Targets
{
    /// <summary>
    /// Configurable definition for one target category. Reference sizes, score values
    /// and the category list come from the original HTML/Three.js implementation
    /// (ProjectDocs/01_PRD.md section 5, ProjectDocs/06_Implementation_Plan.md phase 4).
    /// Do not silently change reference values; adjust them here when balance work is requested.
    /// </summary>
    [CreateAssetMenu(fileName = "Target_New", menuName = "Hungry Hole/Target Data")]
    public class TargetData : ScriptableObject
    {
        public enum Shape
        {
            Sphere,
            Capsule,
            Cube,
            Cylinder
        }

        [Tooltip("Stable identifier matching the reference game (apple, melon, person, bench, tree, car, house, building).")]
        public string id = "apple";

        public string displayName = "Apple";

        [Tooltip("Runtime primitive used for the Milestone 2 placeholder visual.")]
        public Shape shape = Shape.Sphere;

        [Tooltip("Per-axis scale multipliers applied on top of the resolved world size.")]
        public Vector3 relativeScale = Vector3.one;

        [Tooltip("Tint applied to the runtime primitive material.")]
        public Color color = new Color(0.85f, 0.15f, 0.15f, 1f);

        [Tooltip("Smallest spawned world size (reference value).")]
        [Min(0.05f)] public float minSize = 0.35f;

        [Tooltip("Largest spawned world size (reference value).")]
        [Min(0.05f)] public float maxSize = 0.5f;

        [Tooltip("Score awarded on consumption (consumed by the Phase 5 score system).")]
        [Min(1)] public int scoreValue = 5;

        [Tooltip("Relative resistance to the too-large push (1 = normal).")]
        [Min(0.1f)] public float mass = 1f;

        [Tooltip("How many of this category populate the world at run start.")]
        [Min(0)] public int spawnCount = 10;
    }
}