using UnityEngine;

namespace Flare.Models
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