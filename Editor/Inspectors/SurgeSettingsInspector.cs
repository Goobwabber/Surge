using Surge.Editor.Attributes;
using Surge.Editor.Extensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Surge.Editor.Inspectors
{
    [CustomEditor(typeof(SurgeSettings))]
    internal class SurgeSettingsInspector : SurgeInspector
    {
        [PropertyName(nameof(SurgeSettings.WriteDefaults))]
        private readonly SerializedProperty _writeDefaultsProperty = null!;
        
        protected override VisualElement BuildUI(VisualElement root)
        {
            root.CreatePropertyField(_writeDefaultsProperty).WithLabel("Use Write Defaults");
            return root;
        }
    }
}