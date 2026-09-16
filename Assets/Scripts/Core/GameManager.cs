using System;
using HungryHole.Gameplay;
using HungryHole.Targets;
using UnityEngine;

namespace HungryHole.Core
{
    /// <summary>
    /// Round lifecycle for the arcade loop (Phase 8 / reference boot-startGame flow).
    /// The scene boots to the main menu: no run starts until the UI phase's
    /// UIManager calls one of the start methods. On round start the score/combo
    /// are reset, the timer restarts and Time.timeScale is explicitly restored to 1.
    /// On timer zero gameplay stops and the results screen arrives via RoundEnded.
    /// Pause freezes gameplay with Time.timeScale = 0; resume/menu return always
    /// restore normal time so no gameplay runs behind the menu or overlays.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        [Tooltip("Score, combo and timer managers beside this component on the same object.")]
        [SerializeField] private ScoreManager score;
        [SerializeField] private ComboManager combo;
        [SerializeField] private RoundTimer timer;

        [Tooltip("World population; a restart re-runs SpawnAll, which recycles the spawned targets through its reuse pool.")]
        [SerializeField] private TargetSpawner targets;

        /// <summary>True between round start and the timer reaching zero.</summary>
        public bool RoundRunning { get; private set; }

        /// <summary>True while the round is paused (Time.timeScale = 0).</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Shared gameplay configuration.</summary>
        public GameConfig Config => config;

        /// <summary>Raised when a run starts.</summary>
        public event Action RoundStarted;

        /// <summary>Raised once when the round timer reaches zero.</summary>
        public event Action RoundEnded;

        private HoleEating eating;
        private HoleController hole;
        private Vector3 holeStartPosition;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogWarning("GameManager has no GameConfig assigned.", this);
            }

            // Score/combo/timer are usually wired in the scene; the hole-side
            // systems live on the prefab, so those are looked up.
            if (score == null) score = FindFirstObjectByType<ScoreManager>();
            if (combo == null) combo = FindFirstObjectByType<ComboManager>();
            if (timer == null) timer = FindFirstObjectByType<RoundTimer>();
            hole = FindFirstObjectByType<HoleController>();
            eating = FindFirstObjectByType<HoleEating>();
            if (targets == null) targets = FindFirstObjectByType<TargetSpawner>();

            // Authored hole position, reused for every fresh run (M6 restarts).
            holeStartPosition = hole != null ? hole.transform.position : Vector3.zero;

            if (timer != null)
            {
                timer.TimeUp += EndRound;
            }
        }

        private void OnDestroy()
        {
            if (timer != null)
            {
                timer.TimeUp -= EndRound;
            }
        }

        private void Start()
        {
            // M6 menu-first: the scene no longer auto-starts. Gameplay systems
            // stay disabled while the main menu or the game over screen shows;
            // UIManager starts runs explicitly through RestartRound/StartRound.
            if (eating != null) eating.enabled = false;
            if (hole != null) hole.enabled = false;
        }

        /// <summary>
        /// Reference startGame: resets score/combo, restarts the timer and
        /// marks the run active. World population stays with TargetSpawner.
        /// </summary>
        public void StartRound()
        {
            // Lifecycle safeguard: a fresh run always resumes normal time.
            Time.timeScale = 1f;
            IsPaused = false;

            if (score != null) score.Reset();
            if (combo != null) combo.Reset();
            if (timer != null) timer.StartCountdown();

            if (eating != null) eating.enabled = true;
            if (hole != null) hole.enabled = true;

            RoundRunning = true;
            RoundStarted?.Invoke();
        }

        /// <summary>
        /// Reference endGame: stops the run when the timer reaches zero. The
        /// results screen/replay belong to the UI phase (Phase 8).
        /// </summary>
        public void EndRound()
        {
            if (!RoundRunning)
            {
                return;
            }

            RoundRunning = false;
            IsPaused = false;

            // The round ended naturally, but make sure no paused state lingers.
            Time.timeScale = 1f;

            if (eating != null) eating.enabled = false;
            if (hole != null) hole.enabled = false;

            int finalScore = score != null ? score.Current : 0;
            Debug.Log($"[Round] Timer reached zero — run ended. Final score: {finalScore}", this);
            RoundEnded?.Invoke();
        }

        /// <summary>
        /// Full reset for the M6 restart/replay path: returns every spawned
        /// target to the spawner's reuse pool (M9 Stage 4 — no destroy/recreate),
        /// restores the hole to its authored start state, re-populates
        /// the world and then runs the normal round start (which restores
        /// Time.timeScale to 1).
        /// </summary>
        public void RestartRound()
        {
            if (hole != null)
            {
                hole.ResetToStart(holeStartPosition);
            }

            if (targets != null)
            {
                targets.SpawnAll();
            }

            StartRound();
        }

        /// <summary>
        /// Pauses the active run with Time.timeScale = 0. RoundTimer and all
        /// gameplay Update loops read Time.deltaTime, so everything freezes
        /// without any gameplay-code changes. No-op when no round is running.
        /// </summary>
        public void Pause()
        {
            if (!RoundRunning || IsPaused)
            {
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Returns to the exact paused state: Time.timeScale = 1 and the run
        /// keeps its score/combo/timer/hole state unchanged.
        /// </summary>
        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Abandons the current run and returns to the menu state: gameplay
        /// systems are disabled (same path EndRound uses), the timer stops and
        /// Time.timeScale is restored to 1 so nothing runs behind the menu.
        /// </summary>
        public void ReturnToMenu()
        {
            RoundRunning = false;
            IsPaused = false;
            Time.timeScale = 1f;

            if (timer != null) timer.Stop();
            if (eating != null) eating.enabled = false;
            if (hole != null) hole.enabled = false;
        }
    }
}