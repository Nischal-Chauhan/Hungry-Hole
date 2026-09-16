using System.Collections;
using UnityEngine;

namespace HungryHole.Targets
{
    /// <summary>
    /// One spawned target instance. Holds the resolved world size and consumption
    /// state; the hole decides eligibility, this component plays the eat animation
    /// and recycles itself (deactivated in place, ready for pooling).
    /// </summary>
    [DisallowMultipleComponent]
    public class TargetController : MonoBehaviour
    {
        [SerializeField] private TargetData data;

        /// <summary>Definition this instance was spawned from.</summary>
        public TargetData Data => data;

        /// <summary>Resolved world size of this instance (random within the data range).</summary>
        public float Size { get; private set; }

        /// <summary>True once consumption has started; the instance is no longer interactive.</summary>
        public bool IsConsumed { get; private set; }

        /// <summary>Kinematic body used by the too-large push behavior.</summary>
        public Rigidbody Body { get; private set; }

        private Collider targetCollider;

        public void Initialize(TargetData source, float size)
        {
            data = source;
            Size = size;
            IsConsumed = false;
            targetCollider = GetComponent<Collider>();
            Body = GetComponent<Rigidbody>();
        }

        // M9 Stage 4 (pooled reuse): a restart can interrupt the eat animation
        // and Consume leaves the collider disabled, so a reused instance is put
        // back to a clean interactive state before the spawner repositions it.
        // Only actual runtime-mutated state is touched; visual children are
        // static and follow the root, exactly as during the eat animation.
        public void ResetForReuse(TargetData source, float size)
        {
            StopAllCoroutines();
            Initialize(source, size);

            if (targetCollider != null)
            {
                targetCollider.enabled = true;
            }

            if (Body != null)
            {
                Body.linearVelocity = Vector3.zero;
                Body.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Pool-side half of the reuse contract: stop any running eat routine
        /// and deactivate in place (the existing recycle design) so the spawner
        /// can reactivate this instance on a later round. Never destroys.
        /// </summary>
        public void DeactivateForPool()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Reference eligibility: the hole can eat a target once its radius reaches
        /// the target's world size (multiplied by the configurable size factor).
        /// </summary>
        public bool CanBeConsumedBy(float holeRadius, float sizeFactor)
        {
            return !IsConsumed && holeRadius >= Size * sizeFactor;
        }

        /// <summary>
        /// Plays the eat animation (shrink and sink toward the hole) then deactivates
        /// the instance. It stays pooled under World/Targets.
        /// </summary>
        public void Consume(Vector3 holePosition, float duration)
        {
            if (IsConsumed)
            {
                return;
            }

            IsConsumed = true;
            if (targetCollider != null)
            {
                targetCollider.enabled = false;
            }

            StartCoroutine(EatRoutine(holePosition, duration));
        }

        private IEnumerator EatRoutine(Vector3 holePosition, float duration)
        {
            Vector3 startPosition = transform.position;
            Vector3 startScale = transform.localScale;
            Vector3 sunkTarget = new Vector3(startPosition.x, -startScale.y, startPosition.z);

            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.MoveTowards(t, 1f, Time.deltaTime / Mathf.Max(0.01f, duration));
                // Quadratic ease-in: slow at first, then quickly swallowed.
                float eased = t * t;
                transform.localScale = startScale * (1f - eased);
                transform.position = Vector3.Lerp(startPosition, sunkTarget, eased);
                yield return null;
            }

            transform.localScale = startScale; // restored so a pooled reuse starts clean
            transform.position = startPosition;
            gameObject.SetActive(false);
        }
    }
}