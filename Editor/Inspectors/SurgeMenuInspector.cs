﻿using Surge.Editor.Elements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Surge.Editor.Inspectors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SurgeMenu))]
    internal class SurgeMenuInspector : UnityEditor.Editor
    {
        private SerializedProperty? _menuNameProperty;
        private SerializedProperty? _menuIconProperty;
        private SerializedProperty? _synchronizationProperty;
        
        private void OnEnable()
        {
            _menuNameProperty = serializedObject.Property(nameof(SurgeMenu.Name));
            _menuIconProperty = serializedObject.Property(nameof(SurgeMenu.Icon));
            _synchronizationProperty = serializedObject.Property(nameof(SurgeMenu.Synchronize));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var component = target as SurgeMenu;
            
            VisualElement root = new();
            
            // Setup menu name property.
            PropertyField menuNameField = new(_menuNameProperty);
            root.Add(menuNameField);
            
            // Update the name of the component AND the GameObject if it's changed.
            menuNameField.RegisterValueChangeCallback(ctx =>
            {
                if (component.AsNullable() is null || component!.Synchronize is false)
                    return;

                component.SetName(ctx.changedProperty.stringValue);
            });
            
            // Setup menu icon property.
            root.Add(new PropertyField(_menuIconProperty));

            root.Add(new HorizontalSpacer());
            
            // Setup Advanced Settings
            Foldout advancedSettingsFoldout = new()
            {
                text = "Advanced Settings",
                value = EditorPrefs.GetBool("nexus.auros.flare.advancedSettingsFoldout", false)
            };
            advancedSettingsFoldout.RegisterValueChangedCallback(_ =>
            {
                EditorPrefs.SetBool("nexus.auros.flare.advancedSettingsFoldout", advancedSettingsFoldout.value);
            });
            
            PropertyField synchroPropertyField = new(_synchronizationProperty);
            advancedSettingsFoldout.Add(synchroPropertyField);
            
            root.Add(advancedSettingsFoldout);
            
            return root;
        }
    }
}