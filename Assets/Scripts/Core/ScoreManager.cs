using System;
using UnityEngine;

namespace HungryHole.Core
{
    /// <summary>
    /// Raw round score (reference: score += pts per eaten target; no combo or
    /// other multipliers are ever applied). The HUD-side catch-up animation of
    /// the reference score display belongs to the UI phase.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        /// <summary>Points accumulated this round.</summary>
        public int Current { get; private set; }

        /// <summary>Raised after the score changes, with the new total.</summary>
        public event Action<int> ScoreChanged;

        public void Add(int points)
        {
            if (points <= 0)
            {
                return;
            }

            Current += points;
            ScoreChanged?.Invoke(Current);
        }

        public void Reset()
        {
            Current = 0;
            ScoreChanged?.Invoke(Current);
        }
    }
}