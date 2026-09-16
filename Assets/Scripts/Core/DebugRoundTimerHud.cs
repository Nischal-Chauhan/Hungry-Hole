using UnityEngine;

namespace HungryHole.Core
{
    /// <summary>
    /// TEMPORARY Phase 5 diagnostic overlay: draws the RoundTimer state as a
    /// plain IMGUI label so the 90 s countdown can be visually verified before
    /// the real HUD exists (the visible timer HUD is a Phase 8 task).
    /// Remove this component when the Phase 8 HUD lands.
    /// </summary>
    public class DebugRoundTimerHud : MonoBehaviour
    {
        private RoundTimer timer;
        private GUIStyle style;
        private bool styleReady;

        private void Start()
        {
            timer = FindFirstObjectByType<RoundTimer>();
        }

        private void OnGUI()
        {
            if (!styleReady)
            {
                // Built-in font, assigned at runtime so no font asset/GUID is involved.
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    fontStyle = FontStyle.Bold
                };
                style.normal.textColor = new Color(1f, 0.823f, 0.247f); // #ffd23f, reference accent
                if (font != null)
                {
                    style.font = font;
                }
                styleReady = true;
            }

            if (timer == null)
            {
                GUI.Label(new Rect(12f, 12f, 460f, 34f), "DEBUG TIMER: no RoundTimer found", style);
                return;
            }

            GUI.Label(
                new Rect(12f, 12f, 460f, 34f),
                string.Format("DEBUG TIMER: {0:0.00}s  [{1}]", timer.TimeLeft, timer.IsRunning ? "RUNNING" : "STOPPED"),
                style);
        }
    }
}