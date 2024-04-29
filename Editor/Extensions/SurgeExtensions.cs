using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using Surge.Models;
using System;
using System.Collections.Generic;

namespace Surge.Editor.Extensions
{
    internal static class SurgeExtensions
    {
        public static DeclaringPass Run<T>(this Sequence sequence) where T : Pass<T>, new()
        {
            return sequence.Run(new T());
        }

        // this is aids but it works so whatever
        private static Dictionary<PropertyValueType, ValueTypeFilter> _valueTypeDict = new();
        private static Dictionary<PropertyColorType, ColorTypeFilter> _colorTypeDict = new();

        public static bool HasType(this ValueTypeFilter filter, PropertyValueType flag)
        {
            if (_valueTypeDict.TryGetValue(flag, out var value))
                return filter.HasFlag(value);
            var newValue = (ValueTypeFilter)Enum.Parse(typeof(ValueTypeFilter), flag.ToString());
            _valueTypeDict.Add(flag, newValue);
            return filter.HasFlag(newValue);
        }

        public static bool HasType(this ColorTypeFilter filter, PropertyColorType flag)
        {
            if (_colorTypeDict.TryGetValue(flag, out var value))
                return filter.HasFlag(value);
            var newValue = (ColorTypeFilter)Enum.Parse(typeof(ColorTypeFilter), flag.ToString());
            _colorTypeDict.Add(flag, newValue);
            return filter.HasFlag(newValue);
        }
    }
}