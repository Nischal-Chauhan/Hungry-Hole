using System;
using UnityEngine;

namespace HungryHole.Core
{
    /// <summary>
    /// Versioned local save model (M8 Stage 1 — Backend Schema PlayerProgress).
    /// Kept minimal for V1: only the schema version and the persisted best
    /// score. Settings and cosmetic fields are added later if those systems
    /// exist; bumping CurrentSchemaVersion governs how older saves are treated.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Schema version this build writes and reads.</summary>
        public const int CurrentSchemaVersion = 1;

        [Tooltip("Save format version written by this build.")]
        public int schemaVersion = CurrentSchemaVersion;

        [Tooltip("Best round score across application launches.")]
        public int bestScore;
    }
}