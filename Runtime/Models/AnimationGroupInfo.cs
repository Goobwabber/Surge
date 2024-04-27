using System;
using UnityEngine;

namespace Surge.Models
{
    [Serializable]
    internal class AnimationGroupInfo
    {
        [field: SerializeField]
        public AnimationGroupType GroupType { get; private set; }

        [field: SerializeField]
        public AnimationToggleType ToggleType { get; private set; }

        [field: SerializeField]
        public UnityEngine.Object?[] Objects { get; private set; } = Array.Empty<UnityEngine.Object?>();

        [field: SerializeField]
        public AnimationPropertyInfo[] Properties { get; private set; } = Array.Empty<AnimationPropertyInfo>();

        [field: SerializeField]
        public PropertyValueType SharedValueType { get; internal set; }

        [field: SerializeField]
        public PropertyColorType SharedColorType { get; internal set; }

        [field: SerializeField]
        public string SharedObjectType { get; internal set; } = string.Empty;

        [field: SerializeField]
        public AnimationStateInfo[] SharedAnimationStates { get; private set; } = Array.Empty<AnimationStateInfo>();

        [field: SerializeField]
        public SurgeEasing GroupEasing { get; private set; } = SurgeEasing.Sine;

        [field: SerializeField]
        public bool IsPlatformExclusive { get; private set; } = false;

        [field: SerializeField]
        public SurgePlatformType PlatformType { get; private set; } = SurgePlatformType.PC;

        [field: SerializeField]
        public bool ShowSharedCurve { get; private set; } = false;

        [field: SerializeField]
        public AnimationCurve SharedCurve { get; private set; } = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public AnimationGroupInfo(AnimationGroupType type)
        {
            GroupType = type;
        }
    }
}