using Flare.Editor.Extensions;
using Flare.Editor.Models;
using Flare.Models;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flare.Editor.Elements
{
    internal class AnimationStateElement : VisualElement, IFlareBindable
    {
        private SerializedProperty? _menuProperty;
        private SerializedProperty? _valueTypeProperty;
        private SerializedProperty? _colorTypeProperty;
        private SerializedProperty? _objectTypeProperty;
        private AnimationGroupType _groupType;
        private bool _onlyArrayItem;

        private readonly Label _whenRadialLabel;
        private readonly Label _whenPuppetLabel;
        private readonly Slider _radialStateSlider;
        private readonly FloatField _radialStateField;
        private readonly LabelledEnumField _puppetStateField;
        private readonly Label _setLabel;
        private readonly LabelledEnumField _toggleField;

        private readonly Button _addButton;

        public AnimationStateElement()
        {
            this.style.flexDirection = FlexDirection.Row;

            _whenRadialLabel = this.CreateLabel("When radial is").WithHeight(20f);
            _whenRadialLabel.style.alignSelf = Align.FlexStart;
            _whenRadialLabel.style.marginTop = 1f;
            _whenRadialLabel.style.paddingTop = 2f;
            _whenRadialLabel.style.marginRight = 3f;
            _whenRadialLabel.style.marginLeft = StyleKeyword.Auto;
            _whenRadialLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            _whenPuppetLabel = this.CreateLabel("When puppet is").WithHeight(20f);
            _whenPuppetLabel.style.alignSelf = Align.FlexStart;
            _whenPuppetLabel.style.marginTop = 1f;
            _whenPuppetLabel.style.paddingTop = 2f;
            _whenPuppetLabel.style.marginRight = 5f;
            _whenPuppetLabel.style.marginLeft = StyleKeyword.Auto;
            _whenPuppetLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            _radialStateSlider = new Slider(0f, 1f).WithHeight(20f).WithGrow(1f);
            _radialStateSlider.style.marginRight = 1f;
            _radialStateField = new FloatField().WithHeight(20f).WithWidth(40f);
            _radialStateField.style.marginRight = 2f;
            this.Add(_radialStateSlider);
            this.Add(_radialStateField);

            _puppetStateField = new LabelledEnumField(PuppetMenuState.Up, "State: ",
                "The menu state that controls when the value is set.");
            _puppetStateField.WithWidth(95f);
            this.Add(_puppetStateField);

            _setLabel = this.CreateLabel("Set values to").WithHeight(20f);
            _setLabel.style.alignSelf = Align.FlexStart;
            _setLabel.style.marginTop = 1f;
            _setLabel.style.paddingTop = 2f;
            _setLabel.style.marginRight = 3f;
            _setLabel.style.marginLeft = StyleKeyword.Auto;
            _setLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            _toggleField = new LabelledEnumField(ToggleMode.Enabled, "",
                "The value to set the properties to when the menu is in this state.",
                value => value == (int)ToggleMode.Disabled ? FlareUI.DisabledColor : FlareUI.EnabledColor);
            _toggleField.WithWidth(75f);
            this.Add(_toggleField);

            _addButton = this.CreateButton("+").WithHeight(20f).WithWidth(20f).WithFontSize(18f).WithFontStyle(FontStyle.Bold);
            _addButton.style.paddingTop = 0f;
            _addButton.style.paddingBottom = 2f;
            _addButton.style.marginLeft = 1f;
            _addButton.style.marginRight = 1f;
        }

        public void SetBinding(SerializedProperty property)
        {
            var radialStateProperty = property.Property(nameof(AnimationStateInfo.RadialState));
            _radialStateSlider.BindProperty(radialStateProperty);
            _radialStateField.BindProperty(radialStateProperty);

            var puppetStateProperty = property.Property(nameof(AnimationStateInfo.PuppetState));
            _puppetStateField.BindProperty(puppetStateProperty);

            var toggleProperty = property.Property(nameof(AnimationStateInfo.Toggle));
            _toggleField.BindProperty(toggleProperty);
        }

        public void SetData(Action? onAddRequested, Action? onRemoveRequested, bool lastArrayItem, int arraySize, SerializedProperty? menuProperty, AnimationGroupType groupType)
        {
            _menuProperty = menuProperty;
            _groupType = groupType;
            _onlyArrayItem = arraySize == 1;
            _addButton.clicked -= onAddRequested;
            _addButton.clicked += onAddRequested;
            _addButton.Visible(lastArrayItem);
            if (menuProperty is not null)
                _toggleField.WithWidth(lastArrayItem || (MenuItemType)menuProperty.enumValueIndex is MenuItemType.FourAxis && arraySize > 3 ? 75f : 97f);

            void UpdateMenuType(MenuItemType? menuType)
            {
                if (menuType is null)
                    return;
                _toggleField.WithWidth(lastArrayItem && !(menuType is MenuItemType.FourAxis && arraySize > 3) ? 75f : 97f);
                _addButton.Visible(lastArrayItem && !(menuType is MenuItemType.FourAxis && arraySize > 3));
                _whenRadialLabel.Visible(menuType is MenuItemType.Radial);
                _whenPuppetLabel.Visible(menuType is MenuItemType.FourAxis);
                _radialStateSlider.Visible(menuType is MenuItemType.Radial);
                _radialStateField.Visible(menuType is MenuItemType.Radial);
                _puppetStateField.Visible(menuType is MenuItemType.FourAxis);
            }

            if (menuProperty is not null)
            {
                this.TrackPropertyValue(menuProperty, prop => UpdateMenuType((MenuItemType)prop.enumValueIndex));
                UpdateMenuType((MenuItemType)menuProperty?.enumValueIndex);
            }

            ContextualMenuManipulator rightMenu = new(RightMenuPopulate);
            rightMenu.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
            this.AddManipulator(rightMenu);

            void RightMenuPopulate(ContextualMenuPopulateEvent evt)
            {
                if (!_onlyArrayItem)
                    evt.menu.AppendAction("Remove", evt => onRemoveRequested?.Invoke());
            }
        }
    }
}
