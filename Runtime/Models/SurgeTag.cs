using System;
using UnityEngine;

namespace Surge.Models
{
    [Serializable]
    internal class SurgeTag
    {
        [field: SerializeField]
        public string Value { get; set; } = string.Empty;
    }
}