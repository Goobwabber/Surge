using System;
using UnityEngine;
using VRC.SDKBase;

namespace Surge.Models
{
    [Serializable]
    internal class TagInfo
    {
        [field: SerializeField]
        public SurgeTags? Module { get; private set; }
        
        [field: SerializeField]
        public SurgeTag[] Tags { get; private set; } = Array.Empty<SurgeTag>();

        public bool EnsureValidated(GameObject gameObject, bool skipLengthCheck = false)
        {
            if (Tags.Length is 0 && !skipLengthCheck)
                return false;

            var descriptor = gameObject.GetComponentInParent<VRC_AvatarDescriptor>();
            if (!descriptor)
                return false;

            var layerModule = descriptor.GetComponentInChildren<SurgeTags>();
            if (layerModule)
                return true;

            GameObject module = new("Surge Tag Module");
            module.transform.SetParent(descriptor.transform);
            Module = module.AddComponent<SurgeTags>();
            
            var moduleTransform = module.transform;
            moduleTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            moduleTransform.localScale = Vector3.one;
            
            return true;
        }
    }
}