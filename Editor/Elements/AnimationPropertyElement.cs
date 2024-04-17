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
using AnimationPropertyInfo = Surge.Models.AnimationPropertyInfo;

namespace Surge.Editor.Elements
{
    internal class AnimationPropertyElement : VisualElement, ISurgeBindable
    {
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

        // vars
        private AnimationGroupType _groupType;
        private bool _onlyArrayItem;
        private Action? _selector;
        private UnityEngine.Object[]? _targetObjects;
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

            _addButton = this.CreateButton("+").WithHeight(20f).WithWidth(20f).WithFontSize(18f).WithFontStyle(FontStyle.Bold);
            _addButton.style.paddingTop = 0f;
            _addButton.style.paddingBottom = 2f;
            _addButton.style.marginLeft = 1f;
            _addButton.style.marginRight = 1f;
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
                _contextTypeProperty.serializedObject.ApplyModifiedProperties();
            });

            this.TrackPropertyValue(_contextTypeProperty, prop =>
            {
                // if field value is already property value, return
                if (prop.stringValue == _componentField.value?.AssemblyQualifiedName)
                    return;
                var type = Type.GetType(prop.stringValue);
                _componentField.value = type;
                _componentField.tooltip = "The component type the target property is located on." + type is not null ? $" ({type})" : string.Empty;
            });

            // Value Type Label
            ValueTypeChanged();
            CheckForIssues();
            this.TrackPropertyValue(_valueTypeProperty, _ => ValueTypeChanged());
            this.TrackPropertyValue(_colorTypeProperty, _ => ValueTypeChanged());
            this.TrackPropertyValue(_objectTypeProperty, _ => ValueTypeChanged());
            void ValueTypeChanged()
            {
                var valueType = (PropertyValueType)_valueTypeProperty.enumValueIndex;
                var colorType = (PropertyColorType)_colorTypeProperty.enumValueIndex;
                var objectType = _objectTypeProperty.stringValue;
                _propertyTypeLabel.Value = GetPropertyTypeName(valueType, colorType, objectType);
                CheckForIssues();
            }

            // Search Button
            _searchButton.clicked -= _selector;
            _selector = () =>
            {
                this.Focus();
                if (_bindingService is null)
                    return; // HOW??
                ShowPropertyWindow(property, _bindingService);
            };
            _searchButton.clicked += _selector;
        }

        public void SetData(Action? onAddRequested, Action? onRemoveRequested, bool lastArrayItem, bool onlyArrayItem, 
            SerializedProperty? objectsProperty, 
            SerializedProperty? sharedValueTypeProperty,
            SerializedProperty? sharedColorTypeProperty,
            SerializedProperty? sharedObjectTypeProperty,
            BindingService? bindingService,
            AnimationGroupType type)
        {
            // set properties
            _sharedValueTypeProperty = sharedValueTypeProperty;
            _sharedColorTypeProperty = sharedColorTypeProperty;
            _sharedObjectTypeProperty = sharedObjectTypeProperty;

            // set vars
            _groupType = type;
            _onlyArrayItem = onlyArrayItem;
            _bindingService = bindingService;

            // ui stuff
            _addButton.clicked -= onAddRequested;
            _addButton.clicked += onAddRequested;
            _addButton.Visible(lastArrayItem);

            // update target objects
            UpdateTargetObjects();
            this.TrackPropertyValue(objectsProperty, _ => UpdateTargetObjects());
            void UpdateTargetObjects()
            {
                if (bindingService is null)
                    return;
                _ = bindingService.TryGetSearchableObjects(out GameObject[]? objects);
                if (objects is not null)
                    _componentField.Push(objects); // we dont care if get fails as long as not null
            }

            // update shared value type
            CheckForIssues();
            // TODO: fuck this.
            //this.TrackPropertyValue(sharedValueTypeProperty!, prop => CheckForIssues());
            //this.TrackPropertyValue(sharedColorTypeProperty!, prop => CheckForIssues());
            //this.TrackPropertyValue(sharedObjectTypeProperty!, prop => CheckForIssues());

            ContextualMenuManipulator rightMenu = new(RightMenuPopulate);
            rightMenu.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
            this.AddManipulator(rightMenu);

            void RightMenuPopulate(ContextualMenuPopulateEvent evt)
            {
                if (!_onlyArrayItem)
                    evt.menu.AppendAction("Remove", evt => onRemoveRequested?.Invoke());
            }
        }

        private void CheckForIssues()
        {
            // aids and not really necessary but im tired.
            if (_valueTypeProperty is null ||
                _colorTypeProperty is null ||
                _objectTypeProperty is null ||
                _sharedValueTypeProperty is null ||
                _sharedColorTypeProperty is null ||
                _sharedObjectTypeProperty is null)
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
            if (!_bindingService.TryGetPropertyBinding(contextType, name, out SurgeProperty binding))
            {
                _issueIndicator.style.backgroundImage = SurgeUI.GetWarningImage();
                _issueIndicator.Visible(true);
                _propertyTypeLabel.Visible(false);
                _issueIndicator.tooltip = $"Animatable property with name \"{name}\" not found on any components of type \"{contextType.Name}\".";

                // didnt find property so murder value types before return
                _valueTypeProperty.SetValue(PropertyValueType.Boolean);
                _colorTypeProperty.SetValue(PropertyColorType.None);
                _objectTypeProperty.SetValue(string.Empty);
                return;
            }

            // *technically* dont need to check color type... should i let the users go wild?
            // pain in the ass to try to change vectors using a color field and vice versa anyways.
            var equivalentTypes = _sharedValueTypeProperty.enumValueIndex == _valueTypeProperty.enumValueIndex
                && _sharedColorTypeProperty.enumValueIndex == _colorTypeProperty.enumValueIndex
                && string.CompareOrdinal(_sharedObjectTypeProperty.stringValue, _objectTypeProperty.stringValue) == 0;

            // property path IS valid, so we should go ahead and take the type values from it
            _valueTypeProperty.SetValue(binding!.Type);
            _colorTypeProperty.SetValue(binding!.Color);
            _objectTypeProperty.SetValue(binding!.GetPseudoProperty(0).Type.AssemblyQualifiedName);

            if (!equivalentTypes)
            {
                var valueType = (PropertyValueType)_valueTypeProperty.enumValueIndex;
                var colorType = (PropertyColorType)_colorTypeProperty.enumValueIndex;
                var objectType = _objectTypeProperty.stringValue;
                var typeName = GetPropertyTypeName(valueType, colorType, objectType);
                var sharedValueType = (PropertyValueType)_sharedValueTypeProperty.enumValueIndex;
                var sharedColorType = (PropertyColorType)_sharedColorTypeProperty.enumValueIndex;
                var sharedObjectType = _sharedObjectTypeProperty.stringValue;
                var sharedTypeName = GetPropertyTypeName(sharedValueType, sharedColorType, sharedObjectType);
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

        private static string GetPropertyTypeName(PropertyValueType valueType, PropertyColorType colorType, string objectType)
        {
            return valueType switch
            {
                PropertyValueType.Boolean => "Bool",
                PropertyValueType.Integer => "Int",
                PropertyValueType.Float => "Float",
                PropertyValueType.Vector2 => "Vector2",
                PropertyValueType.Vector3 or PropertyValueType.Vector4 => colorType switch
                {
                    PropertyColorType.None => valueType is PropertyValueType.Vector3 ? "Vector3" : "Vector4",
                    PropertyColorType.RGB => "RGB",
                    PropertyColorType.HDR => "HDR",
                    _ => "<null>",
                },
                PropertyValueType.Object => Type.GetType(objectType)?.Name ?? "Object",
                _ => "<null>",
            };
        }
    }
}
