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

        public AnimationGroupInfo(AnimationGroupType type)
        {
            GroupType = type;
        }
    }
}