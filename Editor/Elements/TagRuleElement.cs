using System;
using System.Collections.Generic;
using System.Linq;
using Surge.Editor.Extensions;
using Surge.Editor.Models;
using Surge.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Surge.Editor.Elements
{
    internal class TagRuleElement : VisualElement, ISurgeBindable
    {
        private readonly PaneMenu _paneMenu;
        private readonly EnumField _causeStateField;
        private readonly EnumField _effectStateField;
        private readonly DropdownField _causeTagDropdown;
        private readonly DropdownField _effectTagDropdown;
        
        public TagRuleElement(IEnumerable<string> tags)
        {
            _paneMenu = new PaneMenu();
            
            _paneMenu.WithWidth(10f).WithHeight(20f);
            _paneMenu.style.marginLeft = 5f;
            
            var topContainer = this.CreateHorizontal();
            topContainer.CreateLabel("Rule").WithFontSize(14f).WithFontStyle(FontStyle.Bold).WithGrow(1f);
            topContainer.Add(_paneMenu);

            Add(topContainer);
            
            var root = this.CreateHorizontal()
                .WithWrap(Wrap.Wrap)
                .WithMaxHeight(192f)
                .WithMinHeight(24f)
                .WithShrink(24f);
            
            // Add text: "When a control with a tag"
            root.CreateLabel("When a control with a tag")
                .WithTextAlign(TextAnchor.MiddleCenter)
                .WithHeight(20f);
            
            // [TAG]
            _causeTagDropdown = new DropdownField
            {
                // ReSharper disable once PossibleMultipleEnumeration
                choices = tags.ToList()
            }.WithHeight(20f);
            root.Add(_causeTagDropdown);

            // Add text: "becomes"
            root.CreateLabel("  becomes")
                .WithTextAlign(TextAnchor.MiddleCenter)
                .WithHeight(20f);
            
            // [ENABLED / DISABLED]
            _causeStateField = new EnumField(ToggleMode.Disabled).WithHeight(20f);
            root.Add(_causeStateField);
            
            root.CreateLabel(" ,")
                .WithTextAlign(TextAnchor.MiddleCenter)
                .WithHeight(20f);
            
            // Add text: "all the controls with a tag"
            root.CreateLabel("all the controls with a tag")
                .WithTextAlign(TextAnchor.MiddleCenter)
                .WithHeight(20f);
            
            // [LAYER]
            _effectTagDropdown = new DropdownField
            {
                // ReSharper disable once PossibleMultipleEnumeration (we want multiple copies)
                choices = tags.ToList()
            }.WithHeight(20f);
            root.Add(_effectTagDropdown);
            
            // Add text: "  should be set to"
            root.CreateLabel("  should be set to")
                .WithTextAlign(TextAnchor.MiddleCenter)
                .WithHeight(20f);
            
            // [ENABLED / DISABLED]
            _effectStateField = new EnumField(ToggleMode.Disabled).WithHeight(20f);
            root.Add(_effectStateField);
            
            _causeStateField.RegisterValueChangedCallback(OnCauseStateFieldChange);
            _effectStateField.RegisterValueChangedCallback(OnCauseStateFieldChanged);
        }

        public void SetBinding(SerializedProperty property)
        {
            _causeStateField.Unbind();
            _effectStateField.Unbind();
            _causeTagDropdown.Unbind();
            _effectTagDropdown.Unbind();
            
            var causeStateProperty = property.Property(nameof(SurgeRule.CauseState));
            var effectStateProperty = property.Property(nameof(SurgeRule.EffectState));

            _causeStateField.BindProperty(causeStateProperty);
            _effectStateField.BindProperty(effectStateProperty);
            _causeTagDropdown.BindProperty(property.Property(nameof(SurgeRule.CauseLayer)));
            _effectTagDropdown.BindProperty(property.Property(nameof(SurgeRule.EffectLayer)));
            
            OnModeFieldChanged(_causeStateField, (ToggleMode)causeStateProperty.enumValueIndex);
            OnModeFieldChanged(_effectStateField, (ToggleMode)effectStateProperty.enumValueIndex);
        }

        public void SetData(Action? onRemoveRequested)
        {
            _paneMenu.SetData(evt => evt.menu.AppendAction("Remove", _ => onRemoveRequested?.Invoke()));
        }
        
        private void OnCauseStateFieldChange(ChangeEvent<Enum> evt) => OnModeFieldChanged(_causeStateField, evt);

        private void OnCauseStateFieldChanged(ChangeEvent<Enum> evt) => OnModeFieldChanged(_effectStateField, evt);

        private static void OnModeFieldChanged(EnumField field, ChangeEvent<Enum> evt)
        {
            OnModeFieldChanged(field, (ToggleMode)(evt.newValue ?? evt.previousValue));
        }
        
        // ReSharper disable once SuggestBaseTypeForParameter
        private static void OnModeFieldChanged(EnumField field, ToggleMode value)
        {
            // Color the enum dropdown text according to the value of the toggle mode.
            var enabledColor = SurgeUI.EnabledColor;
            var disabledColor = SurgeUI.DisabledColor;
            
            var targetColor = value is ToggleMode.Enabled ? enabledColor : disabledColor;
            
            field.Q<TextElement>().WithColor(targetColor);
        }

    }
}