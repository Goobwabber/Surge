using UnityEngine;

namespace Surge
{
    [DisallowMultipleComponent]
    internal class SurgeSettings : SurgeModule
    {
        [field: SerializeField]
        public bool WriteDefaults { get; set; }
    }
}