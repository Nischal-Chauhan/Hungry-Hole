using System;
using UnityEngine;

namespace HungryHole.Core
{
    /// <summary>
    /// Round countdown (reference: GAME_LENGTH = 90; timeLeft decremented only
    /// while the run is running; zero clamps to 0 and triggers endGame).
    /// </summary>
    public class RoundTimer : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        /// <summary>Seconds remaining; 0 once the round has ended.</summary>
        public float TimeLeft { get; private set; }

        /// <summary>True while the countdown is active.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>Raised once when the timer reaches zero.</summary>
        public event Action TimeUp;

        public void StartCountdown()
        {
            TimeLeft = config != null ? config.roundDuration : 90f;
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                IsRunning = false;
                TimeUp?.Invoke();
            }
        }
    }
}