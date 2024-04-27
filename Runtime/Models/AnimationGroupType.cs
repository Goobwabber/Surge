using UnityEngine;

namespace Surge.Models
{
    internal enum AnimationGroupType
    {
        [InspectorName("Object Toggle")]
        ObjectToggle,
        [InspectorName("Property")]
        Normal,
        [InspectorName("Global Property")]
        Avatar
    }
}