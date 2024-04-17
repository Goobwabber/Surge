using UnityEngine;

namespace Surge.Models
{
    internal enum ControlState
    {
        [InspectorName("When Active")]
        Enabled,
        
        [InspectorName("When Inactive")]
        Disabled
    }
}