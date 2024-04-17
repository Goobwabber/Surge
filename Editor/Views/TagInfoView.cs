using Surge.Editor.Attributes;
using Surge.Editor.Extensions;
using Surge.Models;
using UnityEditor;
using UnityEngine.UIElements;

namespace Surge.Editor.Views
{
    internal class TagInfoView : IView
    {
        //private const string _helpText = "Tags allow you to trigger many controls at once using a simple rule system. When a tag is enabled or disabled, a rule can be triggered which causes controls of another tag to be activated/deactivated. Create new tags in the Surge Tag Module";

        [PropertyName(nameof(TagInfo.Module))]
        private readonly SerializedProperty _moduleProperty = null!;
        
        [PropertyName(nameof(TagInfo.Tags))]
        private readonly SerializedProperty _tagsProperty = null!;
        
        public void Build(VisualElement root)
        {
            HelpBox help = new("This feature is subject to breaking changes in the future.", HelpBoxMessageType.Warning);
            root.Add(help);

            root.CreateButton("Go To Tag Module", () =>
            {
                var control = (_tagsProperty.serializedObject.targetObject as SurgeControl)!;
                if (!control.TagInfo.EnsureValidated(control.gameObject, true))
                    return;
                
                Selection.activeObject = control.TagInfo.Module;
            });

            root.CreatePropertyField(_tagsProperty);
        }
    }
}