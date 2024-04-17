using System;
using UnityEngine;

namespace Surge.Models
{
    [Serializable]
    internal class AnimationPropertyInfo
    {
        [field: SerializeField]
        public string Name { get; internal set; } = string.Empty;

        [field: SerializeField]
        public string Path { get; internal set; } = string.Empty;

        [field: SerializeField]
        public PropertyValueType ValueType { get; internal set; }

        [field: SerializeField]
        public PropertyColorType ColorType { get; internal set; }

        [field: SerializeField]
        public string ContextType { get; internal set; } = string.Empty;

        [field: SerializeField]
        public string ObjectValueType { get; internal set; } = string.Empty;

        [field: SerializeField]
        public float DefaultAnalog { get; internal set; }

        [field: SerializeField]
        public Vector4 DefaultVector { get; internal set; }

        [field: SerializeField]
        public UnityEngine.Object? DefaultObject { get; internal set; }

        [field: SerializeField]
        public AnimationStateInfo[] AnimationStates { get; private set; } = Array.Empty<AnimationStateInfo>();
    }
}
