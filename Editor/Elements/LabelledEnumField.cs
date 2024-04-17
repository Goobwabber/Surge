using Surge.Editor.Extensions;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Surge.Editor.Elements
{
    internal class LabelledEnumField : VisualElement
    {
        private EnumField _enumField;
        private Label _label;
        private TextElement _value;

        private Func<int, Color32>? _enumColorFunction;
        private EventCallback<ChangeEvent<Enum>>? _valueChangedColorCallback;

        public LabelledEnumField(Enum defaultValue, string label, SerializedProperty? property = null)
        {
            _enumField = new EnumField(defaultValue).WithHeight(20f);
            if (property is not null)
                _enumField.BindProperty(property);
            _label = new Label(label).WithPadding(0f).WithMarginTop(1f);
            _value = _enumField.Q<TextElement>();
            _value.parent.Insert(0, _label);
            _value.WithColor(SurgeUI.FullColor);
            _enumField.style.marginRight = 1f;
            _enumField.style.marginLeft = 2f;
            Add(_enumField);
        }

        public LabelledEnumField(Enum defaultValue, string label, string tooltip, SerializedProperty? property = null) : this(defaultValue, label, property)
        {
            this.tooltip = tooltip;
        }

        public LabelledEnumField(Enum defaultValue, string label, string tooltip, Func<int, Color32> enumColorFunction, SerializedProperty? property = null) : this(defaultValue, label, tooltip, property)
        {
            _enumColorFunction = enumColorFunction;
            if (property is null)
                return;
            _value.WithColor(enumColorFunction(property.enumValueIndex));
            _enumField.RegisterValueChangedCallback(_valueChangedColorCallback = evt =>
                _value.WithColor(enumColorFunction(property.enumValueIndex)));
        }

        public void BindProperty(SerializedProperty property)
        {
            _enumField.BindProperty(property);
            if (_enumColorFunction is null)
                return;
            if (_valueChangedColorCallback is not null)
                _enumField.UnregisterValueChangedCallback(_valueChangedColorCallback);
            _value.WithColor(_enumColorFunction(property.enumValueIndex));
            _enumField.RegisterValueChangedCallback(_valueChangedColorCallback = evt =>
                _value.WithColor(_enumColorFunction(property.enumValueIndex)));
        }

        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<Enum>> callback)
            => _enumField.RegisterValueChangedCallback<Enum>(callback);

        public void UnregisterValueChangedCallback(EventCallback<ChangeEvent<Enum>> callback)
            => _enumField.UnregisterValueChangedCallback<Enum>(callback);
    }
}
