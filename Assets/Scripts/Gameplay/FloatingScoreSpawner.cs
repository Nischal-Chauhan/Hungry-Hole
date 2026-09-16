using UnityEngine;

namespace HungryHole.Gameplay
{
    /// <summary>
    /// Spawns floating score labels at eaten target positions from a fixed pool,
    /// so eating never allocates at runtime (reference: spawnFloatText with a
    /// short-lived screen-space label).
    /// </summary>
    public class FloatingScoreSpawner : MonoBehaviour
    {
        [Tooltip("Pooled labels; comfortably covers several eaten targets per second.")]
        [Min(1)] [SerializeField] private int poolSize = 24;

        private FloatingScorePopup[] pool;
        private int next;

        private void Awake()
        {
            Transform parent = new GameObject("Floating Scores").transform;
            parent.SetParent(transform, false);

            pool = new FloatingScorePopup[poolSize];
            for (int i = 0; i < pool.Length; i++)
            {
                GameObject item = new GameObject("Float Text", typeof(FloatingScorePopup));
                item.transform.SetParent(parent, false);
                pool[i] = item.GetComponent<FloatingScorePopup>();
            }
        }

        public void Spawn(Vector3 position, string text, bool big)
        {
            if (pool == null || pool.Length == 0)
            {
                return;
            }

            FloatingScorePopup popup = pool[next];
            next = (next + 1) % pool.Length;
            popup.Show(position, text, big);
        }
    }
}