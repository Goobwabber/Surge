using System;
using UnityEngine;

namespace Surge.Models
{
    [Serializable]
    internal class InterpolationInfo
    {
        [field: SerializeField, Min(0)]
        public float Duration { get; set; }
    }
}