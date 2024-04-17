using System.Collections.Generic;
using System.Linq;
using Surge.Editor.Attributes;
using Surge.Editor.Elements;
using Surge.Editor.Extensions;
using Surge.Models;
using UnityEditor;
using UnityEngine.UIElements;

namespace Surge.Editor.Inspectors
{
    [CustomEditor(typeof(SurgeTags))]
    internal class SurgeTagModuleInspector : SurgeInspector
    {
        [PropertyName(nameof(SurgeTags.Rules))]
        private readonly SerializedProperty _rulesProperty = null!;
        
        [PropertyName(nameof(SurgeTags.Tags))]
        private readonly SerializedProperty _layersProperty = null!;
        
        protected override VisualElement BuildUI(VisualElement root)
        {
            HelpBox help = new("This feature is subject to breaking changes in the future.", HelpBoxMessageType.Warning);
            root.Add(help);
            
            CategoricalFoldout layersFoldout = new() { text = "Tags" };
            layersFoldout.CreatePropertyField(_layersProperty).WithName("Tags");
            root.Add(layersFoldout);

            CategoricalFoldout rulesFoldout = new() { text = "Rules" };

            var module = (target as SurgeTags)!;

            BuildRulesUI(rulesFoldout, module);
            
            var rulesArrayProperty = _rulesProperty.Field("Array")!;
            rulesFoldout.CreateButton("Add New Rule", () =>
            {
                var index = rulesArrayProperty.arraySize++;
                rulesArrayProperty.serializedObject.ApplyModifiedProperties();
                rulesArrayProperty.GetArrayElementAtIndex(index).SetValue(new SurgeRule()); // Create a new flare rule
            });
            
            root.Add(rulesFoldout);

            List<string> previousLayers = new();
            previousLayers.AddRange(module.Tags);
            root.schedule.Execute(() =>
            {
                if (!module)
                    return;
                
                if (previousLayers.SequenceEqual(module.Tags))
                    return;
                
                previousLayers.Clear();
                previousLayers.AddRange(module.Tags);
                rulesFoldout.Remove(rulesFoldout.Q("RulesList"));
                BuildRulesUI(rulesFoldout, module);

            }).Every(250);
            
            return root;
        }

        private void BuildRulesUI(VisualElement root, SurgeTags module)
        {
            var rulesArrayProperty = _rulesProperty.Field("Array")!;
            SurgeCollectionView<TagRuleElement> rules = new(() =>
            {
                TagRuleElement rule = new(module.Tags);
                rule.WithBackgroundColor(SurgeUI.BackgroundColor)
                    .WithBorderColor(SurgeUI.BorderColor)
                    .WithBorderRadius(3f)
                    .WithBorderWidth(1f)
                    .WithMarginTop(5f)
                    .WithPadding(5f);

                return rule;
            }, (e, i) =>
            {
                e.SetData(() =>
                {
                    rulesArrayProperty.DeleteArrayElementAtIndex(i);
                    rulesArrayProperty.serializedObject.ApplyModifiedProperties();
                });
            });

            rules.WithName("RulesList");
            rules.SetBinding(rulesArrayProperty);
            root.Add(rules);
            rules.SendToBack();
        }
    }
}