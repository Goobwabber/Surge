using System;
using UnityEngine;

namespace Flare.Models
{
    [Serializable]
    internal class AnimationGroupCollectionInfo
    {
        [field: SerializeField]
        public AnimationGroupInfo[] Groups { get; private set; } = Array.Empty<AnimationGroupInfo>();
    }
}
