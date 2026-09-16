using HungryHole.Core;
using HungryHole.Gameplay;
using UnityEngine;

namespace HungryHole.CameraSystem
{
    /// <summary>
    /// Smoothly follows the hole using the reference game's dynamic framing:
    /// distance and height grow with the hole radius, the camera lags slightly
    /// behind the hole's velocity, and the view stays centered on the hole.
    /// Reference updateCamera: dist = 9.5 + r * 1.35, height = 7.5 + r * 1.1,
    /// position lerp rate dt * 3.4, lookAt (hole.x, 0.5, hole.z).
    /// The camera sits on the hole's -Z side: Unity's left-handed projection mirrors
    /// the Three.js reference's +Z view, so the camera side is flipped to keep the
    /// reference's on-screen orientation (W/forward away from the camera, +X right).
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        [Tooltip("Hole to follow. Left empty, the first HoleController in the scene is used.")]
        [SerializeField] private HoleController hole;

        private Camera cam;

        // Reference camShake state: only heavy eats trigger it; the magnitude decays
        // linearly over the shake duration and is applied after the framing lerp,
        // before lookAt, so the verified framing math is unchanged.
        private float shakeTime;
        private float shakeMagnitude;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (hole == null)
            {
                hole = FindFirstObjectByType<HoleController>();
            }

            if (cam != null && config != null)
            {
                cam.fieldOfView = config.cameraFov;
            }

            // Snap to the reference framing immediately so the rotation quaternion
            // is valid from the first rendered frame.
            ApplyFraming(true);
        }

        private void LateUpdate()
        {
            ApplyFraming(false);
        }

        private void ApplyFraming(bool snap)
        {
            if (config == null)
            {
                return;
            }

            Vector3 holePosition;
            Vector3 holeVelocity;
            float radius;
            if (hole != null)
            {
                holePosition = hole.transform.position;
                holeVelocity = hole.Velocity;
                radius = hole.CurrentRadius;
            }
            else
            {
                // No hole available (e.g. scene setup issue): hold the start framing
                // at the world origin instead of leaving an uncontrolled camera.
                holePosition = Vector3.zero;
                holeVelocity = Vector3.zero;
                radius = config.startRadius;
            }

            float distance = config.cameraBaseDistance + radius * config.cameraDistancePerRadius;
            float height = config.cameraBaseHeight + radius * config.cameraHeightPerRadius;

            // Velocity lead always widens the camera-to-hole gap along the camera's
            // -Z axis, so backward (S/Down, -Z) travel gets the same pull-back
            // framing response as forward (W/Up, +Z) travel. |vel.z| == vel.z for
            // +Z motion, so the verified W/Up framing math is bit-identical.
            Vector3 desired = new Vector3(
                holePosition.x - holeVelocity.x * config.cameraVelocityLead,
                height,
                holePosition.z - Mathf.Abs(holeVelocity.z) * config.cameraVelocityLead
                    - distance * config.cameraBackFactor);

            if (snap)
            {
                transform.position = desired;
            }
            else
            {
                // Reference: position lerp factor min(1, dt * 3.4).
                float t = 1f - Mathf.Exp(-config.cameraFollowLerpSpeed * Time.deltaTime);
                transform.position = Vector3.Lerp(transform.position, desired, t);
            }

            if (shakeTime > 0f)
            {
                shakeTime -= Time.deltaTime;
                float decay = Mathf.Max(0f, shakeTime / config.cameraShakeTime);
                float magnitude = shakeMagnitude * decay;
                transform.position += new Vector3(
                    Random.Range(-0.5f, 0.5f) * magnitude,
                    Random.Range(-0.5f, 0.5f) * magnitude * 0.6f,
                    Random.Range(-0.5f, 0.5f) * magnitude);
            }

            Vector3 lookTarget = new Vector3(
                holePosition.x, config.cameraLookAtHeight, holePosition.z);
            transform.LookAt(lookTarget);
        }

        /// <summary>
        /// Reference eatObject heavy shake: time 0.24 s, magnitude min(0.6, size * 0.09).
        /// </summary>
        public void TriggerShake(float eatenSize)
        {
            shakeTime = config != null ? config.cameraShakeTime : 0.24f;
            float perSize = config != null ? config.cameraShakeMagnitudePerSize : 0.09f;
            float max = config != null ? config.cameraShakeMaxMagnitude : 0.6f;
            shakeMagnitude = Mathf.Min(max, eatenSize * perSize);
        }
    }
}

