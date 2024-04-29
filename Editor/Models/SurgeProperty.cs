using System;
using System.Collections.Generic;
using Surge.Models;
using UnityEngine;

namespace Surge.Editor.Models
{
    internal class SurgeProperty
    {
        private string? _id;
        private string? _internalId;

        public string Id => _id ??= $"{Path}/{Name}";

        public string Name { get; }
        
        public string Path { get; }
        
        public Type ContextType { get; }
        
        public PropertyValueType Type { get; }
        
        public PropertyColorType Color { get; }

        public Type? ObjectType { get; }
        
        public GameObject GameObject { get; }
        
        public SurgePropertySource Source { get; }
        
        private SurgePseudoProperty PseudoProperty { get; }
        
        internal IReadOnlyList<SurgePseudoProperty>? PseudoProperties { get; }
        
        // Only used for matching in UI, basically is a unique property name for a specific type and name.
        public string QualifiedId => _internalId ??= $"{Name}::{ContextType.AssemblyQualifiedName}";

        public int Length => PseudoProperties?.Count ?? 1;

        public SurgeProperty(string name, string path, Type contextType, PropertyValueType type, PropertyColorType color, Type? objectType,
            SurgePropertySource source, GameObject gameObject, SurgePseudoProperty? pseudoProperty, IReadOnlyList<SurgePseudoProperty>? pseudoProperties)
        {
            Type = type;
            Name = name;
            Path = path;
            Color = color;
            ObjectType = objectType;
            Source = source;
            GameObject = gameObject;
            ContextType = contextType;

            PseudoProperty = pseudoProperty!;
            PseudoProperties = pseudoProperties;

            if (pseudoProperty == null && pseudoProperties != null)
                PseudoProperty = pseudoProperties[0];
        }

        public SurgePseudoProperty GetPseudoProperty(int index)
        {
            if (index == 0)
                return PseudoProperty;

            if (PseudoProperties is null)
                throw new InvalidOperationException($"Cannot find pseudo property with index {index}");

            return PseudoProperties[index];
        }
    }
}