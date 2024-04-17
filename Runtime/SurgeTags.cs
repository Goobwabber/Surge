using System;
using Surge.Models;
using UnityEngine;

namespace Surge
{
    internal class SurgeTags : SurgeModule
    {
        [field: SerializeField]
        public string[] Tags { get; private set; } = Array.Empty<string>();

        [field: SerializeField]
        public SurgeRule[] Rules { get; private set; } = Array.Empty<SurgeRule>();
    }
}