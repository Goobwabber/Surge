using System;
using System.Linq;
using Surge.Editor.Elements;
using Surge.Editor.Extensions;
using Surge.Editor.Models;
using Surge.Editor.Services;
using Surge.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using VRC.SDKBase;
using AnimationPropertyInfo = Surge.Models.AnimationPropertyInfo;

namespace Surge.Editor.Windows
{
    internal class PropertySelectorWindow : EditorWindow
    {
        private SerializedProperty? _target;
        private BindingService? _bindingService;
        private static Rect? _lastSize;

        public static void Present(SerializedProperty property, BindingService bindingService)
        {
            var window = GetWindow<PropertySelectorWindow>();
            window.titleContent = new GUIContent("Surge Property Selector");
            window._target = property;
            window._bindingService = bindingService;

            var screenPos = GUIUtility.GUIToScreenPoint(
                Event.current != null ? Event.current.mousePosition : Vector3.zero
            );
            _lastSize ??= new Rect(screenPos - new Vector2(200, 30f), new Vector2(500, 550));
            window.position = _lastSize.Value;
        }
        
        private void CreateGUI()
        {
            VisualElement unconfiguredElement = new();
            unconfiguredElement.WithName("Unconfigured");
            unconfiguredElement.CreateLabel("Loading Properties...");
            unconfiguredElement.WithPadding(5f);
            rootVisualElement.Add(unconfiguredElement);

            VisualElement root = new();
            root.WithPadding(5f);
            root.WithName("Root");
            rootVisualElement.Add(root);
            root.style.display = DisplayStyle.None;

        }

        private void CreateSelector(VisualElement root, SerializedProperty property)
        {
            if (property.serializedObject.targetObject is not SurgeControl flareCOntrol)
                return;

            var descriptor = flareCOntrol.GetComponentInParent<VRC_AvatarDescriptor>();
            if (!descriptor)
                return;

            var groupsProperty = property.serializedObject
                .Property(nameof(SurgeControl.AnimationGroupCollection))!;

            // Writing this makes me uncomfortable... Unity Editor dev is pain.
            var propertyGroups = ((AnimationGroupCollectionInfo)groupsProperty.boxedValue).Groups;
            
            // Use the property path to find the index of the Property Group
            // In my opinion, this is pretty cursed, but it's guaranteed to work.
            var pathSpan = property.propertyPath.AsSpan();
            var startBracketIndex = pathSpan.IndexOf('[');
            var endingBracketIndex = pathSpan.IndexOf(']');
            var groupIndexString = pathSpan.Slice(startBracketIndex + 1, endingBracketIndex - startBracketIndex - 1);
            var groupIndex = int.Parse(groupIndexString);

            var propertyGroup = propertyGroups[groupIndex];

            var objects = propertyGroup.Objects.Select(o => o is Component c ? c.gameObject : o is GameObject g ? g : null).ToArray();

            // keeping this here in case we ever find a way to differentiate between context type on the UI.
            // for now, renderer type properties will automatically be merged.
            //var mergeRenderersToggle = new Toggle();
            //mergeRenderersToggle.style.marginLeft = StyleKeyword.Auto;
            //mergeRenderersToggle.label = "Combine Renderer Properties";
            //mergeRenderersToggle.WithFontSize(11f);
            //mergeRenderersToggle.WithPadding(2f);
            //mergeRenderersToggle.value = true;

            var typeFilterLabel = new Label("Target Type:").WithPadding(3f);
            typeFilterLabel.style.marginLeft = StyleKeyword.Auto;

            var typeFilter = new ComponentTypeDropdownField(null);
            typeFilter.NullSelection = true;

            var avatarGameObject = descriptor.gameObject;
            var headingGroup = root.CreateHorizontal();
            headingGroup.CreateLabel($"Current Avatar: {avatarGameObject.name}").WithPadding(3f);
            headingGroup.Add(typeFilterLabel);
            headingGroup.Add(typeFilter);
            //headingGroup.Add(mergeRenderersToggle);

            if (propertyGroup.AnimationType is AnimationGroupType.ObjectToggle)
            {
                // wtf?
                // Maybe make an error happen? but shouldn't be possible so
                return;
            }

            var binder = _bindingService;

            ToolbarSearchField searchField = new();
            root.Add(searchField);
            searchField.style.width = new StyleLength(StyleKeyword.Auto);

            var buttonGroup = root.CreateHorizontal();
            buttonGroup.CreateButton("Blendshapes", () => searchField.value = "t:Blendshape ").WithGrow(1f);
            buttonGroup.CreateButton("Materials", () => searchField.value = "t:Material ").WithGrow(1f);
            buttonGroup.CreateButton("Everything", () => 
            {
                if (searchField.value == string.Empty)
                    typeFilter.value = null;
                else
                    searchField.value = string.Empty;
            }).WithGrow(1f);
            searchField.Focus();

            // ui done, load things now
            // first: load searchable objects
            _ = binder.TryGetSearchableObjects(out GameObject?[] searchableObjects); // dont care if fails, returns empty array if so
            typeFilter.Push(searchableObjects.Select(o => (UnityEngine.Object?)o).ToArray());

            // second: load searchable properties
            var bindingSearch = binder.GetPropertyBindings().Where(b => b.GameObject != avatarGameObject);
            if (propertyGroup.AnimationType is AnimationGroupType.Avatar)
                bindingSearch = bindingSearch.GroupBy(p => p.QualifiedId).Select(g => g.First());

            var bindings = bindingSearch.ToArray();
            var items = bindings.ToList();
            
            ListView list = new(items, 46f, () => new BindablePropertyCell(), (e, i) =>
            {
                if (e is not BindablePropertyCell cell)
                    return;

                var binding = items[i];
                var isVector = binding.Type
                    is PropertyValueType.Vector2
                    or PropertyValueType.Vector3
                    or PropertyValueType.Vector4;
                
                // ReSharper disable once MoveLocalFunctionAfterJumpStatement
                void HandlePseudoProperty(int index)
                {
                    if (index > binding.Length)
                        return;

                    var pseudoProperty = binding.GetPseudoProperty(index);
                    SetProperty(pseudoProperty);
                }

                void SetProperty(SurgePseudoProperty? pseudoProperty = null)
                {
                    var propName = pseudoProperty?.Name ?? binding.Name;
                    var type = pseudoProperty is null ? binding.Type : PropertyValueType.Float;
                    var color = pseudoProperty is null ? binding.Color : PropertyColorType.None;
                    var contextType = typeFilter.value is not null && typeFilter.value.IsAssignableFrom(binding.ContextType) ? typeFilter.value : binding.ContextType;

                    property.Property(nameof(AnimationPropertyInfo.Name)).SetValue(propName);
                    property.Property(nameof(AnimationPropertyInfo.Path)).SetValue(binding.Path);
                    property.Property(nameof(AnimationPropertyInfo.ValueType)).SetValue(type);
                    property.Property(nameof(AnimationPropertyInfo.ColorType)).SetValue(color);
                    property.Property(nameof(AnimationPropertyInfo.ContextType)).SetValue(contextType.AssemblyQualifiedName!);

                    float? predictiveValue = null;
                    
                    switch (type)
                    {
                        // Seed the default value to start with
                        case PropertyValueType.Float or PropertyValueType.Boolean or PropertyValueType.Integer:
                        {
                            var defaultValue = binder.GetPropertyValue(binding);
                            if (defaultValue is bool boolValue)
                                defaultValue = boolValue ? 1f : 0f;

                            if (defaultValue is Vector4 or Vector3 or Vector2)
                                defaultValue = binder.GetPropertyValue(pseudoProperty!);
                            
                            defaultValue = (float)defaultValue;
                            defaultValue = predictiveValue ?? defaultValue;
                            property.Property(nameof(AnimationPropertyInfo.DefaultAnalog)).SetValue(defaultValue);
                            break;
                        }
                        case PropertyValueType.Vector2 or PropertyValueType.Vector3 or PropertyValueType.Vector4:
                        {
                            var defaultValue = (Vector4)binder.GetPropertyValue(binding);
                            property.Property(nameof(AnimationPropertyInfo.DefaultVector)).SetValue(defaultValue);
                            break;
                        }
                        case PropertyValueType.Object:
                        {
                            var defaultValue = (UnityEngine.Object)binder.GetPropertyValue(binding);
                            property.Property(nameof(AnimationPropertyInfo.DefaultObject)).SetValue(defaultValue);
                            var objectType = binding.GetPseudoProperty(0).Type;
                            property.Property(nameof(AnimationPropertyInfo.ObjectValueType)).SetValue(objectType.AssemblyQualifiedName);
                            break;
                        }
                    }
                    
                    Close();
                }

                var isTriOrQuadVector = binding.Type is PropertyValueType.Vector3 or PropertyValueType.Vector4;
                
                cell.SetData
                    (binding,
                    binder.GetPropertyValue(binding),
                    () => SetProperty(),
                    () => Selection.activeGameObject = binding.GameObject,
                    
                    // Context Menu Actions
                    isVector ? () => HandlePseudoProperty(0) : null,
                    isVector ? () => HandlePseudoProperty(1) : null,
                    isTriOrQuadVector ? () => HandlePseudoProperty(2) : null,
                    binding.Type is PropertyValueType.Vector4 ? () => HandlePseudoProperty(3) : null,
                    propertyGroup.AnimationType is not AnimationGroupType.Avatar
                );
            });

            void RefreshList(string search, Type? filterType, bool mergeDuplicates)
            {
                // TODO: Add optimistic search when typing (no backspace)
                items.Clear();
                if (search == string.Empty && filterType is null)
                {
                    items.AddRange(bindings);
                    list.RefreshItems();
                    return;
                }

                var materialsOnly = search.Contains("t:Material");
                var blendshapesOnly = search.Contains("t:Blendshape");

                search = search
                    .Replace("t:Material", string.Empty)
                    .Replace("t:Blendshape", string.Empty)
                    .Trim();

                var searchParts = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                using (HashSetPool<string>.Get(out var addedProps))
                {
                    foreach (var group in bindings)
                    {
                        var failedSearch = false;
                        var source = group.Source;

                        if (materialsOnly && source is not SurgePropertySource.Material)
                            continue;

                        if (blendshapesOnly && source is not SurgePropertySource.Blendshape)
                            continue;

                        foreach (var part in searchParts)
                        {
                            if (group.Id.Contains(part, StringComparison.OrdinalIgnoreCase))
                                continue;

                            failedSearch = true;
                            break;
                        }

                        if (failedSearch)
                            continue;

                        var contextType = group.ContextType;

                        if (mergeDuplicates)
                        {
                            if (addedProps.Contains(contextType.Name + group.Name))
                                continue;
                            else
                                addedProps.Add(contextType.Name + group.Name);
                        }

                        if (filterType is not null && !filterType.IsAssignableFrom(contextType))
                            continue;

                        items.Add(group);
                    }
                }

                list.RefreshItems();
                list.Query<BindablePropertyCell>().ForEach(BindablePropertyCell.FixSelector);
            }

            searchField.RegisterValueChangedCallback(ctx => RefreshList(ctx.newValue, typeFilter.value, true));
            typeFilter.RegisterValueChangedCallback(ctx => RefreshList(searchField.value, ctx.newValue, true));
            //mergeRenderersToggle.RegisterValueChangedCallback(ctx => RefreshList(searchField.value, ctx.newValue));

            VisualElement container = new();
            container.Add(list);
            root.Add(container);
            container.WithGrow(1f);
            container.style.height = new StyleLength(StyleKeyword.Null);
            
            list.selectionType = SelectionType.None;

            var previousProperty = property.Property(nameof(AnimationPropertyInfo.Name)).stringValue;
            var previousContext = property.Property(nameof(AnimationPropertyInfo.ContextType)).stringValue;
            searchField.value = string.IsNullOrWhiteSpace(previousProperty) ? "" : previousProperty;
            typeFilter.value = Type.GetType(previousContext);
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnInspectorUpdate()
        {
            if (_target == null)
                return;

            var rootElement = rootVisualElement.Q<VisualElement>("Root");
            var unconfiguredElement = rootVisualElement.Q<VisualElement>("Unconfigured");
            unconfiguredElement.style.display = DisplayStyle.None;

            if (rootElement.style.display == DisplayStyle.None)
            {
                rootElement.style.display = DisplayStyle.Flex;
                CreateSelector(rootElement, _target);
            }
            
            // Dismiss if there's no valid property.
            // TODO: Also dismiss if they click off the window
            // The goal is to mimic the AnimationCurve property drawer window thingymajig
            try
            {
                // Can't find another way to do this unfortunately, but if we try to access a property
                // when the serialized property is destroyed (array element deleted or exited/reselected component,
                // it will throw once.
                _ = _target.Property(nameof(AnimationPropertyInfo.Path));
            }
            catch
            {
                _target = null;
                Close();
            }
        }

        private void OnDisable()
        {
            _lastSize = position;
            _target = null;
        }
    }
}