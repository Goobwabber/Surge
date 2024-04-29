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

            var mainWindow = EditorGUIUtility.GetMainWindowPosition();
            var screenPos = mainWindow.position + new Vector2(mainWindow.width, mainWindow.height) / 2;
            var windowSize = new Vector2(500, 550);
            _lastSize ??= new Rect(screenPos - windowSize / 2, windowSize);
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
            var animationGroups = ((AnimationGroupCollectionInfo)groupsProperty.boxedValue).Groups;
            
            // Use the property path to find the index of the Property Group
            // In my opinion, this is pretty cursed, but it's guaranteed to work.
            var pathSpan = property.propertyPath.AsSpan();
            var startBracketIndex = pathSpan.IndexOf('[');
            var endingBracketIndex = pathSpan.IndexOf(']');
            var groupIndexString = pathSpan.Slice(startBracketIndex + 1, endingBracketIndex - startBracketIndex - 1);
            var groupIndex = int.Parse(groupIndexString);

            var animationGroup = animationGroups[groupIndex];
            var groupsArrayProperty = groupsProperty.Property(nameof(AnimationGroupCollectionInfo.Groups)).Field("Array");
            var groupProperty = groupsArrayProperty.GetArrayElementAtIndex(groupIndex);

            var objects = animationGroup.Objects.Select(o => o is Component c ? c.gameObject : o is GameObject g ? g : null).ToArray();

            var typeFilterLabel = new Label("Target Type:").WithPadding(3f);
            typeFilterLabel.style.marginLeft = StyleKeyword.Auto;

            var typeFilter = new ComponentTypeDropdownField(null);
            typeFilter.NullSelection = true;

            var avatarGameObject = descriptor.gameObject;
            var headingGroup = root.CreateHorizontal();
            headingGroup.CreateLabel($"Current Avatar: {avatarGameObject.name}").WithPadding(3f);
            headingGroup.Add(typeFilterLabel);
            headingGroup.Add(typeFilter);

            if (animationGroup.GroupType is AnimationGroupType.ObjectToggle)
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
            buttonGroup.CreateButton("Blendshapes", () => SetSearchType("Blendshape")).WithGrow(1f);
            buttonGroup.CreateButton("Materials", () => SetSearchType("Material")).WithGrow(1f);
            buttonGroup.CreateButton("Everything", () => SetSearchType(string.Empty)).WithGrow(1f);
            void SetSearchType(string type)
            {
                var text = searchField.value;
                var tIndex = text.IndexOf("t:");
                if (tIndex != -1)
                {
                    var tEnd = text.IndexOf(' ', tIndex);
                    var oldType = text.Substring(tIndex, tEnd - tIndex + 1);
                    var typeStr = string.IsNullOrEmpty(type) ? string.Empty : "t:" + type + ' ';
                    text = text.Replace(oldType, typeStr);
                    FocusSearch(text);
                    return;
                }
                // if no type string found and 'everything' clicked
                if (type == string.Empty)
                {
                    // if type filter not cleared, clear it 
                    if (typeFilter.value != null)
                        typeFilter.value = null;
                    else // reset search if type filter already cleared
                        FocusSearch(string.Empty);
                    return;
                }
                // if no type string found, insert it at beginning
                text = text.Insert(0, "t:" + type + ' ');
                FocusSearch(text);
            }

            // ui done, load things now
            // first: load searchable objects
            _ = binder.TryGetSearchableObjects(out GameObject?[] searchableObjects); // dont care if fails, returns empty array if so
            typeFilter.Push(searchableObjects.Select(o => (UnityEngine.Object?)o).ToArray());

            // second: load searchable properties
            var bindingSearch = binder.GetPropertyBindings().Where(b => b.GameObject != avatarGameObject);
            if (animationGroup.GroupType is AnimationGroupType.Avatar)
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
                    SetProperty(false, pseudoProperty);
                }

                void SetProperty(bool keepOpen, SurgePseudoProperty? pseudoProperty = null)
                {
                    var propName = pseudoProperty?.Name ?? binding.Name;
                    var type = pseudoProperty is null ? binding.Type : PropertyValueType.Float;
                    var color = pseudoProperty is null ? binding.Color : PropertyColorType.None;
                    var contextType = typeFilter.value is not null && typeFilter.value.IsAssignableFrom(binding.ContextType) ? typeFilter.value : binding.ContextType;

                    // record so we can undo later
                    Undo.RecordObject(property.serializedObject.targetObject, $"Set property data");

                    property.Property(nameof(AnimationPropertyInfo.Name)).SetValueNoRecord(propName);
                    property.Property(nameof(AnimationPropertyInfo.Path)).SetValueNoRecord(binding.Path);
                    property.Property(nameof(AnimationPropertyInfo.ValueType)).SetValueNoRecord(type);
                    property.Property(nameof(AnimationPropertyInfo.ColorType)).SetValueNoRecord(color);
                    property.Property(nameof(AnimationPropertyInfo.ContextType)).SetValueNoRecord(contextType.AssemblyQualifiedName!);

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
                            property.Property(nameof(AnimationPropertyInfo.DefaultAnalog)).SetValueNoRecord(defaultValue);
                            break;
                        }
                        case PropertyValueType.Vector2 or PropertyValueType.Vector3 or PropertyValueType.Vector4:
                        {
                            var defaultValue = (Vector4)binder.GetPropertyValue(binding);
                            property.Property(nameof(AnimationPropertyInfo.DefaultVector)).SetValueNoRecord(defaultValue);
                            break;
                        }
                        case PropertyValueType.Object:
                        {
                            var defaultValue = (UnityEngine.Object)binder.GetPropertyValue(binding);
                            property.Property(nameof(AnimationPropertyInfo.DefaultObject)).SetValueNoRecord(defaultValue);
                            var objectType = binding.GetPseudoProperty(0).Type;
                            property.Property(nameof(AnimationPropertyInfo.ObjectValueType)).SetValueNoRecord(objectType.AssemblyQualifiedName);
                            break;
                        }
                    }

                    // apply the stuffs 
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    property.serializedObject.ApplyModifiedProperties();
                    Debug.Log(property.Property(nameof(AnimationPropertyInfo.Name)).GetValue());

                    // check if we are keeping window open, and if so schedule switching the property
                    if (keepOpen)
                        root.schedule.Execute(() =>
                        {
                            var propertiesArray = groupProperty.Property(nameof(AnimationGroupInfo.Properties)).Field("Array");
                            bool pastCurrentProp = false;
                            SerializedProperty? newProperty = null;
                            for (int i = 0; i < propertiesArray.arraySize; i++)
                            {
                                var propAtIndex = propertiesArray.GetArrayElementAtIndex(i);
                                // find current property
                                if (!pastCurrentProp)
                                {
                                    if (SerializedProperty.EqualContents(propAtIndex, property))
                                        continue;
                                    pastCurrentProp = true;
                                    continue;
                                }
                                // find next empty property
                                var propAtIndexName = propAtIndex.Property(nameof(AnimationPropertyInfo.Name)).stringValue;
                                if (propAtIndexName != string.Empty)
                                    continue;
                                newProperty = propAtIndex;
                                break;
                            }

                            // no suitable property found, create new.
                            if (newProperty is null)
                            {
                                var newPropertyIndex = propertiesArray.arraySize++;
                                newProperty = propertiesArray.GetArrayElementAtIndex(newPropertyIndex);
                                propertiesArray.serializedObject.ApplyModifiedProperties();
                                newProperty.SetValue(new AnimationPropertyInfo());
                            }

                            property = newProperty.Copy();
                            Debug.Log(property.propertyPath + " | " + property.name);
                        }).StartingIn(100); // fuckit, random 100ms delay, i think it's possible for the values to have not updated in time otherwise??? (someone please help me)


                    if (!keepOpen)
                        Close();
                }

                var isTriOrQuadVector = binding.Type is PropertyValueType.Vector3 or PropertyValueType.Vector4;
                
                cell.SetData
                    (binding,
                    binder.GetPropertyValue(binding),
                    shiftClick => SetProperty(shiftClick),
                    () => Selection.activeGameObject = binding.GameObject,
                    
                    // Context Menu Actions
                    isVector ? () => HandlePseudoProperty(0) : null,
                    isVector ? () => HandlePseudoProperty(1) : null,
                    isTriOrQuadVector ? () => HandlePseudoProperty(2) : null,
                    binding.Type is PropertyValueType.Vector4 ? () => HandlePseudoProperty(3) : null,
                    animationGroup.GroupType is not AnimationGroupType.Avatar
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
                var valueTypeString = search.Split(' ').FirstOrDefault(s => s.Length > 1 && string.CompareOrdinal(s[..2], "v:") == 0);
                var valueTypeFilter = PropertyValueType.Boolean;
                var colorTypeFilter = PropertyColorType.None;
                var valueTypeOnly = !string.IsNullOrEmpty(valueTypeString) && TryGetPropertyType(valueTypeString[2..], out valueTypeFilter, out colorTypeFilter);
                if (valueTypeOnly)
                    search = search.Replace(valueTypeString, string.Empty);

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

                        if (valueTypeOnly && valueTypeFilter != group.Type || colorTypeFilter != group.Color)
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

            VisualElement container = new();
            container.Add(list);
            root.Add(container);
            container.WithGrow(1f);
            container.style.height = new StyleLength(StyleKeyword.Null);
            
            list.selectionType = SelectionType.None;

            var previousContext = property.Property(nameof(AnimationPropertyInfo.ContextType)).stringValue;

            // If this is a group type that uses shared animation states, pull shared data to filter the search with
            var isSharedValueType = animationGroup.GroupType is AnimationGroupType.ObjectToggle or AnimationGroupType.Normal or AnimationGroupType.Avatar;
            var previousProperty = isSharedValueType ? animationGroup.Properties.FirstOrDefault()?.Name : property.Property(nameof(AnimationPropertyInfo.Name)).stringValue;
            var previousValueType = isSharedValueType ? animationGroup.SharedValueType : (PropertyValueType)property.Property(nameof(AnimationPropertyInfo.ValueType)).enumValueIndex;
            var previousColorType = isSharedValueType ? animationGroup.SharedColorType : (PropertyColorType)property.Property(nameof(AnimationPropertyInfo.ColorType)).enumValueIndex;

            typeFilter.value = Type.GetType(previousContext);

            // check to see if binding exists or if we are smoking crack before we insert value type into searchbar
            if (!string.IsNullOrEmpty(previousProperty))
            {
                foreach (var binding in bindings)
                {
                    if (string.CompareOrdinal(binding.Name, previousProperty) != 0)
                        continue;
                    // TODO: this should probably allow object type searches but goobie is lazy atm
                    searchField.value = $"v:{GetPropertyTypeName(previousValueType, previousColorType)} ";
                    break;
                }
            }

            for(int i = 0; i < items.Count; i++)
            {
                if (string.CompareOrdinal(items[i].Name, previousProperty) != 0)
                    continue;
                list.ScrollToItem(i);
                break;
            }

            // focus searchfield and then put text cursor at end of line
            FocusSearch();
            void FocusSearch(string? value = null)
            {
                if (value is not null)
                    searchField.value = value;

                searchField.Focus();
                root.schedule.Execute(() =>
                {
                    using (var e = KeyDownEvent.GetPooled((char)0, KeyCode.End, EventModifiers.FunctionKey))
                        searchField.SendEvent(e);
                }).StartingIn(5); // 5 ms after, otherwise this sometimes fails ig (maybe needs to be higher for slower pcs)
            }
        }

        // note this is different from the one in SurgeUI for now
        private static string GetPropertyTypeName(PropertyValueType valueType, PropertyColorType colorType)
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
                    PropertyColorType.RGB => valueType is PropertyValueType.Vector3 ? "RGB" : "RGBA",
                    PropertyColorType.HDR => valueType is PropertyValueType.Vector3 ? "HDR" : "HDRA",
                    _ => "<null>",
                },
                PropertyValueType.Object => "Object",
                _ => "<null>",
            };
        }

        private static bool TryGetPropertyType(string name, out PropertyValueType valueType, out PropertyColorType colorType)
        {
            var lowercase = name.ToLower();

            valueType = lowercase switch
            {
                "bool" => PropertyValueType.Boolean,
                "int" => PropertyValueType.Integer,
                "float" => PropertyValueType.Float,
                "vector2" => PropertyValueType.Vector2,
                "vector3" or "rgb" or "hdr" => PropertyValueType.Vector3,
                "vector4" or "rgba" or "hdra" => PropertyValueType.Vector4,
                "object" => PropertyValueType.Object,
                _ => (PropertyValueType)(-1),
            };

            colorType = lowercase switch
            {
                "rgb" or "rgba" => PropertyColorType.RGB,
                "hdr" or "hdra" => PropertyColorType.HDR,
                _ => PropertyColorType.None,
            };

            return (int)valueType != -1;
        }

        private void OnLostFocus()
        {
            //Close();
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