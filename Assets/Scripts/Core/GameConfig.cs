using UnityEngine;

namespace HungryHole.Core
{
    /// <summary>
    /// Central tuning data for Hungry Hole.
    /// Default values are taken from the original HTML/Three.js reference game
    /// (see ProjectDocs/01_PRD.md section 5 and 06_Implementation_Plan.md).
    /// Do not silently change reference values; adjust them here when balance work is requested.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Hungry Hole/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("World")]
        [Tooltip("Full width/depth of the square playable ground (reference: 240).")]
        [Min(1f)] public float groundSize = 240f;

        [Tooltip("Playable-area inset from the ground edge (reference: 8).")]
        [Min(0f)] public float spawnMargin = 8f;

        [Header("Hole Radius")]
        [Tooltip("Hole radius at the start of a run (reference START_RADIUS: 1.15).")]
        [Min(0.1f)] public float startRadius = 1.15f;

        [Tooltip("Lowest radius the hole may shrink to (reference: 1.15).")]
        [Min(0.1f)] public float minRadius = 1.15f;

        [Tooltip("Highest radius the hole may grow to (reference MAX_RADIUS: 18).")]
        [Min(0.1f)] public float maxRadius = 18f;

        [Tooltip("How quickly radius changes settle, in seconds (smooth growth animation).")]
        [Min(0.01f)] public float radiusSmoothTime = 0.25f;

        [Header("Movement")]
        [Tooltip("Movement speed at the minimum radius (reference: 12.5).")]
        [Min(0.1f)] public float moveSpeedAtMinRadius = 12.5f;

        [Tooltip("Movement speed at the maximum radius (reference: 4.0).")]
        [Min(0.1f)] public float moveSpeedAtMaxRadius = 4f;

        [Tooltip("Curve exponent applied to size progress for the speed lerp (reference: 1.3).")]
        [Min(0.1f)] public float speedSizeCurveExponent = 1.3f;

        [Tooltip("How quickly velocity approaches the desired velocity while accelerating (units/s^2).")]
        [Min(0.1f)] public float acceleration = 40f;

        [Tooltip("How quickly velocity returns to zero while decelerating (units/s^2).")]
        [Min(0.1f)] public float deceleration = 50f;

        [Header("Growth")]
        [Tooltip("Radius gained per unit of eaten target size (reference: size * 0.045).")]
        [Min(0f)] public float radiusGainPerSize = 0.045f;

        [Tooltip("Flat radius added with every eaten target (reference: 0.015).")]
        [Min(0f)] public float radiusGainFlat = 0.015f;

        [Header("Eating")]
        [Tooltip("Target is consumable when hole radius >= target size * factor (reference: 1).")]
        [Min(0.01f)] public float consumableSizeFactor = 1f;

        [Tooltip("How long the shrink-into-hole animation takes, in seconds.")]
        [Min(0.01f)] public float eatDuration = 0.25f;

        [Tooltip("Push speed applied to targets that are too large to eat (units/s).")]
        [Min(0f)] public float tooLargePushSpeed = 1.5f;

        [Header("Combo")]
        [Tooltip("Seconds after the last eaten target before the streak resets (reference: 1.1).")]
        [Min(0.01f)] public float comboWindow = 1.1f;

        [Header("Round")]
        [Tooltip("Round duration in seconds (reference: 90).")]
        [Min(1f)] public float roundDuration = 90f;

        [Header("Town")]
        [Tooltip("Town ground tile cell size in units (reference: 48).")]
        [Min(1f)] public float townCellSize = 48f;

        [Tooltip("Road strip width in units (reference: 7.5, i.e. 80/512 of the cell).")]
        [Min(0.1f)] public float townRoadWidth = 7.5f;

        [Header("Game Feel")]
        [Tooltip("Camera shake duration in seconds for heavy targets (reference: 0.24).")]
        [Min(0.01f)] public float cameraShakeTime = 0.24f;

        [Tooltip("Camera shake magnitude per unit of eaten target size (reference: 0.09).")]
        [Min(0f)] public float cameraShakeMagnitudePerSize = 0.09f;

        [Tooltip("Maximum camera shake magnitude (reference: 0.6).")]
        [Min(0f)] public float cameraShakeMaxMagnitude = 0.6f;

        [Tooltip("Base rim emissive intensity; streak boost adds on top (reference: 0.6).")]
        [Min(0f)] public float rimEmissiveIntensity = 0.6f;

        [Header("Camera Framing")]
        [Tooltip("Camera vertical field of view (reference: 54).")]
        [Min(10f)] public float cameraFov = 54f;

        [Tooltip("Camera distance at minimum radius (reference: 9.5 + radius * 1.35).")]
        [Min(1f)] public float cameraBaseDistance = 9.5f;

        [Tooltip("Extra camera distance per unit of hole radius (reference: 1.35).")]
        [Min(0f)] public float cameraDistancePerRadius = 1.35f;

        [Tooltip("Camera height at minimum radius (reference: 7.5 + radius * 1.1).")]
        [Min(1f)] public float cameraBaseHeight = 7.5f;

        [Tooltip("Extra camera height per unit of hole radius (reference: 1.1).")]
        [Min(0f)] public float cameraHeightPerRadius = 1.1f;

        [Tooltip("Horizontal fraction of the camera distance kept behind the hole (reference: 0.6).")]
        [Min(0f)] public float cameraBackFactor = 0.6f;

        [Tooltip("How far the camera lags behind hole velocity, in seconds (reference: 0.35).")]
        [Min(0f)] public float cameraVelocityLead = 0.35f;

        [Tooltip("Camera position lerp rate (reference updateCamera uses dt * 3.4).")]
        [Min(0.1f)] public float cameraFollowLerpSpeed = 3.4f;

        [Tooltip("World height the camera looks at (reference: 0.5).")]
        public float cameraLookAtHeight = 0.5f;
    }
}

