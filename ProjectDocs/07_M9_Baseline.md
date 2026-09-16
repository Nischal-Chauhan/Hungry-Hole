# Hungry Hole — M9 / Phase 11 Performance Baseline

Stage 1 (measurement only). No optimization was performed. This document records
**only** measurements that were actually captured during the Stage 1 session on
2026-09-14. Values that were not captured are marked **"Not captured"** — nothing
in this document is estimated or inferred beyond explicitly labeled correlations.

## 1. Test Environment

| Item | Value |
|---|---|
| CPU | Intel Core i5-5300U @ 2.30 GHz (2 cores / 4 threads) |
| GPU | Intel HD Graphics 5500 |
| RAM | 16 GB |
| OS | Windows 11 Pro 24H2 |
| Unity | 6.3 LTS / 6000.3.23f1 |
| Rendering | URP (PC renderer asset: render scale 1, main-light shadowmap 1024, shadow distance 40) |
| Standard test resolution | 1280×720 |
| Test date/time | 2026-09-14, ~19:49–20:59 local (telemetry window); second Play Mode session entered ~21:05 local (outside telemetry window) |
| GPU Usage module availability | **Not captured** (module availability was not reported during the session) |

## 2. Methodology

The baseline was measured with two recording streams, neither of which added code,
packages, or project files:

1. **Unity Profiler / Game View Stats readings** (CPU Usage, Memory, Rendering
   modules; Stats window FPS/draw calls/batches/SetPass; no Deep Profile):
   requested per scenario, but **the readings were not supplied during the
   session** — all frame-level Profiler values below are therefore marked
   "Not captured".
2. **OS-level process telemetry** (objective, timestamped, ~2 s sampling):
   Unity.exe CPU time (% of total machine), working set, private memory, and
   Windows GPU Engine utilization for the Unity process. 928 valid samples,
   19:49:01–20:59:57 local. This is the unbiased record of frame-behavior
   trends, since it does not add profiler overhead.

Scenario markers were additionally captured from Unity's own Editor.log
(Play Mode entry/exit, natural round-end events, Unity-reported memory lines).

Caveats: GPU Engine utilization sums all engines of the process (3D, copy,
video) and can exceed 100% briefly; ~2 s sampling cannot capture single-frame
spikes. CPU% is relative to all 4 logical threads.

## 3. Scenario 1 — Menu Idle (60 s)

| Metric | Result |
|---|---|
| FPS | **Not captured** |
| Frame time | **Not captured** |
| Draw Calls / Batches / SetPass | **Not captured** |
| GC.Alloc | **Not captured** |
| Process idle baseline (measured) | Editor-idle (pre-Play) CPU 0–0.8%, GPU 0%, working set 1210.8 MB, private 2022.4 MB |

A 60-second menu-idle window was not separately identifiable in telemetry
(menu time is not distinguishable from other idle periods without session notes).

## 4. Scenario 2 — Fresh Round

### Spawn Spike

| Metric | Result |
|---|---|
| Worst spawn-frame ms | **Not captured** |
| CPU frame time / GPU frame time | **Not captured** |
| GC.Alloc | **Not captured** |
| Object-count delta at Play Mode entry (measured, Editor.log) | 8,484 → 8,893 loaded objects (**+409** objects) |
| Unity-reported memory at Play Mode entry (measured) | "Memory consumption went from 1.28 GB to 1.28 GB" |
| Process behavior at play entry (measured) | CPU burst 19:54:31–19:55:54 (peak 44.5% machine) with GPU saturation 85–96% during the burst |

### Steady State

| Metric | Result |
|---|---|
| FPS / frame time | **Not captured** |
| Draw Calls / Batches / SetPass | **Not captured** |
| GC.Alloc | **Not captured** |
| UIManager.Update ms | **Not captured** |
| HoleEating.FixedUpdate ms | **Not captured** |
| Process behavior during observed play bursts (measured) | GPU 72–100% sustained during bursts (see §6 timing); CPU 30–49% of machine during bursts |

⚠ **Round-completion note:** Unity's Editor.log records natural round ends
("Timer reached zero"). The only such events in the current editor session's log
are two rounds from the earlier M8 Stage 2 verification (final scores 170 and
195). **No natural round-end was logged during the Stage 1 telemetry window**,
so the 90-second steady-state scenario cannot be individually confirmed from the
objective record.

## 5. Scenario 3 — Restart ×5

| Restart | Worst frame |
|---|---|
| 1 | **Not captured** |
| 2 | **Not captured** |
| 3 | **Not captured** |
| 4 | **Not captured** |
| 5 | **Not captured** |

Restart actions are not logged by the game, so no restart could be
objectively marked in the record. CPU burst clusters were detected (§6), but
mapping clusters to individual restarts is **suspected correlation, not
measurement**.

## 6. Scenario 4 — Long Session

**Measured (process telemetry, 19:49–20:59, 928 samples):**

| Observation | Value |
|---|---|
| Window duration | ~71 minutes |
| Working set | start 1210.8 MB → **peak 2451.4 MB** → 1973.2 MB at window end (declining) |
| Private memory | start 2022.4 MB → 3336.0 MB by 20:11 → **flat at 3265–3336 MB for the final ~48 min** (no upward trend in the observed window) |
| Unity.exe CPU (whole window) | mean 3.7%, p95 32.8%, p99 43.8%, max 49.2% (of 4 logical threads) |
| GPU Engine utilization (whole window) | mean 5.8%, p99 97.8%, max 100.9% |
| Activity burst clusters (CPU ≥10% grouped) | 19:54:31–19:58:57, 20:00:10–20:01:37, 20:07:22–20:08:39, 20:29:55–20:33:21 |
| GPU during bursts | 61–100.9% (near-saturation across most burst samples) |
| Observed spikes/stutters | **Not captured** (frame-level) |
| Performance degradation / crashes / errors | None observed: no error/warning CS lines and no crash in Editor.log; memory plateaued rather than creeping |

Duration of actual gameplay within the window is not separately identifiable;
rounds completed: **not captured** (see §4 note). A second Play Mode session
entered ~21:05 local (log line 790) falls **outside** the telemetry window —
no telemetry for its tail.

## 7. Resolution Testing

### 1600×900

Not captured. (No Profiler/Stats readings were supplied and the actual Game
View size was not reported.)

### 1920×1080

Not captured.

## 8. Profiler Hotspots

**Measured:**
- **GPU saturation during active play (measured, process-level):** during every
  activity burst, GPU Engine utilization held at 72–100% while CPU ran at
  30–49% — on this iGPU, rendering is the dominant process-level cost during
  play. Sampling is ~2 s, so this characterizes burst windows, not single frames.
- **CPU bursts at round activity (measured, process-level):** peak 49.2%
  machine-wide (≈2 of 4 logical threads busy); 17 burst clusters identified.
- **Memory plateau (measured, process-level):** private memory flat
  3265–3336 MB for the final ~48 observed minutes; working set declined
  2451 → 1973 MB late in the window. No unbounded growth.

**Suspected (from code inspection in the approved M9 inspection report — NOT
profiler-measured):**
- Per-frame UI text string allocations (`UIManager` score roll / timer / radius).
- Restart path destroys + rebuilds ~101 target roots and ~186 visual children
  per restart (`ClearSpawnedTargets` + `SpawnAll`).
- Per-hit `GetComponentInParent<TargetController>()` in `HoleEating.FixedUpdate`.

**Not measured:**
- Frame-level costs of `UIManager.Update`, `HoleEating.FixedUpdate`,
  `TargetSpawner`, rendering passes, shadows, and particles (Profiler readings
  not supplied).
- GC.Alloc per frame; draw calls / batches / SetPass; per-resolution behavior;
  shadow cost at max hole radius.

## 9. Baseline Findings

- **Startup/spawn cost (measured):** Play Mode entry produced a distinct CPU
  burst (peak 44.5% machine) with GPU at 85–96% for its duration, and a
  +409-object delta (Editor.log object counts). The mapping to
  `TargetSpawner.SpawnAll` specifically is **suspected**, not marked.
- **Steady-state CPU/GPU behavior (measured, process-level):** GPU near
  saturation during all observed play bursts on the HD 5500; CPU moderate.
- **Restart cost (not captured):** no restart markers exist in the record;
  restarts are not log-detectable without frame-level data.
- **UI GC behavior (not captured):** no GC.Alloc readings; the code-inspection
  concern remains suspected only.
- **HoleEating cost (not captured):** no per-script Profiler data.
- **Rendering/draw-call behavior (not captured):** no draw-call/batch data;
  process-level GPU saturation suggests rendering pressure on the iGPU.
- **Memory behavior (measured, process-level):** controlled in the observed
  window — rises with Play Mode/target population, then plateaus and declines;
  no creep over ~48 observed minutes.
- **Resolution sensitivity (not captured).**

## 10. M9 Optimization Priority

The approved order is retained:

1. Baseline measurement/documentation (this stage)
2. UI text GC cleanup
3. Eating query tuning
4. Target pooling
5. Shadow/light check
6. Draw-call check
7. Long-session + resolution verification
8. M9 checkpoint

**Justification for keeping the order:** the captured telemetry is
process-level and cannot rank frame-level optimizations (the Profiler readings
that would justify reordering were not captured). The GPU-saturation finding
raises the *suspected* importance of Stage 5/6 (shadow and draw-call work), but
moving them ahead of Stage 2–4 would be speculation; Stages 2–4 remain cheap,
reversible, and code-verified. If frame-level Profiler readings are captured in
a later stage, the order can be re-justified then.

## 11. Phase 11 Acceptance Criteria

Unchanged, qualitative (no numeric FPS requirement invented):

- No major performance spikes.
- Memory use remains controlled.
- Game is comfortable on the target machine.

## 12. Stage 1 File Changes

- Added: `ProjectDocs/07_M9_Baseline.md` (this file)
- No Assets changes; no scripts changed; no scene/prefab/material/config changes;
  no package changes; no ProjectSettings changes.

## 13. M8 Integrity Check

Verified against `C:\Users\nischal.chauhan\Hungry Hole_Checkpoint_M8_2026-09-14`
after this document was created (result recorded in the Stage 1 final report).

## 14. Compile Verification

Verified against the live editor session log: **0 errors, 0 warnings**
(`error CS` / `warning CS` / compilation-failure matches: 0 across the full
891-line session log; scripts last modified 2026-09-12, before the verified
session-start compile).

---

## M9 Final Optimization Results

*(appended 2026-09-16 at M9 finalization; the baseline sections above are
historical and unchanged. Per the reporting rules, no numerical
Profiler/FPS/draw-call/GC value was invented; every value that was not actually
captured is explicitly marked "Not captured".)*

### Stage 4 — Target Pooling (implemented and runtime-validated)

- Change: `TargetSpawner.cs`, `TargetController.cs`, `TargetVisualFactory.cs`,
  `GameManager.cs` — spawned target roots are pooled and reused across rounds
  instead of destroyed and recreated on every restart/Play Again (pool keyed by
  TargetData; full reset contract: coroutine, consumed state, collider enable,
  transform position/rotation/scale, size/data, rigidbody velocities, active
  state). The previous per-restart `Destroy`+recreate loop in
  `GameManager.ClearSpawnedTargets` was removed. Shared materials/textures are
  untouched; no per-target material instances; GameConfig, Main.unity,
  Hole.prefab, UIManager, SaveData/SaveSystem, TargetData unchanged.
- Compile: **0 errors, 0 warnings** (verified in the editor session log after
  the Stage 4 reload; whole-log `error CS`/`warning CS` matches: 0).
- Runtime result (user-confirmed): target pooling/reuse works correctly; no
  missing or duplicate targets; gameplay regression clean; restart / Play Again
  / Main Menu lifecycle clean.
- Frame-level cost of restart/rebuild vs baseline: **Not captured**.

### Stage 5 — Shadow/Light Check (inspected and retained)

- Configuration inspected from project files: single directional light with
  soft shadows, 1024 shadow map, 2 cascades, shadow distance 40, pixel light
  count 2 (PC quality level; PC_RPAsset/PC_Renderer, GUID-verified chain from
  the Standalone default quality).
- Decision: **no changes** — M7 lighting presentation is approved, detail-child
  shadow casting was already reduced in M7, and no measurement demonstrated a
  shadow cost worth changing. Frame-level shadow cost: **Not captured**.

### Stage 6 — Draw-Call Check (inspected and retained)

- Existing optimizations confirmed in code/config: shared category materials,
  TargetVisualFactory material/texture caching (created once per session, no
  per-target instances), SRP Batcher enabled, detail-child shadows already
  reduced.
- Decision: **no changes** — no Stats/Frame Debugger reading demonstrated an
  unacceptable draw-call/set-pass cost. Draw Calls / Batches / SetPass:
  **Not captured**.

### Stage 7 — Long Session + Resolutions (functionally clean; user-confirmed)

- Long Play Mode session and resolution testing (1280×720, ~1600×900,
  ~1920×1080): functionally clean per the user's validation — no crashes,
  errors, visual degradation, or stability issues; pooling stable.
- Frame-level values (FPS, frame ms, draw calls, batches, SetPass, GC.Alloc)
  and numeric memory trend per resolution: **Not captured**.

### Objective session-log evidence (Stage 4–7 window)

- Editor.log during the runtime window: **0 runtime exceptions**
  (`NullReferenceException` / `MissingReferenceException` / coroutine
  deactivation errors: 0); natural round end logged ("Timer reached zero",
  final score 260); no crashes; no error/warning CS lines.
- Editor-Play-Mode stability on the pooled build therefore verified; the
  same evidence does not provide frame-level metrics.

### M9 change summary (files)

- Stage 1: added `ProjectDocs/07_M9_Baseline.md`.
- Stage 2: modified `Assets/Scripts/UI/UIManager.cs` (UI text GC caching).
- Stage 3: modified `Assets/Scripts/Gameplay/HoleEating.cs`
  (attachedRigidbody → TryGetComponent lookup).
- Stage 4: modified `Assets/Scripts/Targets/TargetSpawner.cs`,
  `Assets/Scripts/Targets/TargetController.cs`,
  `Assets/Scripts/Targets/TargetVisualFactory.cs` (behavior-identical rotation
  extraction), `Assets/Scripts/Core/GameManager.cs` (destroy step removed).
- Stages 5–6: no changes. Stage 7: no code changes.
- Final M9 checkpoint:
  `C:\Users\nischal.chauhan\Hungry Hole_Checkpoint_M9_2026-09-15`.

### Final acceptance

- Official Phase 11 criteria: no major performance spikes (user-confirmed clean
  runtime; frame-level metric **Not captured**); memory use controlled
  (functionally clean per user; numeric trend **Not captured**); game
  comfortable on the target machine (user-confirmed).
- M6 behavior, M7 presentation, M8 persistence: unchanged (byte-level checks
  against the Pre-Stage-4 checkpoint in the Stage 4 verification; user-confirmed
  runtime regressions).
- Compile clean; no avoidable allocations introduced (Stage 3/4 changes are
  allocation-free in per-frame paths); no severe regression.
- **M9 PASS.**