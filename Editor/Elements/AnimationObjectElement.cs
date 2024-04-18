using Surge.Editor.Extensions;
using Surge.Editor.Models;
using Surge.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Surge.Editor.Elements
{
    internal class AnimationObjectElement : VisualElement, ISurgeBindable
    {
        public static string Name = "Object";

        private AnimationGroupType _groupType;

        private readonly ObjectField _objectField;
        private readonly ComponentDropdownField _componentField;

        public AnimationObjectElement()
        {
            this.style.flexDirection = FlexDirection.Row;

            _objectField = new ObjectField().WithFontSize(12f).WithGrow(1f).WithHeight(20f);
            _objectField.RegisterValueChangedCallback(ObjectFieldValueChanged);
            _objectField.style.marginRight = 1f;
            _objectField.style.flexBasis = 1f;
            this.Add(_objectField);

            _componentField = new ComponentDropdownField(null).WithHeight(20f);
            _componentField.style.marginLeft = 1f;
            _componentField.style.marginRight = 1f;
            this.Add(_componentField);
        }

        public void SetBinding(SerializedProperty property)
        {
            _objectField.BindProperty(property);
            _componentField.BindProperty(property);
        }

        public void SetData(AnimationGroupType type)
        {
            _groupType = type;
            _componentField.Visible(type == AnimationGroupType.ObjectToggle);
        }

        private void ObjectFieldValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            _componentField.Visible(evt.newValue != null && _groupType is AnimationGroupType.ObjectToggle);
            _componentField.Push(evt.newValue);
        }
    }
}
