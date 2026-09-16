using HungryHole.CameraSystem;
using HungryHole.Core;
using HungryHole.Targets;
using UnityEngine;

namespace HungryHole.Gameplay
{
    /// <summary>
    /// Consumes targets under the hole's mouth and pushes away targets that are too
    /// large to eat. Detection samples the mouth volume each physics step with
    /// OverlapSphereNonAlloc so it follows the hole's animated radius without touching
    /// the Hole prefab. Growth follows the reference formula
    /// radius += size * 0.045 + 0.015 via HoleController.AddRadius.
    /// </summary>
    public class HoleEating : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        [Tooltip("Hole whose mouth consumes targets. Left empty, the first HoleController in the scene is used.")]
        [SerializeField] private HoleController hole;

        [Tooltip("Collider results sampled around the mouth per physics step.")]
        [Min(16)] [SerializeField] private int maxOverlapResults = 128;

        [Tooltip("Score owner; awards TargetData.scoreValue for each eaten target.")]
        [SerializeField] private ScoreManager score;

        [Tooltip("Combo/streak state; advanced on each eaten target.")]
        [SerializeField] private ComboManager combo;

        [Tooltip("Pooled floating score labels. Left empty, the scene spawner is used.")]
        [SerializeField] private FloatingScoreSpawner floatingScore;

        [Tooltip("Camera that shakes on heavy eats. Left empty, the first CameraFollow is used.")]
        [SerializeField] private CameraFollow cameraFollow;

        private Collider[] overlapBuffer;

        private void Awake()
        {
            overlapBuffer = new Collider[maxOverlapResults];
        }

        private void Start()
        {
            if (hole == null)
            {
                hole = FindFirstObjectByType<HoleController>();
            }

            if (score == null) score = FindFirstObjectByType<ScoreManager>();
            if (combo == null) combo = FindFirstObjectByType<ComboManager>();
            if (floatingScore == null) floatingScore = FindFirstObjectByType<FloatingScoreSpawner>();
            if (cameraFollow == null) cameraFollow = FindFirstObjectByType<CameraFollow>();
        }

        private void FixedUpdate()
        {
            if (hole == null)
            {
                return;
            }

            Vector3 center = hole.transform.position;
            float radius = hole.CurrentRadius;
            float sizeFactor = config != null ? config.consumableSizeFactor : 1f;
            float growPerSize = config != null ? config.radiusGainPerSize : 0.045f;
            float growFlat = config != null ? config.radiusGainFlat : 0.015f;
            float eatDuration = config != null ? config.eatDuration : 0.25f;

            int count = Physics.OverlapSphereNonAlloc(
                center, radius, overlapBuffer, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                // M9 Stage 3: targets are single GameObjects — the root collider,
                // its kinematic Rigidbody and the TargetController all sit on the
                // same root (visual children carry no colliders), so the collider's
                // attachedRigidbody identifies the target without a hierarchy walk.
                // Colliders with no body (ground/scene/static) are skipped at once.
                Rigidbody body = overlapBuffer[i].attachedRigidbody;
                if (body == null || !body.TryGetComponent(out TargetController target) || target.IsConsumed)
                {
                    continue;
                }

                if (target.CanBeConsumedBy(radius, sizeFactor))
                {
                    target.Consume(center, eatDuration);
                    hole.AddRadius(target.Size * growPerSize + growFlat);

                    // Reference eatObject: score += pts, comboCount++ with a
                    // restarted 1.1 s window, and a floating '+pts' label at the
                    // eaten position (big style for heavy targets, size >= 2).
                    int points = target.Data != null ? target.Data.scoreValue : 0;
                    if (points > 0 && score != null)
                    {
                        score.Add(points);
                    }

                    if (combo != null)
                    {
                        combo.RegisterEat();
                    }

                    if (floatingScore != null)
                    {
                        floatingScore.Spawn(
                            target.transform.position + Vector3.up * (target.Size * 0.4f),
                            "+" + points,
                            target.Size >= 2f);
                    }

                    // Reference eatObject: only heavy targets (size >= 2) shake the
                    // camera, with magnitude proportional to the eaten size.
                    if (target.Size >= 2f && cameraFollow != null)
                    {
                        cameraFollow.TriggerShake(target.Size);
                    }
                }
                else
                {
                    PushTarget(target, center);
                }
            }
        }

        // Reference behavior: targets too large for the hole are not consumed; the
        // hole nudges them aside so it can keep moving past them.
        private void PushTarget(TargetController target, Vector3 center)
        {
            Rigidbody body = target.Body;
            if (body == null || target.Data == null)
            {
                return;
            }

            Vector3 direction = target.transform.position - center;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            }
            direction.Normalize();

            float pushSpeed = config != null ? config.tooLargePushSpeed : 1.5f;
            float step = pushSpeed * Time.deltaTime / Mathf.Max(0.1f, target.Data.mass);

            Vector3 next = body.position + direction * step;
            float half = (config != null ? config.groundSize : 240f) * 0.5f;
            float margin = config != null ? config.spawnMargin : 8f;
            float limit = half - margin;
            next.x = Mathf.Clamp(next.x, -limit, limit);
            next.z = Mathf.Clamp(next.z, -limit, limit);
            body.MovePosition(next);
        }
    }
}