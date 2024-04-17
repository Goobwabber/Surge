using System;
using UnityEngine;

namespace Flare.Models
{
    [Serializable]
    internal class MenuItemInfo
    {
        [field: SerializeField]
        public string Path { get; set; } = string.Empty;

        [field: SerializeField]
        public Texture2D Icon { get; private set; } = null!;

        [field: SerializeField]
        public MenuItemType Type { get; private set; } = MenuItemType.Toggle;

        [field: SerializeField]
        public bool IsSaved { get; private set; } = true;

        [field: SerializeField]
        public ToggleMenuState DefaultState { get; private set; } = ToggleMenuState.Inactive;

        [field: Range(0, 1)]
        [field: SerializeField]
        public float DefaultRadialValue { get; private set; }

        [field: SerializeField]
        public PuppetMenuState DefaultPuppetState { get; private set; }

        [field: SerializeField]
        public InterpolationInfo Interpolation { get; private set; } = new();

        [field: SerializeField]
        public bool ApplyToAvatar { get; private set; } = new();

        [field: SerializeField]
        public bool ShowDefault { get; private set; }

        [field: SerializeField]
        public bool ShowIcon { get; private set; }

        [field: SerializeField]
        public bool ShowDuration { get; private set; }
    }
}
