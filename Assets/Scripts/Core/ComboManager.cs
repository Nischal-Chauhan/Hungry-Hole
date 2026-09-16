using UnityEngine;

namespace HungryHole.Core
{
    /// <summary>
    /// Combo/streak state (reference: comboCount/comboTimer). Every eaten target
    /// increments the count and restarts the 1.1 s window; when the window
    /// expires the streak resets to 0. The streak never touches the score — the
    /// reference uses it only for the rim emissive glow factor (StreakBoost),
    /// which the later game-feel/polish phases apply to the rim material.
    /// </summary>
    public class ComboManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        /// <summary>Targets eaten within the current streak window.</summary>
        public int Count { get; private set; }

        /// <summary>Seconds left in the current streak window.</summary>
        public float WindowTimeLeft { get; private set; }

        /// <summary>
        /// Reference streakBoost: 0 for single eats, otherwise
        /// min(count, 12) * 0.15 * (fraction of the window still remaining).
        /// Exposed for the later rim-glow work; unused in this milestone.
        /// </summary>
        public float StreakBoost
        {
            get
            {
                if (Count <= 1)
                {
                    return 0f;
                }

                float window = config != null ? config.comboWindow : 1.1f;
                return Mathf.Min(Count, 12) * 0.15f * Mathf.Max(0f, WindowTimeLeft / window);
            }
        }

        public void RegisterEat()
        {
            Count++;
            WindowTimeLeft = config != null ? config.comboWindow : 1.1f;
        }

        public void Reset()
        {
            Count = 0;
            WindowTimeLeft = 0f;
        }

        private void Update()
        {
            if (WindowTimeLeft > 0f)
            {
                WindowTimeLeft -= Time.deltaTime;
                if (WindowTimeLeft <= 0f)
                {
                    WindowTimeLeft = 0f;
                    Count = 0;
                }
            }
        }
    }
}