using Flare.Editor.Extensions;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flare.Editor.Elements
{
    internal class LabelledEnumField : VisualElement
    {
        private EnumField _enumField;
        private Label _label;
        private TextElement _value;

        public LabelledEnumField(Enum defaultValue, SerializedProperty property, string label)
        {
            _enumField = new EnumField(defaultValue).WithHeight(20f);
            _enumField.BindProperty(property);
            _label = new Label(label).WithPadding(0f).WithMarginTop(1f);
            _value = _enumField.Q<TextElement>();
            _value.parent.Insert(0, _label);
            _value.WithColor(FlareUI.FullColor);
            _enumField.style.marginRight = 1f;
            _enumField.style.marginLeft = 2f;
            Add(_enumField);
        }

        public LabelledEnumField(Enum defaultValue, SerializedProperty property, string label, string tooltip) : this(defaultValue, property, label)
        {
            this.tooltip = tooltip;
        }

        public LabelledEnumField(Enum defaultValue, SerializedProperty property, string label, string tooltip, Func<int, Color32> enumColorFunction) : this(defaultValue, property, label, tooltip)
        {
            _enumField.RegisterValueChangedCallback(evt =>
                _value.WithColor(enumColorFunction(property.enumValueIndex)));
        }

        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<Enum>> callback)
            => _enumField.RegisterValueChangedCallback<Enum>(callback);

        public void UnregisterValueChangedCallback(EventCallback<ChangeEvent<Enum>> callback)
            => _enumField.UnregisterValueChangedCallback<Enum>(callback);
    }
}
