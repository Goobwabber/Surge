using Surge.Editor.Extensions;
using Surge.Editor.Models;
using Surge.Editor.Services;
using Surge.Editor.Windows;
using Surge.Models;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Surge.Editor.Elements
{
    internal class AnimationPropertyElement : VisualElement, ISurgeBindable
    {
        public static string Name = "Animation Property";

        // properties
        private SerializedProperty? _nameProperty;
        private SerializedProperty? _contextTypeProperty;
        // properties: value types
        private SerializedProperty? _valueTypeProperty;
        private SerializedProperty? _colorTypeProperty;
        private SerializedProperty? _objectTypeProperty;
        // properties: shared value types
        private SerializedProperty? _sharedValueTypeProperty;
        private SerializedProperty? _sharedColorTypeProperty;
        private SerializedProperty? _sharedObjectTypeProperty;
        // properties: ignore warnings property
        private SerializedProperty? _ignoreWarningsProperty;

        // vars
        private AnimationGroupType _groupType;
        private bool _firstArrayItem;
        private Action? _selector;
        private BindingService? _bindingService;

        // ui
        private readonly ComponentTypeDropdownField _componentField;
        private readonly TextField _propertyNameField;
        private readonly SettingLabelElement _propertyTypeLabel;
        private readonly VisualElement _issueIndicator;
        private readonly Button _searchButton;
        private readonly Button _addButton;

        public AnimationPropertyElement()
        {
            this.style.flexDirection = FlexDirection.Row;

            _componentField = new ComponentTypeDropdownField(null).WithHeight(20f).WithShrink(1f);
            _componentField.style.marginLeft = 3f;
            _componentField.style.marginRight = 1f;
            _componentField.style.textOverflow = TextOverflow.Ellipsis;
            _componentField.tooltip = "The component type the target property is located on.";
            _componentField.RemoveDuplicateTypes = true;
            this.Add(_componentField);

            _propertyNameField = new TextField().WithHeight(20f).WithGrow(1f);
            _propertyNameField.style.marginLeft = 1f;
            _propertyNameField.style.marginRight = 1f;
            this.Add(_propertyNameField);

            var nameInputBox = _propertyNameField[0];
            nameInputBox.WithPadding(0f);
            var nameTextElement = nameInputBox[0];
            nameTextElement.style.marginTop = 1f;
            nameTextElement.style.marginLeft = 2f;
            nameTextElement.style.marginRight = 2f;

            _propertyTypeLabel = new SettingLabelElement("type", 16f).WithFontSize(10f);
            _propertyTypeLabel.style.paddingLeft = 2f;
            _propertyTypeLabel.style.paddingRight = 2f;
            //_propertyTypeLabel.style.marginLeft = 0f;
            _propertyTypeLabel.style.marginRight = -1f;
            nameInputBox.Insert(0, _propertyTypeLabel);

            _issueIndicator = new VisualElement().WithMargin(2f).WithWidth(15f).WithHeight(14f);
            _issueIndicator.style.marginTop = 3f;
            _issueIndicator.style.marginRight = -2f;
            _issueIndicator.style.backgroundImage = SurgeUI.GetErrorImage();
            nameInputBox.Insert(0, _issueIndicator);

            _searchButton = nameInputBox.CreateButton(string.Empty).WithWidth(18f).WithHeight(18f).WithMargin(0f).WithBorderRadius(0f);
            _searchButton.style.backgroundImage = SurgeUI.GetSearchImage();
            _searchButton.AddToClassList(ObjectField.selectorUssClassName);
        }

        public void SetBinding(SerializedProperty property)
        {
            // We need to copy this property for the Select Property callback.
            // Otherwise, it could (will) be iterated over.
            property = property.Copy();

            // get props
            _contextTypeProperty = property.Property(nameof(AnimationPropertyInfo.ContextType));
            _valueTypeProperty = property.Property(nameof(AnimationPropertyInfo.ValueType));
            _colorTypeProperty = property.Property(nameof(AnimationPropertyInfo.ColorType));
            _objectTypeProperty = property.Property(nameof(AnimationPropertyInfo.ObjectValueType));
            _nameProperty = property.Property(nameof(AnimationPropertyInfo.Name));

            // Bind
            _propertyNameField.BindProperty(_nameProperty);
            _propertyNameField.RegisterValueChangedCallback(_ => CheckForIssues());
            // only track the first value type property, bc they should all get updated at the same time :clueless:
            this.TrackPropertyValue(_valueTypeProperty, _ => UpdateValueType());

            // Component Field
            _componentField.value = Type.GetType(_contextTypeProperty.stringValue);
            if (_componentField.value is not null)
                _componentField.tooltip = $"The component type the target property is located on. ({_componentField.value.Name})";
            _componentField.RegisterValueChangedCallback(evt =>
            {
                // if null or if property is already set to value, return
                if (evt.newValue is null || evt.newValue.AssemblyQualifiedName == _contextTypeProperty.stringValue)
                    return;
                _componentField.tooltip = $"The component type the target property is located on. ({evt.newValue.Name})";
                _contextTypeProperty.SetValue(evt.newValue.AssemblyQualifiedName);
                CheckForIssues();
            });

            this.TrackPropertyValue(_contextTypeProperty, prop =>
            {
                // if field value is already property value, return
                if (prop.stringValue == _componentField.value?.AssemblyQualifiedName)
                    return;
                var type = Type.GetType(prop.stringValue);
                _componentField.value = type;
                _componentField.tooltip = "The component type the target property is located on." + type is not null ? $" ({type})" : string.Empty;
                CheckForIssues();
            });

            // Search Button
            _searchButton.clicked -= _selector;
            _selector = () =>
            {
                this.Focus(); // make ew ew ugly selection go away
                if (_bindingService is null)
                    return; // HOW??
                ShowPropertyWindow(property, _bindingService);
            };
            _searchButton.clicked += _selector;

            // Update things on ui load
            this.schedule.Execute(UpdateTargetObjects);
            this.schedule.Execute(UpdateValueType);
        }

        public void SetData(
            bool firstArrayItem,
            SerializedProperty? objectsProperty, 
            SerializedProperty? groupProperty,
            BindingService? bindingService,
            AnimationGroupType type)
        {
            // unbind (SetData runs before SetBinding)
            this.Unbind();

            // set props
            _sharedValueTypeProperty = groupProperty.Property(nameof(AnimationGroupInfo.SharedValueType));
            _sharedColorTypeProperty = groupProperty.Property(nameof(AnimationGroupInfo.SharedColorType));
            _sharedObjectTypeProperty = groupProperty.Property(nameof(AnimationGroupInfo.SharedObjectType));
            _ignoreWarningsProperty = groupProperty.Property(nameof(AnimationGroupInfo.IgnoreWarnings));

            // set vars
            _groupType = type;
            _firstArrayItem = firstArrayItem;
            _bindingService = bindingService;

            // bindings
            // schedule the UpdateTargetObjects call, so that it doesnt execute before bindingService has been updated.
            this.TrackPropertyValue(objectsProperty, _ => this.schedule.Execute(UpdateTargetObjects));
            // only track the first value type property, bc they should all get updated at the same time :clueless:
            this.TrackPropertyValue(_sharedValueTypeProperty!, prop => CheckForIssues());
            this.TrackPropertyValue(_ignoreWarningsProperty!, prop => CheckForIssues());
        }

        private void UpdateTargetObjects()
        {
            if (_bindingService is null)
                return;
            _ = _bindingService.TryGetSearchableObjects(out GameObject[]? objects);
            if (objects is not null)
                _componentField.Push(objects); // we dont care if get fails as long as not null
            CheckForIssues();
        }

        private void UpdateValueType()
        {
            var valueType = (PropertyValueType)_valueTypeProperty.enumValueIndex;
            var colorType = (PropertyColorType)_colorTypeProperty.enumValueIndex;
            var objectType = _objectTypeProperty.stringValue;
            _propertyTypeLabel.Value = SurgeUI.GetPropertyTypeName(valueType, colorType, objectType);

            if (_firstArrayItem)
            {
                // update all of these at the same time.
                _sharedValueTypeProperty.SetValueNoRecord(valueType);
                _sharedColorTypeProperty.SetValueNoRecord(colorType);
                _sharedObjectTypeProperty.SetValueNoRecord(objectType);
                EditorUtility.SetDirty(_sharedValueTypeProperty.serializedObject.targetObject);
                _sharedValueTypeProperty.serializedObject.ApplyModifiedProperties();
            }

            CheckForIssues();
        }

        private void CheckForIssues()
        {
            // aids and not really necessary but im tired.
            if (_valueTypeProperty is null ||
                _colorTypeProperty is null ||
                _objectTypeProperty is null ||
                _sharedValueTypeProperty is null ||
                _sharedColorTypeProperty is null ||
                _sharedObjectTypeProperty is null ||
                _ignoreWarningsProperty is null ||
                _bindingService is null)
                return;

            var name = _propertyNameField.value;
            if (string.IsNullOrEmpty(name))
            {
                _issueIndicator.Visible(false);
                _propertyTypeLabel.Visible(false);
                return;
            }

            var contextType = _componentField.value;
            // TODO: make asynchronous :plead:
            if (!_bindingService.TryGetPropertyBinding(contextType, name, out SurgeProperty binding, out bool pseudo))
            {
                _issueIndicator.style.backgroundImage = SurgeUI.GetWarningImage();
                _issueIndicator.Visible(!_ignoreWarningsProperty.boolValue);
                _propertyTypeLabel.Visible(false);
                _issueIndicator.tooltip = $"Animatable property with name \"{name}\" not found on any components of type \"{contextType?.Name ?? "<null>"}\".";

                // didnt find property so murder value types before return (and apply them at the same time)
                // Note: I commented out this code because i think its a better experience for the user if we dont do it
                //_valueTypeProperty.SetValueNoRecord(PropertyValueType.Boolean);
                //_colorTypeProperty.SetValueNoRecord(PropertyColorType.None);
                //_objectTypeProperty.SetValueNoRecord(string.Empty);
                //EditorUtility.SetDirty(_valueTypeProperty.serializedObject.targetObject);
                //_valueTypeProperty.serializedObject.ApplyModifiedProperties();
                return;
            }

            // property path IS valid, so we should go ahead and take the type values from it
            _valueTypeProperty.SetValueNoRecord(!pseudo ? binding!.Type : PropertyValueType.Float);
            _colorTypeProperty.SetValueNoRecord(!pseudo ? binding!.Color : PropertyColorType.None);
            _objectTypeProperty.SetValueNoRecord(binding!.ObjectType?.AssemblyQualifiedName ?? string.Empty);
            EditorUtility.SetDirty(_valueTypeProperty.serializedObject.targetObject);
            _valueTypeProperty.serializedObject.ApplyModifiedProperties();

            // *technically* dont need to check color type... should i let the users go wild?
            // pain in the ass to try to change vectors using a color field and vice versa anyways.
            var equivalentTypes = _sharedValueTypeProperty.enumValueIndex == _valueTypeProperty.enumValueIndex
                && _sharedColorTypeProperty.enumValueIndex == _colorTypeProperty.enumValueIndex
                && string.CompareOrdinal(_sharedObjectTypeProperty.stringValue, _objectTypeProperty.stringValue) == 0;

            if (!equivalentTypes)
            {
                var valueType = (PropertyValueType)_valueTypeProperty.enumValueIndex;
                var colorType = (PropertyColorType)_colorTypeProperty.enumValueIndex;
                var objectType = _objectTypeProperty.stringValue;
                var typeName = SurgeUI.GetPropertyTypeName(valueType, colorType, objectType);
                var sharedValueType = (PropertyValueType)_sharedValueTypeProperty.enumValueIndex;
                var sharedColorType = (PropertyColorType)_sharedColorTypeProperty.enumValueIndex;
                var sharedObjectType = _sharedObjectTypeProperty.stringValue;
                var sharedTypeName = SurgeUI.GetPropertyTypeName(sharedValueType, sharedColorType, sharedObjectType);
                _issueIndicator.style.backgroundImage = SurgeUI.GetErrorImage();
                _propertyTypeLabel.Visible(true);
                _issueIndicator.Visible(true);
                _issueIndicator.tooltip = $"Property has value type \"{typeName}\" which does not match the first property value's type, \"{sharedTypeName}\".";
                return;
            }

            _propertyTypeLabel.Visible(true);
            _issueIndicator.Visible(false);
        }

        private static void ShowPropertyWindow(SerializedProperty? property, BindingService bindingService)
        {
            if (property is null)
                return;
            
            PropertySelectorWindow.Present(property, bindingService);
        }
    }
}
