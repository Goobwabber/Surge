using System;

namespace Surge.Models
{
    internal enum PropertyColorType
    {
        None,
        RGB,
        HDR,
    }

    [Flags] // these need to have the same names
    internal enum ColorTypeFilter
    {
        None = 1,
        RGB = 2,
        HDR = 4,
        Color = RGB | HDR,
    }
}