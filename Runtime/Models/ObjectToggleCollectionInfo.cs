using System;
using UnityEngine;

namespace Surge.Models
{
    [Serializable]
    internal class ObjectToggleCollectionInfo
    {
        [field: SerializeField]
        public ObjectToggleInfo[] Toggles { get; private set; } = Array.Empty<ObjectToggleInfo>();
    }
}