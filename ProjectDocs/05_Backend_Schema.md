# Document 05 — Backend Schema — Hungry Hole

## 1. Backend Decision

**V1 does not require a backend.**

Hungry Hole is designed as a self-contained Windows arcade game. The existing reference game is client-side and does not require a remote database for its core loop.

Therefore, the backend-schema template from the supplied Vibe Coding document is adapted here to explicitly record:

> **No remote backend, database, authentication, or cloud API is required for V1.**

This is intentional, not an unfinished section.

## 2. Local Data

If persistence is implemented in V1/V2, store only lightweight local game data.

Suggested local data:

```text
GameSettings
- masterVolume
- musicVolume
- sfxVolume
- graphicsQuality
- fullscreen

PlayerProgress
- bestScore
- unlockedCosmetics
- equippedCosmetic
```

## 3. No Authentication

There is no:
- Login
- Signup
- OAuth
- User account
- Password
- Session token

## 4. No Remote Database

Do not introduce:
- PostgreSQL
- Supabase
- Firebase
- MongoDB
- Redis
- Cloud save

unless a future product requirement explicitly introduces online functionality.

## 5. No API Requirements

V1 has no required external API endpoints.

The game should remain playable without an internet connection after installation.

## 6. Local Persistence Rules
If local persistence is added:
- Validate loaded values.
- Use sensible defaults when data is missing/corrupted.
- Never allow malformed save data to crash gameplay.
- Keep save data small.
- Version save data if the schema changes.

## 7. Future Online Features — Out of Scope
Potential future systems could include:
- Online leaderboards.
- Cloud saves.
- Accounts.
- Cosmetic inventory synchronization.
- Challenges/events.

These require a separate architecture review and must not be assumed by the AI agent.

## 8. Security
Because V1 has no remote backend:
- No server secrets.
- No API keys.
- No authentication secrets.
- No environment variables required for gameplay.

Never place future service credentials inside client assets.

## 9. Source of Truth
For gameplay state:
- Runtime Unity systems are authoritative.
- ScriptableObjects/configuration assets hold tunable definitions.
- Local persistence is authoritative only for saved local preferences/progress.

Do not create a fake backend merely because the generic six-document template expects one.
