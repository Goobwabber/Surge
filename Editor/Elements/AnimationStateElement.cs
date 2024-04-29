using Surge.Editor.Extensions;
using Surge.Editor.Models;
using Surge.Models;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Surge.Editor.Elements
{
    internal class AnimationStateElement : VisualElement, ISurgeBindable
    {
        public const float VectorFieldWidth = 40f;

        // properties
        private SerializedProperty? _valueTypeProperty;
        private SerializedProperty? _colorTypeProperty;
        private SerializedProperty? _objectTypeProperty;

        // values
        private bool _lastArrayItem;
        private int _arraySize;

        // when menu state
        private readonly Label _whenLabel;
        private readonly LabelledEnumField _toggleStateField;
        private readonly Slider _radialStateSlider;
        private readonly FloatField _radialStateField;
        private readonly LabelledEnumField _puppetStateField;

        // set value to
        private readonly Label _setLabel;
        private readonly LabelledEnumField _toggleField;
        private readonly IntegerField _integerField;
        private readonly FloatField _floatField;
        private readonly ColorField _colorField;
        private readonly Vector2Field _vector2Field;
        private readonly Vector3Field _vector3Field;
        private readonly Vector4Field _vector4Field;
        private readonly ObjectField _objectField;

        public AnimationStateElement()
        {
            this.style.flexDirection = FlexDirection.Row;

            // When state stuff
            var whenStateElement = new VisualElement().WithGrow(1f);
            whenStateElement.style.flexDirection = FlexDirection.Row;
            whenStateElement.style.justifyContent = Justify.Center;
            whenStateElement.tooltip = "The menu state that controls when the value is set.";
            this.Add(whenStateElement);

            _whenLabel = whenStateElement.CreateLabel("When control is").WithHeight(20f);
            _whenLabel.style.alignSelf = Align.FlexStart;
            _whenLabel.style.marginTop = 0f;
            _whenLabel.style.paddingTop = 2f;
            _whenLabel.style.marginRight = 4f;
            _whenLabel.style.marginLeft = 4f;
            _whenLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            _toggleStateField = new LabelledEnumField(ToggleMenuState.Active, "", whenStateElement.tooltip,
                value => value == (int)ToggleMenuState.Inactive ? SurgeUI.DisabledColor : SurgeUI.EnabledColor);
            _toggleStateField.WithWidth(75f);
            whenStateElement.Add(_toggleStateField);

            _radialStateSlider = new Slider(0f, 1f).WithHeight(20f).WithGrow(1f);
            _radialStateSlider.style.marginRight = 1f;
            _radialStateField = new FloatField().WithHeight(20f).WithWidth(40f);
            _radialStateField.style.marginRight = 2f;
            whenStateElement.Add(_radialStateSlider);
            whenStateElement.Add(_radialStateField);

            _puppetStateField = new LabelledEnumField(PuppetMenuState.Up, "State: ");
            _puppetStateField.WithWidth(95f);
            //_puppetStateField.style.marginRight = StyleKeyword.Auto;
            whenStateElement.Add(_puppetStateField);

            // Set values stuff
            var setValuesElement = new VisualElement();
            setValuesElement.style.flexDirection = FlexDirection.Row;
            setValuesElement.tooltip = "The value to set the properties to when the menu is in this state.";
            this.Add(setValuesElement);

            _setLabel = setValuesElement.CreateLabel("Set values to").WithHeight(20f);
            _setLabel.style.alignSelf = Align.FlexStart;
            _setLabel.style.marginTop = 0f;
            _setLabel.style.paddingTop = 2f;
            _setLabel.style.marginRight = 1f;
            _setLabel.style.marginLeft = 3f;
            _setLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            _toggleField = new LabelledEnumField(ToggleMode.Enabled, "", setValuesElement.tooltip,
                value => value == (int)ToggleMode.Disabled ? SurgeUI.DisabledColor : SurgeUI.EnabledColor);
            _toggleField.WithWidth(75f);
            setValuesElement.Add(_toggleField);

            _integerField = new IntegerField("Set values to");
            _floatField = new FloatField("Set values to");
            _colorField = new ColorField();
            _vector2Field = new Vector2Field();
            _vector3Field = new Vector3Field();
            _vector4Field = new Vector4Field();
            _objectField = new ObjectField();

            // fix styles
            _colorField.style.alignItems = Align.Center;
            _colorField.style.marginRight = 1f;
            _floatField.style.marginRight = 1f;
            _integerField.style.marginRight = 1f;
            _vector2Field.style.marginRight = 1f;
            _vector3Field.style.marginRight = 1f;
            _vector4Field.style.marginRight = 1f;
            _objectField.style.marginRight = 1f;
            // fix styles: field labels
            _integerField[0].style.marginTop = 1f;
            _integerField[0].style.minWidth = 0f;
            _floatField[0].style.marginTop = 1f;
            _floatField[0].style.minWidth = 0f;
            // fix styles: delete labels for vectors
            _vector2Field[0].RemoveAt(2); // there is a random last element in vector2 fields
            foreach (var child in _vector2Field[0].Children())
            {
                child.RemoveAt(0);
                child[0].style.marginLeft = 1f;
                child.WithWidth(VectorFieldWidth).WithBasis(StyleKeyword.Auto).WithGrow(0f);
            }
            foreach (var child in _vector3Field[0].Children())
            {
                child.RemoveAt(0);
                child[0].style.marginLeft = 1f;
                child.WithWidth(VectorFieldWidth).WithBasis(StyleKeyword.Auto).WithGrow(0f);
            }
            foreach (var child in _vector4Field[0].Children())
            {
                child.RemoveAt(0);
                child[0].style.marginLeft = 1f;
                child.WithWidth(VectorFieldWidth).WithBasis(StyleKeyword.Auto).WithGrow(0f);
            }

            setValuesElement.Add(_integerField);
            setValuesElement.Add(_floatField);
            setValuesElement.Add(_colorField);
            setValuesElement.Add(_vector2Field);
            setValuesElement.Add(_vector3Field);
            setValuesElement.Add(_vector4Field);
            setValuesElement.Add(_objectField);
        }

        public void SetBinding(SerializedProperty property)
        {
            // when menu state
            var toggleStateProperty = property.Property(nameof(AnimationStateInfo.ToggleState));
            _toggleStateField.BindProperty(toggleStateProperty);
            var radialStateProperty = property.Property(nameof(AnimationStateInfo.RadialState));
            _radialStateSlider.BindProperty(radialStateProperty);
            _radialStateField.BindProperty(radialStateProperty);
            var puppetStateProperty = property.Property(nameof(AnimationStateInfo.PuppetState));
            _puppetStateField.BindProperty(puppetStateProperty);

            // set values to
            var toggleProperty = property.Property(nameof(AnimationStateInfo.Toggle));
            _toggleField.BindProperty(toggleProperty);
            var integerProperty = property.Property(nameof(AnimationStateInfo.Integer));
            _integerField.BindProperty(integerProperty);
            var floatProperty = property.Property(nameof(AnimationStateInfo.Float));
            _floatField.BindProperty(floatProperty);
            var colorProperty = property.Property(nameof(AnimationStateInfo.Color));
            _colorField.BindProperty(colorProperty);
            var vector2Property = property.Property(nameof(AnimationStateInfo.Vector2));
            _vector2Field.BindProperty(vector2Property);
            var vector3Property = property.Property(nameof(AnimationStateInfo.Vector3));
            _vector3Field.BindProperty(vector3Property);
            var vector4Property = property.Property(nameof(AnimationStateInfo.Vector4));
            _vector4Field.BindProperty(vector4Property);
            var objectProperty = property.Property(nameof(AnimationStateInfo.Object));
            _objectField.BindProperty(objectProperty);
        }

        public void SetData(bool lastArrayItem, int arraySize, 
            SerializedProperty? menuProperty, 
            SerializedProperty? valueTypeProperty,
            SerializedProperty? colorTypeProperty,
            SerializedProperty? objectTypeProperty,
            AnimationGroupType groupType)
        {
            // set properties
            _valueTypeProperty = valueTypeProperty;
            _colorTypeProperty = colorTypeProperty;
            _objectTypeProperty = objectTypeProperty;

            // set values
            _lastArrayItem = lastArrayItem;
            _arraySize = arraySize;

            if (menuProperty is not null)
                _toggleField.WithWidth(lastArrayItem || (MenuItemType)menuProperty.enumValueIndex is MenuItemType.FourAxis && arraySize > 3 ? 75f : 97f);

            if (menuProperty is not null)
            {
                this.TrackPropertyValue(menuProperty, prop => UpdateMenuType((MenuItemType)prop.enumValueIndex));
                UpdateMenuType((MenuItemType)menuProperty?.enumValueIndex);
            }

            if (valueTypeProperty is not null && colorTypeProperty is not null && objectTypeProperty is not null)
            {
                this.TrackPropertyValue(valueTypeProperty, prop => UpdateValueType());
                this.TrackPropertyValue(colorTypeProperty, prop => UpdateValueType());
                this.TrackPropertyValue(objectTypeProperty, prop => UpdateValueType());
                UpdateValueType();
            }
        }

        private void UpdateMenuType(MenuItemType? menuType)
        {
            if (menuType is null)
                return;

            // state field visibility
            _toggleStateField.Visible(menuType is MenuItemType.Button or MenuItemType.Toggle);
            _radialStateSlider.Visible(menuType is MenuItemType.Radial);
            _radialStateField.Visible(menuType is MenuItemType.Radial);
            _puppetStateField.Visible(menuType is MenuItemType.FourAxis);

            // value field length
            var maxSize = menuType switch
            {
                MenuItemType.FourAxis => 4,
                MenuItemType.Toggle => 2,
                _ => -1,
            };
            var extraWidth = _lastArrayItem && !(maxSize != -1 && _arraySize > maxSize - 1) ? 0 : 22f;
            _toggleField.WithWidth(75f + extraWidth);
            _colorField.WithWidth(75f + extraWidth);
            _integerField.WithWidth(150f + extraWidth);
            _floatField.WithWidth(150f + extraWidth);
            _objectField.WithWidth(150f + extraWidth);
            _vector2Field[0][1].WithWidth(VectorFieldWidth + extraWidth);
            _vector3Field[0][2].WithWidth(VectorFieldWidth + extraWidth);
            _vector4Field[0][3].WithWidth(VectorFieldWidth + extraWidth);

            // set when label text
            _whenLabel.text = "When " + menuType switch
            {
                MenuItemType.Button => "button",
                MenuItemType.Toggle => "toggle",
                MenuItemType.Radial => "radial",
                MenuItemType.FourAxis => "puppet",
                _ => "control",
            } + " is";
        }

        private void UpdateValueType()
        {
            var valueType = (PropertyValueType)_valueTypeProperty.enumValueIndex;
            var colorType = (PropertyColorType)_colorTypeProperty.enumValueIndex;
            var isColor = colorType is not PropertyColorType.None;

            _toggleField.Visible(false);
            _integerField.Visible(false);
            _floatField.Visible(false);
            _colorField.Visible(false);
            _vector2Field.Visible(false);
            _vector3Field.Visible(false);
            _vector4Field.Visible(false);
            _objectField.Visible(false);

            VisualElement field = valueType switch
            {
                PropertyValueType.Boolean => _toggleField,
                PropertyValueType.Integer => _integerField,
                PropertyValueType.Float => _floatField,
                PropertyValueType.Vector2 => _vector2Field,
                PropertyValueType.Vector3 => isColor ? _colorField : _vector3Field,
                PropertyValueType.Vector4 => isColor ? _colorField : _vector4Field,
                PropertyValueType.Object => _objectField,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (field is ColorField colorField)
            {
                colorField.showAlpha = valueType is PropertyValueType.Vector4;
                colorField.hdr = colorType is PropertyColorType.HDR;
                colorField.showEyeDropper = enabledSelf;
            }

            if (field is ObjectField objectField)
                objectField.objectType = Type.GetType(_objectTypeProperty.stringValue);

            // get rid of label if int or float, so you can click on the field's label and drag to increase/decrease
            if (field is IntegerField or FloatField)
                _setLabel.Visible(false);
            else
                _setLabel.Visible(true);

            field.Visible(true);
        }
    }
}
