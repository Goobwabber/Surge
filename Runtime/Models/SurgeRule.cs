using System;
using UnityEngine;

namespace Surge.Models
{
    [Serializable]
    internal class SurgeRule
    {
        [field: SerializeField]
        public string CauseLayer { get; private set; } = string.Empty;

        [field: SerializeField]
        public string EffectLayer { get; private set; } = string.Empty;

        [field: SerializeField]
        public ToggleMode CauseState { get; private set; } = ToggleMode.Enabled;

        [field: SerializeField]
        public ToggleMode EffectState { get; private set; } = ToggleMode.Disabled;
    }
}