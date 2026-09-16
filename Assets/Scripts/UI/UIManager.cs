using System.Collections;
using System.Globalization;
using HungryHole.Core;
using HungryHole.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace HungryHole.UI
{
    /// <summary>
    /// Runtime UGUI front end (Phase 8 / reference boot-startGame-endGame flow):
    /// one Screen-Space-Overlay canvas built from code, scaled with screen size
    /// against the 1280x720 reference resolution. Owns the menu -> playing <->
    /// paused -> game over state machine and the in-game HUD (score pill with
    /// rolling count-up, hole-radius pill with progress fill, timer pill with
    /// low-time pulse, streak chip, restart confirm). All visuals come from
    /// UiTheme runtime sprites and the built-in font — no UI assets.
    /// M7 Stage E adds presentation-only screen fades (menu / pause / game over
    /// CanvasGroups, unscaled time), a game-over score count-up on the displayed
    /// text only, and a subtle button hover tint — the state machine and all
    /// button routes are unchanged.
    /// M8 Stage 2 loads the local best score once per launch (SaveSystem) and
    /// saves it exactly once whenever a round ends above the stored best.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private enum UiState
        {
            Menu,
            Playing,
            Paused,
            GameOver
        }

        [Tooltip("Round lifecycle owner. Left empty, the first GameManager is used.")]
        [SerializeField] private GameManager game;

        [Tooltip("Score source for the HUD count-up. Left empty, auto-found.")]
        [SerializeField] private ScoreManager score;

        [Tooltip("Combo/streak source for the HUD chip. Left empty, auto-found.")]
        [SerializeField] private ComboManager combo;

        [Tooltip("Countdown source for the timer pill. Left empty, auto-found.")]
        [SerializeField] private RoundTimer timer;

        [Tooltip("Hole whose radius the HUD tracks. Left empty, auto-found.")]
        [SerializeField] private HoleController hole;

        private UiState state = UiState.Menu;

        private GameObject hudRoot;
        private GameObject menuScreen;
        private GameObject pauseScreen;
        private GameObject confirmDialog;
        private GameObject gameOverScreen;

        private Text scoreValueText;
        private RectTransform scorePillRect;
        private Text radiusText;
        private Image radiusFill;
        private Text timerText;
        private RectTransform timerPillRect;
        private Text comboText;
        private CanvasGroup comboGroup;
        private Text finalScoreText;
        private Text bestScoreText;

        // M7 Stage E state: per-screen CanvasGroups + one fade coroutine each, and
        // the game-over count-up (displayed text only — ScoreManager is untouched).
        private CanvasGroup menuGroup;
        private CanvasGroup pauseGroup;
        private CanvasGroup gameOverGroup;
        private Coroutine menuFade;
        private Coroutine pauseFade;
        private Coroutine gameOverFade;
        private bool gameOverCounting;
        private float gameOverScoreShown;
        private int gameOverScoreTarget;

        private int displayedScore;
        // Best round score shown on the results screen; loaded once per launch
        // from the local save (M8 Stage 2) so it survives application restarts.
        private int bestScore;
        private int lastComboCount;
        private float scoreBumpTimer;
        private float comboPopTimer;
        private float comboAlpha;

        // M9 Stage 2: HUD display caches. The score/timer/radius labels are
        // rewritten only when their displayed value changes, so steady-state
        // frames no longer build strings. Sentinels force the first render.
        private int lastShownScore = int.MinValue;
        private int lastShownTimerTotal = -1;
        private int lastShownRadiusTenths = int.MinValue;

        private const float ScoreBumpDuration = 0.18f;
        private const float ComboPopDuration = 0.2f;

        // M7 Stage E: screen fades and the game-over score count-up (all unscaled).
        private const float ScreenFadeDuration = 0.25f;
        private const float GameOverCountDuration = 0.7f;

        private void Start()
        {
            ResolveReferences();
            BuildEventSystem();
            BuildUi();
            LoadPersistedBestScore();

            // Reference boot: the game opens on the start screen; no round runs.
            SetState(UiState.Menu);
        }

        // M8 Stage 2: load the local best score once per launch. Missing, corrupt
        // or unsupported saves already fall back to defaults inside SaveSystem.
        private void LoadPersistedBestScore()
        {
            SaveData save = SaveSystem.LoadProgress();
            if (save != null)
            {
                bestScore = save.bestScore;
            }
        }

        private void ResolveReferences()
        {
            if (game == null) game = FindFirstObjectByType<GameManager>();
            if (score == null) score = FindFirstObjectByType<ScoreManager>();
            if (combo == null) combo = FindFirstObjectByType<ComboManager>();
            if (timer == null) timer = FindFirstObjectByType<RoundTimer>();
            if (hole == null) hole = FindFirstObjectByType<HoleController>();

            if (game != null)
            {
                game.RoundEnded += OnRoundEnded;
            }

            if (score != null)
            {
                score.ScoreChanged += OnScoreChanged;
            }
        }

        private void OnDestroy()
        {
            if (game != null)
            {
                game.RoundEnded -= OnRoundEnded;
            }

            if (score != null)
            {
                score.ScoreChanged -= OnScoreChanged;
            }
        }

        private void Update()
        {
            HandleEscape();
            UpdateScoreRoll();
            UpdateTimerPill();
            UpdateRadiusPill();
            UpdateComboChip();
            UpdateGameOverCount();
        }

        // ----- State transitions (reference boot / startGame / pause / endGame) -----

        private void StartPlaying()
        {
            // RestartRound performs the full reset (targets, hole, timer, score,
            // Time.timeScale = 1) so every entry point starts an identical run.
            game.RestartRound();
            SetState(UiState.Playing);
        }

        private void PauseGame()
        {
            if (state != UiState.Playing)
            {
                return;
            }

            game.Pause();
            SetState(UiState.Paused);
        }

        private void ResumeGame()
        {
            if (state != UiState.Paused)
            {
                return;
            }

            game.Resume();
            SetState(UiState.Playing);
        }

        private void ReturnToMenu()
        {
            game.ReturnToMenu();
            SetState(UiState.Menu);
        }

        private void ConfirmRestart()
        {
            confirmDialog.SetActive(false);
            game.RestartRound();
            SetState(UiState.Playing);
        }

        private void OpenRestartConfirm()
        {
            confirmDialog.SetActive(true);
        }

        private void CloseRestartConfirm()
        {
            confirmDialog.SetActive(false);
        }

        private void SetState(UiState newState)
        {
            state = newState;

            // M9 Stage 2: invalidate the HUD display caches on every transition
            // (Menu/Playing/Paused/GameOver) so the next Update re-renders each
            // label once from its live value — no state change can leave stale
            // cached text behind.
            lastShownScore = int.MinValue;
            lastShownTimerTotal = -1;
            lastShownRadiusTenths = int.MinValue;

            hudRoot.SetActive(state == UiState.Playing || state == UiState.Paused);

            // M7 Stage E: the three full screens fade instead of toggling — the
            // state routes and active-flags logic are otherwise identical.
            PlayScreenFade(menuScreen, menuGroup, ref menuFade, state == UiState.Menu);
            PlayScreenFade(pauseScreen, pauseGroup, ref pauseFade, state == UiState.Paused);
            PlayScreenFade(gameOverScreen, gameOverGroup, ref gameOverFade, state == UiState.GameOver);

            if (state != UiState.Paused)
            {
                confirmDialog.SetActive(false);
            }
        }

        // ----- M7 Stage E screen fades (unscaled time, one routine per screen) -----

        /// <summary>
        /// Shows a screen with a quick fade-in, or lets a currently visible one
        /// fade out before deactivating. A mid-fade screen simply continues from
        /// its current alpha; the routine handle is stopped first so a rapid
        /// state flip never fights itself.
        /// </summary>
        private void PlayScreenFade(GameObject screen, CanvasGroup group, ref Coroutine routine, bool show)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (show)
            {
                // Activating from inactive always starts the fade from black.
                if (!screen.activeSelf)
                {
                    group.alpha = 0f;
                }

                screen.SetActive(true);
                routine = StartCoroutine(FadeScreen(group, true));
            }
            else if (screen.activeSelf)
            {
                routine = StartCoroutine(FadeScreen(group, false));
            }
        }

        private IEnumerator FadeScreen(CanvasGroup group, bool show)
        {
            // Clicks never hit a screen that is not fully visible: raycast block
            // is re-enabled only once the fade-in completes.
            group.blocksRaycasts = false;
            float target = show ? 1f : 0f;

            while (group.alpha != target)
            {
                group.alpha = Mathf.MoveTowards(
                    group.alpha, target, Time.unscaledDeltaTime / ScreenFadeDuration);
                yield return null;
            }

            group.alpha = target;
            if (show)
            {
                group.blocksRaycasts = true;
            }
            else
            {
                group.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Presentation-only count-up of the results score line from 0 to the
        /// final score. ScoreManager's value and the best-session text are
        /// untouched; the animation simply dies if the player leaves the screen.
        /// </summary>
        private void UpdateGameOverCount()
        {
            if (!gameOverCounting)
            {
                return;
            }

            if (state != UiState.GameOver)
            {
                gameOverCounting = false;
                return;
            }

            gameOverScoreShown = Mathf.MoveTowards(
                gameOverScoreShown,
                gameOverScoreTarget,
                gameOverScoreTarget * (Time.unscaledDeltaTime / GameOverCountDuration));
            int shown = Mathf.RoundToInt(gameOverScoreShown);
            finalScoreText.text = "SCORE: " + shown.ToString(CultureInfo.InvariantCulture);

            if (shown >= gameOverScoreTarget)
            {
                gameOverCounting = false;
            }
        }

        // ----- Per-frame HUD behaviour (unscaled so pause keeps the UI responsive) -----

        private void HandleEscape()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (confirmDialog.activeSelf)
            {
                CloseRestartConfirm();
                return;
            }

            if (state == UiState.Playing)
            {
                PauseGame();
            }
            else if (state == UiState.Paused)
            {
                ResumeGame();
            }
        }

        // Reference score display: the shown number rolls toward the real score
        // (displayedScore += max(1, diff * 10 * dt)) and the pill bumps on eats.
        private void UpdateScoreRoll()
        {
            if (scoreValueText == null)
            {
                return;
            }

            int target = score != null ? score.Current : 0;
            if (displayedScore < target)
            {
                int step = Mathf.Max(1, Mathf.RoundToInt((target - displayedScore) * 10f * Time.unscaledDeltaTime));
                displayedScore = Mathf.Min(target, displayedScore + step);
            }
            else if (displayedScore > target)
            {
                // Reset/restart: snap back down immediately.
                displayedScore = target;
            }

            // M9 Stage 2: rewrite the label only when the displayed value changes
            // (format unchanged).
            if (displayedScore != lastShownScore)
            {
                lastShownScore = displayedScore;
                scoreValueText.text = displayedScore.ToString(CultureInfo.InvariantCulture);
            }

            if (scoreBumpTimer > 0f)
            {
                scoreBumpTimer = Mathf.Max(0f, scoreBumpTimer - Time.unscaledDeltaTime);
                float bump = scoreBumpTimer / ScoreBumpDuration;
                float scale = 1f + 0.14f * bump;
                scorePillRect.localScale = new Vector3(scale, scale, 1f);
            }
            else if (scorePillRect.localScale.x != 1f)
            {
                scorePillRect.localScale = Vector3.one;
            }
        }

        private void OnScoreChanged(int newScore)
        {
            if (newScore > displayedScore)
            {
                scoreBumpTimer = ScoreBumpDuration;
            }
        }

        private void UpdateTimerPill()
        {
            if (timerText == null)
            {
                return;
            }

            float timeLeft = timer != null ? timer.TimeLeft : 0f;
            int total = Mathf.CeilToInt(timeLeft);

            // M9 Stage 2: the m:ss text only depends on the whole second, so it is
            // rebuilt only when that value changes (format unchanged). The low-time
            // pulse below keeps running every frame as before.
            if (total != lastShownTimerTotal)
            {
                lastShownTimerTotal = total;
                timerText.text = $"{total / 60}:{(total % 60).ToString("00", CultureInfo.InvariantCulture)}";
            }

            bool warn = timer != null && timer.IsRunning && timeLeft <= 15f && timeLeft > 0f;
            if (warn)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 9f);
                timerText.color = Color.Lerp(UiTheme.Paper, UiTheme.Accent, 0.55f + 0.45f * pulse);
                float scale = 1f + 0.05f * pulse;
                timerPillRect.localScale = new Vector3(scale, scale, 1f);
            }
            else
            {
                timerText.color = UiTheme.Paper;
                timerPillRect.localScale = Vector3.one;
            }
        }

        private void UpdateRadiusPill()
        {
            if (radiusText == null || game == null || game.Config == null)
            {
                return;
            }

            float radius = hole != null ? hole.CurrentRadius : 0f;

            // M9 Stage 2: one decimal is the whole visible text, so the label is
            // rebuilt only when the tenths value changes — the same rounding the
            // previous per-frame ToString("F1") produced (Math.Round with
            // MidpointRounding.AwayFromZero, matching the "F" format convention).
            int tenths = (int)System.Math.Round((double)radius * 10.0, System.MidpointRounding.AwayFromZero);
            if (tenths != lastShownRadiusTenths)
            {
                lastShownRadiusTenths = tenths;
                radiusText.text = (((double)tenths) / 10.0).ToString("F1", CultureInfo.InvariantCulture) + " m";
            }

            // Reference size meter: fill = min(100, progress * 100 + 8) percent
            // from the starting radius to the maximum radius.
            float min = game.Config.startRadius;
            float max = game.Config.maxRadius;
            float progress = max > min ? Mathf.Clamp01((radius - min) / (max - min)) : 0f;
            float fill = Mathf.Min(1f, progress + 0.08f);
            if (radiusFill.fillAmount != fill)
            {
                radiusFill.fillAmount = fill;
            }
        }

        private void UpdateComboChip()
        {
            if (comboGroup == null)
            {
                return;
            }

            bool active = combo != null && combo.Count > 0 && combo.WindowTimeLeft > 0f
                && (state == UiState.Playing || state == UiState.Paused);
            if (active)
            {
                if (combo.Count != lastComboCount)
                {
                    lastComboCount = combo.Count;
                    comboPopTimer = ComboPopDuration;
                }

                comboText.text = "x" + combo.Count + " STREAK!";
            }
            else
            {
                lastComboCount = 0;
            }

            comboAlpha = Mathf.MoveTowards(
                comboAlpha, active ? 1f : 0f, Time.unscaledDeltaTime * 5f);
            comboGroup.alpha = comboAlpha;

            if (comboPopTimer > 0f)
            {
                comboPopTimer = Mathf.Max(0f, comboPopTimer - Time.unscaledDeltaTime);
                float pop = comboPopTimer / ComboPopDuration;
                float scale = 1f + 0.28f * pop;
                comboGroup.transform.localScale = new Vector3(scale, scale, 1f);
            }
            else
            {
                comboGroup.transform.localScale = Vector3.one;
            }
        }

        private void OnRoundEnded()
        {
            int finalScore = score != null ? score.Current : 0;

            // M8 Stage 2: the results-screen best is the persistent best. A new
            // record updates the loaded value and writes the save exactly once;
            // every other round end (including restarts and menu returns) never
            // touches the save.
            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                SaveSystem.SaveProgress(new SaveData { bestScore = bestScore });
            }

            // M7 Stage E: best shows its final value immediately; the
            // score line counts up from 0 as presentation only.
            finalScoreText.text = "SCORE: 0";
            bestScoreText.text = "BEST THIS SESSION: " + bestScore.ToString(CultureInfo.InvariantCulture);
            gameOverScoreTarget = finalScore;
            gameOverScoreShown = 0f;
            gameOverCounting = finalScore > 0;
            SetState(UiState.GameOver);
        }

        // ----- Runtime construction (UiTheme sprites + built-in font only) -----

        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("UiEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("UiCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Transform canvasTransform = canvasObject.transform;
            BuildHud(canvasTransform);
            BuildMenu(canvasTransform);
            BuildPause(canvasTransform);
            BuildConfirm(canvasTransform);
            BuildGameOver(canvasTransform);
        }

        private void BuildHud(Transform parent)
        {
            hudRoot = new GameObject("Hud", typeof(RectTransform));
            hudRoot.transform.SetParent(parent, false);
            Stretch((RectTransform)hudRoot.transform);
            hudRoot.SetActive(false);

            // Score pill, top-left: rolling score count-up with an eat bump.
            GameObject scorePill = CreatePill("ScorePill", hudRoot.transform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(220f, 58f), UiTheme.Pill);
            scorePillRect = (RectTransform)scorePill.transform;
            CreateText("Label", scorePill.transform, "SCORE", 12, UiTheme.PaperSoft,
                TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -7f), new Vector2(190f, 14f));
            scoreValueText = CreateText("Value", scorePill.transform, "0", 26, UiTheme.Paper,
                TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 10f), new Vector2(190f, 28f));

            // Hole radius pill with the reference progress fill (min 8%).
            GameObject radiusPill = CreatePill("RadiusPill", hudRoot.transform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -88f), new Vector2(220f, 58f), UiTheme.Pill);
            CreateText("Label", radiusPill.transform, "HOLE RADIUS", 12, UiTheme.PaperSoft,
                TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -7f), new Vector2(190f, 14f));
            radiusText = CreateText("Value", radiusPill.transform, "0.0 m", 17, UiTheme.Paper,
                TextAnchor.MiddleRight, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 2f), new Vector2(120f, 22f));

            Image track = AddImage(radiusPill.transform, "Track", UiTheme.RoundedSprite, new Color(1f, 1f, 1f, 0.12f));
            SetRect((RectTransform)track.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 12f), new Vector2(192f, 8f));
            radiusFill = AddImage(radiusPill.transform, "Fill", UiTheme.FillGradientSprite, Color.white);
            radiusFill.type = Image.Type.Filled;
            radiusFill.fillMethod = Image.FillMethod.Horizontal;
            radiusFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            radiusFill.fillAmount = 0f;
            SetRect((RectTransform)radiusFill.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 12f), new Vector2(192f, 8f));

            // Restart control (opens the confirm dialog).
            Button restartButton = CreateButton("RestartButton", hudRoot.transform, "RESTART", 14, UiTheme.Paper,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -156f), new Vector2(120f, 34f), UiTheme.ButtonGhost);
            restartButton.onClick.AddListener(OpenRestartConfirm);

            // Timer pill, top-right.
            GameObject timerPill = CreatePill("TimerPill", hudRoot.transform,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(150f, 58f), UiTheme.Pill);
            timerPillRect = (RectTransform)timerPill.transform;
            timerText = CreateText("Value", timerPill.transform, "1:30", 26, UiTheme.Paper,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130f, 40f));

            // Streak chip, centered under the top bar.
            GameObject comboChip = CreatePill("ComboChip", hudRoot.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(240f, 44f), UiTheme.Dim);
            comboGroup = comboChip.AddComponent<CanvasGroup>();
            comboGroup.blocksRaycasts = false;
            comboGroup.alpha = 0f;
            comboAlpha = 0f;
            comboText = CreateText("Value", comboChip.transform, "x2 STREAK!", 20, UiTheme.Accent2,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 30f));
        }

        private void BuildMenu(Transform parent)
        {
            menuScreen = CreateBackdrop("MenuScreen", parent);
            menuGroup = menuScreen.GetComponent<CanvasGroup>();
            menuScreen.SetActive(false);

            AddImage(menuScreen.transform, "HoleMark", UiTheme.HoleMarkSprite, Color.white);
            SetRect((RectTransform)menuScreen.transform.Find("HoleMark"), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(150f, 150f));

            CreateText("Title", menuScreen.transform, "HUNGRY HOLE", 62, UiTheme.Paper,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(900f, 80f));
            CreateText("Subtitle", menuScreen.transform, "Eat the whole town before the clock runs out.", 20, UiTheme.PaperSoft,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -42f), new Vector2(900f, 30f));

            Button playButton = CreateButton("PlayButton", menuScreen.transform, "START PLAYING", 24, UiTheme.Ink,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(340f, 74f), UiTheme.Accent);
            playButton.onClick.AddListener(StartPlaying);

            CreateText("Instructions", menuScreen.transform,
                "WASD OR ARROW KEYS TO MOVE   ·   EAT ANYTHING SMALLER THAN YOUR HOLE   ·   ESC TO PAUSE",
                15, UiTheme.PaperFaint, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1100f, 24f));
        }

        private void BuildPause(Transform parent)
        {
            pauseScreen = CreateDimScreen("PauseScreen", parent);
            pauseGroup = pauseScreen.GetComponent<CanvasGroup>();
            pauseScreen.SetActive(false);

            CreateText("Title", pauseScreen.transform, "PAUSED", 38, UiTheme.Paper,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(400f, 60f));

            Button resumeButton = CreateButton("ResumeButton", pauseScreen.transform, "RESUME", 20, UiTheme.Ink,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(280f, 56f), UiTheme.Accent);
            resumeButton.onClick.AddListener(ResumeGame);

            Button restartButton = CreateButton("RestartButton", pauseScreen.transform, "RESTART", 20, UiTheme.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(280f, 56f), UiTheme.ButtonGhost);
            restartButton.onClick.AddListener(OpenRestartConfirm);

            Button menuButton = CreateButton("MenuButton", pauseScreen.transform, "MAIN MENU", 20, UiTheme.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(280f, 56f), UiTheme.ButtonGhost);
            menuButton.onClick.AddListener(ReturnToMenu);
        }

        private void BuildConfirm(Transform parent)
        {
            confirmDialog = CreateDimScreen("ConfirmDialog", parent);
            confirmDialog.SetActive(false);

            CreateText("Title", confirmDialog.transform, "RESTART THIS RUN?", 24, UiTheme.Paper,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(500f, 40f));
            CreateText("Note", confirmDialog.transform, "Your current score will be lost.", 16, UiTheme.PaperSoft,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(500f, 26f));

            Button yesButton = CreateButton("YesButton", confirmDialog.transform, "YES, RESTART", 18, UiTheme.Ink,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-105f, -60f), new Vector2(190f, 52f), UiTheme.Accent);
            yesButton.onClick.AddListener(ConfirmRestart);

            Button cancelButton = CreateButton("CancelButton", confirmDialog.transform, "CANCEL", 18, UiTheme.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(105f, -60f), new Vector2(190f, 52f), UiTheme.ButtonGhost);
            cancelButton.onClick.AddListener(CloseRestartConfirm);
        }

        private void BuildGameOver(Transform parent)
        {
            gameOverScreen = CreateBackdrop("GameOverScreen", parent);
            gameOverGroup = gameOverScreen.GetComponent<CanvasGroup>();
            gameOverScreen.SetActive(false);

            AddImage(gameOverScreen.transform, "HoleMark", UiTheme.HoleMarkSprite, Color.white);
            SetRect((RectTransform)gameOverScreen.transform.Find("HoleMark"), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), new Vector2(120f, 120f));

            CreateText("Title", gameOverScreen.transform, "TOWN CLEARED!", 54, UiTheme.Accent2,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 70f));
            finalScoreText = CreateText("FinalScore", gameOverScreen.transform, "SCORE: 0", 34, UiTheme.Paper,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(700f, 50f));
            bestScoreText = CreateText("BestScore", gameOverScreen.transform, "BEST THIS SESSION: 0", 17, UiTheme.PaperSoft,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -84f), new Vector2(700f, 26f));

            Button againButton = CreateButton("AgainButton", gameOverScreen.transform, "PLAY AGAIN", 22, UiTheme.Ink,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(300f, 62f), UiTheme.Accent);
            againButton.onClick.AddListener(StartPlaying);

            Button menuButton = CreateButton("MenuButton", gameOverScreen.transform, "MAIN MENU", 18, UiTheme.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -236f), new Vector2(300f, 50f), UiTheme.ButtonGhost);
            menuButton.onClick.AddListener(ReturnToMenu);
        }

        // ----- Shared widget helpers -----

        private static GameObject CreateBackdrop(string name, Transform parent)
        {
            GameObject screen = new GameObject(name, typeof(Image), typeof(CanvasGroup));
            screen.transform.SetParent(parent, false);
            Image image = screen.GetComponent<Image>();
            image.sprite = UiTheme.BackdropSprite;
            image.color = Color.white;
            Stretch((RectTransform)screen.transform);
            return screen;
        }

        private static GameObject CreateDimScreen(string name, Transform parent)
        {
            GameObject screen = new GameObject(name, typeof(Image), typeof(CanvasGroup));
            screen.transform.SetParent(parent, false);
            Image image = screen.GetComponent<Image>();
            image.sprite = UiTheme.RoundedSprite;
            image.type = Image.Type.Simple;
            image.color = UiTheme.Dim;
            Stretch((RectTransform)screen.transform);
            return screen;
        }

        private static GameObject CreatePill(
            string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color color)
        {
            GameObject pill = new GameObject(name, typeof(Image));
            pill.transform.SetParent(parent, false);
            Image image = pill.GetComponent<Image>();
            image.sprite = UiTheme.RoundedSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            SetRect((RectTransform)pill.transform, anchor, pivot, position, size);
            return pill;
        }

        private static Image AddImage(Transform parent, string name, Sprite sprite, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        private static Text CreateText(
            string name, Transform parent, string content, int fontSize, Color color,
            TextAnchor alignment, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            GameObject textObject = new GameObject(name, typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = UiTheme.GameFont;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            SetRect((RectTransform)textObject.transform, anchor, pivot, position, size);
            return text;
        }

        private static Button CreateButton(
            string name, Transform parent, string label, int fontSize, Color textColor,
            Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color color)
        {
            GameObject buttonObject = new GameObject(name, typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.sprite = UiTheme.RoundedSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            SetRect((RectTransform)buttonObject.transform, anchor, pivot, position, size);

            CreateText("Label", buttonObject.transform, label, fontSize, textColor,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                size - new Vector2(16f, 12f));

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.fadeDuration = 0.08f;
            // M7 Stage E: a subtle hover darken on top of the existing press tint.
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            button.colors = colors;
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}