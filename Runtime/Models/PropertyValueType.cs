using System;

namespace Surge.Models
{
    internal enum PropertyValueType
    {
        Boolean,
        Integer,
        Float,
        Vector2,
        Vector3,
        Vector4,
        Object,
    }

    [Flags] // these need to have the same names
    internal enum ValueTypeFilter
    {
        Boolean = 1,
        Integer = 2,
        Float = 4,
        Vector2 = 8,
        Vector3 = 16,
        Vector4 = 32,
        Vector = Vector2 | Vector3 | Vector4,
        Color = Vector3 | Vector4,
        Object = 64,
    }
}