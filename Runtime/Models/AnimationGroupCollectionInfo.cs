using System;
using UnityEngine;

namespace Surge.Models
{
    [Serializable]
    internal class AnimationGroupCollectionInfo
    {
        [field: SerializeField]
        public AnimationGroupInfo[] Groups { get; private set; } = Array.Empty<AnimationGroupInfo>();
    }
}
