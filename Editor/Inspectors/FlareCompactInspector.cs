using System;
using System.Runtime.CompilerServices;
using System.Text;
using Flare.Editor.Attributes;
using Flare.Editor.Editor.Extensions;
using Flare.Editor.Elements;
using Flare.Editor.Extensions;
using Flare.Editor.Views;
using Flare.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Flare.Editor.Inspectors
{
    [CustomEditor(typeof(FlareCompact))]
    internal class FlareCompactInspector : FlareInspector
    {
        [PropertyName(nameof(FlareCompact.Path))]
        private readonly SerializedProperty _pathProperty = null!;

        [PropertyName(nameof(FlareCompact.Type))]
        private readonly SerializedProperty _typeProperty = null!;

        [PropertyName(nameof(FlareCompact.Icon))]
        private readonly SerializedProperty _iconProperty = null!;

        [PropertyName(nameof(FlareCompact.IsSaved))]
        private readonly SerializedProperty _isSavedProperty = null!;

        [PropertyName(nameof(FlareCompact.DefaultState))]
        private readonly SerializedProperty _defaultStateProperty = null!;

        [PropertyName(nameof(FlareCompact.DefaultRadialValue))]
        private readonly SerializedProperty _defaultRadialProperty = null!;

        [PropertyName(nameof(FlareCompact.DefaultPuppetState))]
        private readonly SerializedProperty _defaultPuppetProperty = null!;

        [PropertyName(nameof(FlareCompact.Interpolation))]
        private readonly SerializedProperty _interpolationProperty = null!;

        [PropertyName(nameof(FlareCompact.ApplyToAvatar))]
        private readonly SerializedProperty _applyToAvatarProperty = null!;

        [PropertyName(nameof(FlareCompact.ShowIcon))]
        private readonly SerializedProperty _showIconProperty = null!;

        [PropertyName(nameof(FlareCompact.ShowDefault))]
        private readonly SerializedProperty _showDefaultProperty = null!;

        [PropertyName(nameof(FlareCompact.ShowDuration))]
        private readonly SerializedProperty _showDurationProperty = null!;

        [PropertyName(nameof(FlareCompact.TagInfo))]
        private readonly TagInfoView _tagInfoView = new();

        protected override void OnInitialization()
        {
            if (target is not FlareCompact control)
                return;
        }

        protected override VisualElement BuildUI(VisualElement root)
        {
            HelpBox misconfigurationErrorBox = new();
            root.Add(misconfigurationErrorBox);
            misconfigurationErrorBox.Visible(false);

            // Menu foldout
            CategoricalFoldout menuItemFoldout = new() { text = "Control (Menu)" };
            root.Add(menuItemFoldout);

            // Path
            var topHorizontal = menuItemFoldout.CreateHorizontal();

            var pathField = topHorizontal.CreatePropertyField(_pathProperty)
                .WithTooltip("The path of the menu item, used for generating the parameter name and the display in the hand menu. Use '/' to place something in a submenu.")
                .WithGrow(1f);

            var paneMenu = new PaneMenu().WithWidth(10f).WithHeight(20f).WithMarginLeft(20f);
            paneMenu.style.marginRight = 5f;
            topHorizontal.Add(paneMenu);

            // Icon
            var iconField = menuItemFoldout.CreatePropertyField(_iconProperty)
                .WithTooltip("The icon used when displaying this item in the hand menu.");

            // Default Radial Value
            var defaultRadialField = menuItemFoldout.CreatePropertyField(_defaultRadialProperty)
                .WithTooltip("The default position for the radial in the menu.");

            // Duration
            var durationField = menuItemFoldout.CreatePropertyField(_interpolationProperty.Property(nameof(InterpolationInfo.Duration)))
                .WithTooltip("The duration (in seconds) this control takes to execute. A value of 0 means instant. This can also be called interpolation.");



            // Settingbar
            var settingHorizontal = menuItemFoldout.CreateHorizontal();
            settingHorizontal.style.alignItems = Align.FlexStart;
            settingHorizontal.style.flexWrap = Wrap.Wrap;
            settingHorizontal.style.marginTop = 1f;
            settingHorizontal.style.marginBottom = 2f;

            // Settingbar Type
            var typeField = new LabelledEnumField((MenuItemType)_typeProperty.enumValueIndex, _typeProperty, "Type: ",
                "The type of menu item this control is.");
            settingHorizontal.Add(typeField);

            // Settingbar Default (Toggle)
            var defaultField = new LabelledEnumField((ToggleMenuState)_defaultStateProperty.enumValueIndex, _defaultStateProperty, "Default: ",
                "Is this menu item ON or OFF by default?",
                value => value == (int)ToggleMenuState.Inactive ? FlareUI.DisabledColor : FlareUI.EnabledColor);
            settingHorizontal.Add(defaultField);

            // Settingbar Default (Puppet)
            var defaultPuppetField = new LabelledEnumField((PuppetMenuState)_defaultPuppetProperty.enumValueIndex, _defaultPuppetProperty, "Default: ",
                "The default state of the control.");
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
            var interpolationLabel = new SettingLabelElement("Interpolation", "Allows smoothing between the beginning of the animation and the end, rather than being instant.");
            settingHorizontal.Add(interpolationLabel);



            // Tags Foldout
            CategoricalFoldout tagFoldout = new() { text = "Tags (Experimental)", value = false };
            _tagInfoView.Build(tagFoldout);
            root.Add(tagFoldout);



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

                var buttonMode = _typeProperty.enumValueIndex == (int)MenuItemType.Button;

                evt.menu.AppendAction("Saved", 
                    evt => UpdateValues(_isSavedProperty), 
                    buttonMode ? lockedStatus : _isSavedProperty.boolValue ? enabledStatus : disabledStatus);
                evt.menu.AppendAction("Default",
                    evt => UpdateValues(_showDefaultProperty),
                    buttonMode ? lockedStatus : _showDefaultProperty.boolValue ? enabledStatus : disabledStatus);
                evt.menu.AppendAction("Icon",
                    evt => UpdateValues(_showIconProperty),
                    _showIconProperty.boolValue ? enabledStatus : disabledStatus);
                evt.menu.AppendAction("Apply To Avatar",
                    evt => UpdateValues(_applyToAvatarProperty),
                    buttonMode ? lockedStatus : _applyToAvatarProperty.boolValue ? enabledStatus : disabledStatus);
                evt.menu.AppendAction("Interpolation",
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
                defaultPuppetField.Visible(_showDefaultProperty.boolValue && _typeProperty.enumValueIndex == (int)MenuItemType.FourAxis);
                defaultLabel.Visible(_showDefaultProperty.boolValue && _typeProperty.enumValueIndex == (int)MenuItemType.Radial);
                savedLabel.Visible(_isSavedProperty.boolValue && _typeProperty.enumValueIndex != (int)MenuItemType.Button);
                iconField.Visible(_showIconProperty.boolValue);
                iconLabel.Visible(_showIconProperty.boolValue);
                durationField.Visible(_showDurationProperty.boolValue);
                applyAvatarLabel.Visible(_applyToAvatarProperty.boolValue);
                interpolationLabel.Visible(_showDurationProperty.boolValue);
            }

            UpdateVisibility();
            typeField.RegisterValueChangedCallback(_ => UpdateVisibility());

            // Display warning for if any object references are not on this avatar.
            root.schedule.Execute(() =>
            {
                if (target is not FlareCompact control)
                    return;

                var notOnAvatar = ListPool<Object?>.Get();
                control.GetReferencesNotOnAvatar(notOnAvatar);

                var errors = notOnAvatar.Count is not 0;
                misconfigurationErrorBox.Visible(errors);
                if (errors)
                {
                    StringBuilder sb = new();
                    sb.AppendLine("Some references are not located under the current avatar.");

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < notOnAvatar.Count; i++)
                    {
                        var invalidObject = notOnAvatar[i];
                        if (invalidObject != null)
                            sb.AppendLine(invalidObject.ToString());
                    }

                    misconfigurationErrorBox.messageType = HelpBoxMessageType.Error;
                    misconfigurationErrorBox.text = sb.ToString();
                }

                ListPool<Object?>.Release(notOnAvatar);

            }).Every(20);

            return root;
        }
    }
}