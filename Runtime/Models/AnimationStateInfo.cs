using System;
using UnityEngine;

namespace Surge.Models
{
    // maybe we should only have float state, float analog, vector, and then object props. means more hell in ui code though?
    // the way this is now means things get more complicated inside of the containerization pass
    [Serializable]
    internal class AnimationStateInfo
    {
        [field: SerializeField]
        public ToggleMenuState ToggleState { get; internal set; }

        [field: SerializeField, Range(0, 1)]
        public float RadialState { get; internal set; }

        [field: SerializeField]
        public PuppetMenuState PuppetState { get; internal set; }

        [field: SerializeField]
        public ToggleMode Toggle { get; internal set; }

        [field: SerializeField]
        public int Integer { get; internal set; }

        [field: SerializeField]
        public float Float { get; internal set; }
        
        [field: SerializeField]
        public Color Color { get; internal set; }

        [field: SerializeField]
        public Vector2 Vector2 { get; internal set; }

        [field: SerializeField]
        public Vector3 Vector3 { get; internal set; }

        [field: SerializeField]
        public Vector4 Vector4 { get; internal set; }

        [field: SerializeField]
        public UnityEngine.Object Object { get; internal set; }
    }
}
