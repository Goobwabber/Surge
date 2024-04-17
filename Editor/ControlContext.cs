using System.Collections.Generic;
using Surge.Editor.Models;
using Surge.Models;
using JetBrains.Annotations;

namespace Surge.Editor
{
    [PublicAPI]
    internal class ControlContext
    {
        private readonly List<AnimatableBinaryProperty> _properties = new();
        
        public string Id { get; }

        public bool IsBinary => Control.MenuItem.Type is MenuItemType.Toggle or MenuItemType.Button;
        
        public SurgeControl Control { get; }

        public IReadOnlyList<AnimatableBinaryProperty> Properties => _properties;
        
        public ControlContext(string id, SurgeControl control)
        {
            Id = id;
            Control = control;
        }

        public void AddProperty(AnimatableBinaryProperty property)
        {
            _properties.Add(property);
        }
    }
}