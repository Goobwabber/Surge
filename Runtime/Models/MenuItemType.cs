using UnityEngine;

namespace Surge.Models
{
    internal enum MenuItemType
    {
        Button,
        Toggle,
        Radial,
        [InspectorName("Four Axis")]
        FourAxis,
    }
}