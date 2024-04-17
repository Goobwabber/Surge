using System;
using UnityEngine;

namespace Surge.Models
{
    [Serializable]
    internal class AnimationStateInfo
    {
        [field: SerializeField, Range(0, 1)]
        public float RadialState { get; internal set; }

        [field: SerializeField]
        public PuppetMenuState PuppetState { get; internal set; }

        [field: SerializeField]
        public ToggleMode Toggle { get; internal set; }

        [field: SerializeField]
        public float Analog { get; internal set; }

        [field: SerializeField]
        public Vector4 Vector { get; internal set; }

        [field: SerializeField]
        public UnityEngine.Object Object { get; internal set; }
    }
}
