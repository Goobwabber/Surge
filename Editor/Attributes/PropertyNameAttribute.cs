﻿using System;

namespace Surge.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class PropertyNameAttribute : Attribute
    {
        public string Value { get; }
        
        public PropertyNameAttribute(string propertyName)
        {
            Value = propertyName;
        }
    }
}