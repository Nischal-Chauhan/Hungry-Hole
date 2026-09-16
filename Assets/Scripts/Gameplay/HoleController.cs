using HungryHole.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HungryHole.Gameplay
{
    /// <summary>
    /// Moves the hole and owns its radius. Movement speed follows the reference game's
    /// size curve (faster when small, slower when large). Radius changes are clamped to
    /// config limits and animated smoothly. Eating targets arrives in Phase 3 and will
    /// call AddRadius; scoring/timer belong to later systems.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class HoleController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        [Tooltip("Child transform that holds the hole visuals; scaled with the radius.")]
        [SerializeField] private Transform visualRoot;

        private SphereCollider mouthCollider;
        private Vector3 velocity;
        private float targetRadius;
        private float displayedRadius;
        private float radiusSmoothVelocity;

        /// <summary>Radius currently used by visuals and the mouth collider.</summary>
        public float CurrentRadius => displayedRadius;

        /// <summary>Current horizontal velocity, used for camera framing.</summary>
        public Vector3 Velocity => velocity;

        /// <summary>Radius the hole is animating toward.</summary>
        public float TargetRadius => targetRadius;

        private void Awake()
        {
            mouthCollider = GetComponent<SphereCollider>();
            displayedRadius = targetRadius = config != null ? config.startRadius : 1.15f;
            ApplyRadius(displayedRadius);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            UpdateMovement(dt);
            UpdateRadius(dt);
        }

        private void UpdateMovement(float dt)
        {
            Vector2 input = ReadMoveInput();

            // Reference: speed = lerp(12.5, 4.0, pow(sizeT, 1.3)) with
            // sizeT = progress from min to max radius.
            float sizeT = 0f;
            if (config != null && config.maxRadius > config.minRadius)
            {
                sizeT = Mathf.Clamp01(
                    (targetRadius - config.minRadius) / (config.maxRadius - config.minRadius));
            }

            float speedMin = config != null ? config.moveSpeedAtMinRadius : 12.5f;
            float speedMax = config != null ? config.moveSpeedAtMaxRadius : 4f;
            float exponent = config != null ? config.speedSizeCurveExponent : 1.3f;
            float speed = Mathf.Lerp(speedMin, speedMax, Mathf.Pow(sizeT, exponent));

            Vector3 desiredVelocity = new Vector3(input.x, 0f, input.y) * speed;
            float rate = input.sqrMagnitude > 0.0001f
                ? (config != null ? config.acceleration : 40f)
                : (config != null ? config.deceleration : 50f);
            velocity = Vector3.MoveTowards(velocity, desiredVelocity, rate * dt);

            Vector3 position = transform.position + velocity * dt;
            position = ClampToGround(position);
            transform.position = position;
        }

        private Vector3 ClampToGround(Vector3 position)
        {
            float half = config != null ? config.groundSize * 0.5f : 120f;
            float margin = config != null ? config.spawnMargin : 8f;
            float limit = half - margin;
            position.x = Mathf.Clamp(position.x, -limit, limit);
            position.z = Mathf.Clamp(position.z, -limit, limit);
            return position;
        }

        // W/Up moves the hole away from the camera; the camera is on the -Z side,
        // so forward is +Z (visual parity port of the reference's -Z forward).
        private static Vector2 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float x = 0f;
            float z = 0f;
            // Visual parity with the reference game: A/Left = screen left (-X),
            // D/Right = screen right (+X), W/Up = forward/away from the camera,
            // S/Down = back toward the camera. The camera sits on the hole's -Z side
            // (see CameraFollow), so "away from the camera" is +Z in world space.
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;

            Vector2 input = new Vector2(x, z);
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void UpdateRadius(float dt)
        {
            if (Mathf.Approximately(displayedRadius, targetRadius))
            {
                return;
            }

            float smoothTime = config != null ? config.radiusSmoothTime : 0.25f;
            displayedRadius = Mathf.SmoothDamp(
                displayedRadius, targetRadius, ref radiusSmoothVelocity, smoothTime, Mathf.Infinity, dt);
            ApplyRadius(displayedRadius);
        }

        private void ApplyRadius(float radius)
        {
            if (mouthCollider != null)
            {
                mouthCollider.radius = radius;
            }

            if (visualRoot != null)
            {
                visualRoot.localScale = new Vector3(radius, 1f, radius);
            }
        }

        /// <summary>
        /// Grows the hole by the given amount, clamped to the configured radius limits.
        /// Intended entry point for the Phase 3 eating system
        /// (reference growth: radius += size * 0.045 + 0.015).
        /// </summary>
        public void AddRadius(float amount)
        {
            SetRadius(targetRadius + amount);
        }

        /// <summary>Sets the radius the hole will animate toward, clamped to configured limits.</summary>
        public void SetRadius(float radius)
        {
            float min = config != null ? config.minRadius : 0.1f;
            float max = config != null ? config.maxRadius : 18f;
            targetRadius = Mathf.Clamp(radius, min, max);
        }

        /// <summary>
        /// M6 lifecycle support (GameManager.RestartRound): restores a fresh-run
        /// state instantly — authored position, starting radius, zero velocity.
        /// No verified movement/growth/framing behavior is touched; this only
        /// runs on explicit restarts from the menu/restart flow.
        /// </summary>
        public void ResetToStart(Vector3 position)
        {
            velocity = Vector3.zero;
            radiusSmoothVelocity = 0f;
            displayedRadius = targetRadius = config != null ? config.startRadius : 1.15f;
            transform.position = position;
            ApplyRadius(displayedRadius);
        }
    }
}

