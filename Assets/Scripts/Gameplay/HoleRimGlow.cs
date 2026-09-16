using HungryHole.Core;
using UnityEngine;

namespace HungryHole.Gameplay
{
    /// <summary>
    /// Combo streak feedback (Phase 7): drives the hole rim's emissive glow from
    /// ComboManager.StreakBoost — the base reference intensity plus the streak
    /// boost, which drains with the combo window. M7 Stage B adds a short eat
    /// pulse on top: each eaten target (watched via ComboManager.Count increases)
    /// briefly boosts emission and pops the rim scale. Score/combo rules, growth
    /// and shake are untouched; the on-disk HoleRim.mat and Hole prefab are never
    /// modified because the glow runs on a runtime material instance of the rim
    /// renderer.
    /// </summary>
    public class HoleRimGlow : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        [Tooltip("Combo/streak state beside this component on the same object.")]
        [SerializeField] private ComboManager combo;

        [Tooltip("Hole whose rim glows. Left empty, the first HoleController in the scene is used.")]
        [SerializeField] private HoleController hole;

        private Renderer rimRenderer;
        private bool rimResolved;
        private Material rimMaterial;
        private Color rimAccent;
        private bool accentReady;

        // M7 eat pulse: brief, additive on top of the streak glow.
        private const float EatPulseDuration = 0.2f;
        private const float EatPulseEmission = 0.55f;
        private const float EatPulseScale = 0.08f;
        private float eatPulseTimer;
        private int lastComboCount;
        private Vector3 rimBaseScale;
        private bool rimScaleCached;

        private void Start()
        {
            // Score/combo/timer managers are usually wired in the scene; the rim
            // renderer lives on the Hole prefab, so those are looked up.
            if (combo == null) combo = FindFirstObjectByType<ComboManager>();
            if (hole == null) hole = FindFirstObjectByType<HoleController>();
        }

        private void Update()
        {
            if (hole == null || combo == null)
            {
                return;
            }

            if (!ResolveRimMaterial())
            {
                return;
            }

            // Reference eatObject feedback: every eaten target pops the rim.
            // Only increases count — combo expiry and restarts (Count -> 0)
            // never trigger a pulse.
            int count = combo.Count;
            if (count > lastComboCount)
            {
                eatPulseTimer = EatPulseDuration;
            }

            lastComboCount = count;

            if (eatPulseTimer > 0f)
            {
                // Time.deltaTime decay: freezes with the paused round, resumes cleanly.
                eatPulseTimer = Mathf.Max(0f, eatPulseTimer - Time.deltaTime);
            }

            float pulse = eatPulseTimer > 0f ? eatPulseTimer / EatPulseDuration : 0f;
            float intensity = config != null ? config.rimEmissiveIntensity : 0.6f;
            rimMaterial.SetColor(
                "_EmissionColor", rimAccent * (intensity + combo.StreakBoost + EatPulseEmission * pulse));

            if (pulse > 0f && rimScaleCached)
            {
                rimRenderer.transform.localScale = rimBaseScale * (1f + EatPulseScale * pulse);
            }
            else if (rimScaleCached && rimRenderer.transform.localScale != rimBaseScale)
            {
                rimRenderer.transform.localScale = rimBaseScale;
            }
        }

        /// <summary>
        /// Finds the rim renderer once under the hole's visual root and instantiates
        /// its runtime material with emission enabled. Returns false until both exist.
        /// </summary>
        private bool ResolveRimMaterial()
        {
            if (accentReady)
            {
                return true;
            }

            if (!rimResolved)
            {
                rimResolved = true;
                foreach (Renderer childRenderer in hole.GetComponentsInChildren<Renderer>(true))
                {
                    if (childRenderer.name == "Rim")
                    {
                        rimRenderer = childRenderer;
                        break;
                    }
                }

                if (rimRenderer == null)
                {
                    Debug.LogWarning("HoleRimGlow found no rim renderer on the hole; glow disabled.", this);
                }
            }

            if (rimRenderer == null)
            {
                return false;
            }

            // Runtime material instance so the shared HoleRim.mat stays untouched.
            rimMaterial = rimRenderer.material;
            rimMaterial.EnableKeyword("_EMISSION");
            rimAccent = rimMaterial.color;

            // M7 eat pulse: remember the authored rim scale; the pulse restores it.
            rimBaseScale = rimRenderer.transform.localScale;
            rimScaleCached = true;

            accentReady = true;
            return true;
        }
    }
}