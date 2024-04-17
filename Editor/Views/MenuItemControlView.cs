﻿using Flare.Editor.Attributes;
using Flare.Editor.Elements;
using Flare.Editor.Extensions;
using Flare.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flare.Editor.Views
{
    internal class MenuItemControlView : IView
    {
        [PropertyName(nameof(MenuItemInfo.Path))]
        private readonly SerializedProperty _pathProperty = null!;

        [PropertyName(nameof(MenuItemInfo.Type))]
        private readonly SerializedProperty _typeProperty = null!;

        [PropertyName(nameof(MenuItemInfo.Icon))]
        private readonly SerializedProperty _iconProperty = null!;

        [PropertyName(nameof(MenuItemInfo.IsSaved))]
        private readonly SerializedProperty _isSavedProperty = null!;

        [PropertyName(nameof(MenuItemInfo.DefaultState))]
        private readonly SerializedProperty _defaultStateProperty = null!;

        [PropertyName(nameof(MenuItemInfo.DefaultRadialValue))]
        private readonly SerializedProperty _defaultRadialProperty = null!;

        [PropertyName(nameof(MenuItemInfo.DefaultPuppetState))]
        private readonly SerializedProperty _defaultPuppetProperty = null!;

        [PropertyName(nameof(MenuItemInfo.Interpolation))]
        private readonly SerializedProperty _interpolationProperty = null!;

        [PropertyName(nameof(MenuItemInfo.ApplyToAvatar))]
        private readonly SerializedProperty _applyToAvatarProperty = null!;

        [PropertyName(nameof(MenuItemInfo.ShowIcon))]
        private readonly SerializedProperty _showIconProperty = null!;

        [PropertyName(nameof(MenuItemInfo.ShowDefault))]
        private readonly SerializedProperty _showDefaultProperty = null!;

        [PropertyName(nameof(MenuItemInfo.ShowDuration))]
        private readonly SerializedProperty _showDurationProperty = null!;

        public void Build(VisualElement root)
        {
            // Path
            var topHorizontal = root.CreateHorizontal();

            var parentFlareMenu = _pathProperty.serializedObject.targetObject is GameObject go ? go.GetComponentInParent<FlareMenu>() : null;
            var pathFieldTooltip = parentFlareMenu is not null ?
                "The name of the menu item, used for generating the parameter name and the display in the hand menu." :
                "The path of the menu item, used for generating the parameter name and the display in the hand menu. Use '/' to place something in a submenu.";
            var pathField = topHorizontal.CreatePropertyField(_pathProperty).WithGrow(1f).WithLabel(parentFlareMenu is not null ? "Menu Name" : "Menu Path").WithTooltip(pathFieldTooltip);

            foreach (var element in pathField.Children())
                Debug.Log(element.name);

            var paneMenu = new PaneMenu().WithWidth(10f).WithHeight(20f).WithMarginLeft(20f);
            paneMenu.style.marginRight = 5f;
            topHorizontal.Add(paneMenu);

            // Icon
            var iconField = root.CreatePropertyField(_iconProperty).WithLabel("Custom Icon")
                .WithTooltip("The icon used when displaying this item in the hand menu.");
            //iconField.labelElement.WithWidth(100f);

            // Default Radial Value
            var defaultRadialField = root.CreatePropertyField(_defaultRadialProperty).WithHeight(20f)
                .WithTooltip("The default position for the radial in the menu.");
            //defaultRadialField.Q<TextField>().WithWidth(130f);

            // Duration
            var durationField = root.CreatePropertyField(_interpolationProperty.Property(nameof(InterpolationInfo.Duration)))
                .WithTooltip("The duration (in seconds) this control takes to execute. A value of 0 means instant. This can also be called interpolation.");
            //durationField.Q<TextField>().WithWidth(130f);



            // Settingbar
            var settingHorizontal = root.CreateHorizontal();
            settingHorizontal.style.alignItems = Align.FlexStart;
            settingHorizontal.style.flexWrap = Wrap.Wrap;
            settingHorizontal.style.marginTop = 1f;
            settingHorizontal.style.marginBottom = 2f;

            // Settingbar Type
            var typeField = new LabelledEnumField((MenuItemType)_typeProperty.enumValueIndex, "Type: ",
                "The type of menu item this control is.", _typeProperty);
            settingHorizontal.Add(typeField);

            // Settingbar Default (Toggle)
            var defaultField = new LabelledEnumField((ToggleMenuState)_defaultStateProperty.enumValueIndex, "Default: ",
                "Is this menu item ON or OFF by default?",
                value => value == (int)ToggleMenuState.Inactive ? FlareUI.DisabledColor : FlareUI.EnabledColor,
                _defaultStateProperty);
            settingHorizontal.Add(defaultField);

            // Settingbar Default (Puppet)
            var defaultPuppetField = new LabelledEnumField((PuppetMenuState)_defaultPuppetProperty.enumValueIndex, "Default: ",
                "The default state of the control.", _defaultPuppetProperty);
            settingHorizontal.Add(defaultPuppetField);

            // Settingbar Default (Radial)
            var defaultLabel = new SettingLabelElement("Default", "This control has a custom default value.");
            settingHorizontal.Add(defaultLabel);
            // Settingbar Saved
            var savedLabel = new SettingLabelElement("Saved", "This control is saved between worlds.");
            settingHorizontal.Add(savedLabel);
            // Settingbar Icon 
            var iconLabel = new SettingLabelElement("Icon", "This control uses a custom icon.");
            settingHorizontal.Add(iconLabel);
            // Settingbar Apply To Avatar
            var applyAvatarLabel = new SettingLabelElement("Apply To Avatar", "(BETA) This control will assign its default values to the avatar on upload.", FlareUI.GetWarningImage());
            settingHorizontal.Add(applyAvatarLabel);
            // Settingbar Duration
            var interpolationLabel = new SettingLabelElement("Interpolation", "Smooths between animation states rather than being instant.");
            settingHorizontal.Add(interpolationLabel);
            // Affected by Trigger Tags
            var tagLabel = new SettingLabelElement("Tagged", "This control can be overriden by trigger tags.");
            settingHorizontal.Add(tagLabel);

            // Settings context menu
            paneMenu.SetData(SettingsMenuPopulate);
            ContextualMenuManipulator settingMenu = new(SettingsMenuPopulate);
            settingMenu.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
            settingHorizontal.AddManipulator(settingMenu);

            void SettingsMenuPopulate(ContextualMenuPopulateEvent evt)
            {
                var enabledStatus = DropdownMenuAction.Status.Checked;
                var disabledStatus = DropdownMenuAction.Status.Normal;
                var lockedStatus = DropdownMenuAction.Status.Disabled;
                var enabledLockedStatus = enabledStatus | lockedStatus;

                var buttonMode = _typeProperty.enumValueIndex == (int)MenuItemType.Button;
                var puppetMode = _typeProperty.enumValueIndex == (int)MenuItemType.FourAxis;

                evt.menu.AppendAction("Saved Between Worlds",
                    evt => UpdateValues(_isSavedProperty),
                    buttonMode ? lockedStatus : _isSavedProperty.boolValue ? enabledStatus : disabledStatus);
                evt.menu.AppendAction("Custom Default Value",
                    evt => UpdateValues(_showDefaultProperty),
                    puppetMode ? enabledLockedStatus : buttonMode ? lockedStatus : _showDefaultProperty.boolValue ? enabledStatus : disabledStatus);
                evt.menu.AppendAction("Custom Icon",
                    evt => UpdateValues(_showIconProperty),
                    _showIconProperty.boolValue ? enabledStatus : disabledStatus);
                evt.menu.AppendAction("Apply To Avatar",
                    evt => UpdateValues(_applyToAvatarProperty),
                    buttonMode ? lockedStatus : _applyToAvatarProperty.boolValue ? enabledStatus : disabledStatus);
                evt.menu.AppendAction("Property Interpolation",
                    evt => UpdateValues(_showDurationProperty),
                    _showDurationProperty.boolValue ? enabledStatus : disabledStatus);

                void UpdateValues(SerializedProperty prop)
                {
                    prop.boolValue = !prop.boolValue;
                    prop.serializedObject.ApplyModifiedProperties();
                    UpdateVisibility();
                }
            }

            void UpdateVisibility()
            {
                defaultRadialField.Visible(_showDefaultProperty.boolValue && _typeProperty.enumValueIndex == (int)MenuItemType.Radial);
                defaultField.Visible(_showDefaultProperty.boolValue && _typeProperty.enumValueIndex == (int)MenuItemType.Toggle);
                defaultPuppetField.Visible(_typeProperty.enumValueIndex == (int)MenuItemType.FourAxis);
                defaultLabel.Visible(_showDefaultProperty.boolValue && _typeProperty.enumValueIndex == (int)MenuItemType.Radial);
                savedLabel.Visible(_isSavedProperty.boolValue && _typeProperty.enumValueIndex != (int)MenuItemType.Button);
                iconField.Visible(_showIconProperty.boolValue);
                iconLabel.Visible(_showIconProperty.boolValue);
                durationField.Visible(_showDurationProperty.boolValue);
                applyAvatarLabel.Visible(_applyToAvatarProperty.boolValue);
                interpolationLabel.Visible(_showDurationProperty.boolValue);
                tagLabel.Visible(false);
            }

            UpdateVisibility();
            typeField.RegisterValueChangedCallback(_ => UpdateVisibility());
        }
    }
}