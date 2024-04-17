using System;
using UnityEngine;

namespace Flare.Models
{
    [Serializable]
    internal class AnimationGroupInfo
    {
        [field: SerializeField]
        public AnimationGroupType AnimationType { get; private set; }

        [field: SerializeField]
        public AnimationToggleType ToggleType { get; private set; }

        [field: SerializeField]
        public UnityEngine.Object?[] Objects { get; private set; } = Array.Empty<UnityEngine.Object?>();

        [field: SerializeField]
        public AnimationPropertyInfo[] Properties { get; private set; } = Array.Empty<AnimationPropertyInfo>();

        [field: SerializeField]
        public AnimationStateInfo[] SharedAnimationStates { get; private set; } = Array.Empty<AnimationStateInfo>();

        public AnimationGroupInfo(AnimationGroupType type)
        {
            AnimationType = type;
        }
    }
}