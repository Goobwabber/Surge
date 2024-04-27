using Surge.Editor.Extensions;
using Surge.Editor.Models;
using Surge.Editor.Services;
using Surge.Models;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using AnimationPropertyInfo = Surge.Models.AnimationPropertyInfo;

namespace Surge.Editor.Elements
{
    internal class AnimationGroupElement : VisualElement, ISurgeBindable
    {
        // constants
        private const float LabelWidth = 105f;
        public readonly Type[] AvatarSearchTypes = { typeof(Renderer), typeof(Behaviour) };

        // properties
        private SerializedProperty? _groupProperty;
        private SerializedProperty? _toggleTypeProperty;
        private SerializedProperty? _objectsProperty;
        private SerializedProperty? _propertiesProperty;
        private SerializedProperty? _sharedStatesProperty;
        private SerializedProperty? _sharedValueTypeProperty;
        private SerializedProperty? _sharedColorTypeProperty;
        private SerializedProperty? _sharedObjectTypeProperty;
        private SerializedProperty? _menuTypeProperty;

        // vars
        private AnimationGroupType _groupType;
        private BindingService _bindingService;

        // objects section
        private readonly VisualElement _objectsContainer;
        private readonly Label _objectsLabel;
        private readonly Label _exclusionsLabel;
        private readonly LabelledEnumField _toggleField;
        private readonly VisualElement _objectsSeparator;
        private readonly SurgeCompactCollectionView<AnimationObjectElement> _objectsField;
        private readonly PaneMenu _paneMenu;

        // properties section
        private readonly VisualElement _propertiesContainer;
        private readonly Label _propertiesLabel;
        private readonly VisualElement _propertiesSeparator;
        private readonly SurgeCompactCollectionView<AnimationPropertyElement> _propertiesField;

        // states section
        private readonly VisualElement _compactVectorKey;
        private readonly SurgeCompactCollectionView<AnimationStateElement> _statesField;

        public AnimationGroupElement()
        {
            // surrounding container
            _paneMenu = new PaneMenu().WithWidth(10f).WithHeight(20f).WithShrink(0f);
            _paneMenu.style.marginLeft = 5f;
            var container = new VisualElement().WithGrow(1f);
            container.style.flexDirection = FlexDirection.Column;
            this.style.flexDirection = FlexDirection.Row;
            this.Add(container);
            this.Add(_paneMenu);


            _objectsField = new(CreateObjectElement, (e, i) => e.SetData(_groupType));
            _objectsField.WithGrow(1f).SetRemoveName("Object");

            _objectsSeparator = new VisualElement().WithMargin(0f).WithPadding(0f).WithBorderWidth(0f);
            _objectsSeparator.WithBorderColor(SurgeUI.ButtonBorderColor);
            _objectsSeparator.style.borderLeftWidth = 2f;
            _objectsSeparator.style.marginLeft = 2f;

            _objectsLabel = new Label("Target Objects");
            _objectsLabel.style.alignSelf = Align.FlexStart;
            _objectsLabel.style.marginTop = 1f;
            _objectsLabel.style.paddingTop = 2f;
            _objectsLabel.style.width = LabelWidth;
            _objectsLabel.tooltip = "The objects to be animated.";

            _exclusionsLabel = new Label("Excluded Objects");
            _exclusionsLabel.style.alignSelf = Align.FlexStart;
            _exclusionsLabel.style.marginTop = 1f;
            _exclusionsLabel.style.paddingTop = 2f;
            _exclusionsLabel.style.width = LabelWidth;
            _exclusionsLabel.tooltip = "The objects to be excluded from this avatar-wide animation.";

            _toggleField = new LabelledEnumField(AnimationToggleType.TurnOn, "When Active: ",
                "Whether to enable these objects or disable them when the menu item is active.",
                value => value == (int)AnimationToggleType.TurnOff ? SurgeUI.DisabledColor : SurgeUI.EnabledColor);
            _toggleField.style.width = 160f;

            // Create the container for the object picker
            _objectsContainer = container.CreateHorizontal();
            _objectsContainer.Add(_objectsLabel);
            _objectsContainer.Add(_exclusionsLabel);
            _objectsContainer.Add(_toggleField);
            _objectsContainer.Add(_objectsSeparator);
            _objectsContainer.Add(_objectsField);



            // Animation Properties
            _propertiesField = new(CreatePropertyElement, (e, i) => 
                e.SetData(i == 0, _objectsProperty, _groupProperty, _bindingService, _groupType));
            _propertiesField.WithGrow(1f).SetRemoveName("Property");

            _propertiesLabel = new Label("Target Properties");
            _propertiesLabel.style.alignSelf = Align.FlexStart;
            _propertiesLabel.style.marginTop = 1f;
            _propertiesLabel.style.paddingTop = 2f;
            _propertiesLabel.style.width = LabelWidth;
            _propertiesLabel.tooltip = "The properties to be animated.";

            _propertiesSeparator = new VisualElement().WithMargin(0f).WithPadding(0f).WithBorderWidth(0f);
            _propertiesSeparator.WithBorderColor(SurgeUI.ButtonBorderColor);
            _propertiesSeparator.style.borderLeftWidth = 2f;
            _propertiesSeparator.style.marginLeft = 2f;

            // Create the container for animation properties
            _propertiesContainer = container.CreateHorizontal();
            _propertiesContainer.Add(_propertiesLabel);
            _propertiesContainer.Add(_propertiesSeparator);
            _propertiesContainer.Add(_propertiesField);



            // Animation States
            // Animation States: vector field labels
            _compactVectorKey = container.CreateHorizontal().WithHeight(12f);
            _compactVectorKey.style.unityTextAlign = TextAnchor.MiddleLeft;
            _compactVectorKey.style.paddingBottom = 1f;
            // take some off width so that the letters are slightly to the right
            // x has a different value bc if it doesnt then it doesnt line up???? (and is still about half a pixel off!!)
            _compactVectorKey.CreateLabel("x").WithWidth(AnimationStateElement.VectorFieldWidth - 1f).WithMarginLeft(StyleKeyword.Auto);
            _compactVectorKey.CreateLabel("y").WithWidth(AnimationStateElement.VectorFieldWidth - 2f);
            _compactVectorKey.CreateLabel("z").WithWidth(AnimationStateElement.VectorFieldWidth - 2f);
            // add button width here to move all the letters to the left
            _compactVectorKey.CreateLabel("w").WithWidth(AnimationStateElement.VectorFieldWidth + 22f - 2f);

            // Animation States: list
            _statesField = new(CreateStateElement, (e, i) => 
                e.SetData(_sharedStatesProperty?.arraySize - 1 == i, _sharedStatesProperty?.arraySize ?? 0, _menuTypeProperty, 
                    _sharedValueTypeProperty, _sharedColorTypeProperty, _sharedObjectTypeProperty, _groupType));
            _statesField.WithGrow(1f).SetRemoveName("State");
            container.Add(_statesField);
        }

        public void SetBinding(SerializedProperty property)
        {
            // properties
            _groupProperty = property.Copy();
            _groupType = (AnimationGroupType)property.Property(nameof(AnimationGroupInfo.GroupType)).enumValueIndex;
            _toggleTypeProperty = property.Property(nameof(AnimationGroupInfo.ToggleType));
            _objectsProperty = property.Property(nameof(AnimationGroupInfo.Objects)).Field("Array");
            _propertiesProperty = property.Property(nameof(AnimationGroupInfo.Properties)).Field("Array");
            _sharedStatesProperty = property.Property(nameof(AnimationGroupInfo.SharedAnimationStates)).Field("Array");
            _sharedValueTypeProperty = property.Property(nameof(AnimationGroupInfo.SharedValueType));
            _sharedColorTypeProperty = property.Property(nameof(AnimationGroupInfo.SharedColorType));
            _sharedObjectTypeProperty = property.Property(nameof(AnimationGroupInfo.SharedObjectType));

            // bind properties
            _toggleField.BindProperty(_toggleTypeProperty);
            _objectsField.SetBinding(_objectsProperty!);
            _statesField.SetBinding(_sharedStatesProperty!);

            // fix arrays to have initial element
            if (_objectsProperty?.arraySize == 0)
                CreateObject();
            if (_sharedStatesProperty?.arraySize == 0)
                CreateState();
            if (_propertiesProperty?.arraySize == 0)
                CreateProperty();

            // create binding service to search for and validate animation bindings
            if (property.serializedObject.targetObject is not SurgeControl flareControl)
                return;
            var descriptor = flareControl.GetComponentInParent<VRC_AvatarDescriptor>();
            if (!descriptor)
                return;
            var avatarObject = descriptor.gameObject;
            var targetObjects = GetTargetObjects();
            _bindingService = _groupType is AnimationGroupType.Normal ?
                new BindingService(avatarObject, targetObjects)
                : new BindingService(avatarObject, targetObjects, AvatarSearchTypes);

            // bind properties property after binding service creation
            _propertiesField.SetBinding(_propertiesProperty!);

            // invalidate bindings data when target objects change
            // TODO: make not invalidate when objects are reordered (reordering not implemented for now, doesnt matter)
            this.TrackPropertyValue(_objectsProperty!, _ => _bindingService.SetTargetObjects(GetTargetObjects()));

            // update ui thingies when ui thingies change
            this.TrackPropertyValue(_objectsProperty!.Field("size"), _ => UpdateUI());
            this.TrackPropertyValue(_propertiesProperty!.Field("size"), _ => UpdateUI());
            this.TrackPropertyValue(_sharedValueTypeProperty!, _ => UpdateUI());
            this.TrackPropertyValue(_sharedColorTypeProperty!, _ => UpdateUI());
            UpdateUI();
        }

        public void SetData(SerializedProperty menuProperty, Action? onRemoveRequested = null)
        {
            // SetData will run before SetBinding, so unbind stuff here :)
            this.Unbind();

            _paneMenu.SetData(evt => evt.menu.AppendAction("Remove" + _groupType switch
            {
                AnimationGroupType.ObjectToggle => " Object Toggle",
                AnimationGroupType.Normal => " Property Animation",
                AnimationGroupType.Avatar => " Avatar Property Animation",
                _ => ""
            }, _ => onRemoveRequested?.Invoke()));

            _menuTypeProperty = menuProperty.Property(nameof(MenuItemInfo.Type));

            this.TrackPropertyValue(_menuTypeProperty, prop => UpdateUI());
            UpdateUI();
        }

        private GameObject?[] GetTargetObjects()
        {
            if (_groupProperty is null)
                return Array.Empty<GameObject>();
            var group = (AnimationGroupInfo)_groupProperty.boxedValue;
            return group.Objects.Select(o => o is Component c ? c.gameObject : o is GameObject g ? g : null).ToArray();
        }

        private void UpdateUI()
        {
            if (_menuTypeProperty is null)
                return;

            var menuType = (MenuItemType)_menuTypeProperty.enumValueIndex;
            _objectsLabel.Visible(_groupType is AnimationGroupType.Normal || _groupType is AnimationGroupType.ObjectToggle && menuType is not MenuItemType.Toggle and not MenuItemType.Button);
            _exclusionsLabel.Visible(_groupType is AnimationGroupType.Avatar);
            _toggleField.Visible(_groupType is AnimationGroupType.ObjectToggle && menuType is MenuItemType.Toggle or MenuItemType.Button);
            _statesField.Visible(_groupType is AnimationGroupType.Normal or AnimationGroupType.Avatar || _groupType is AnimationGroupType.ObjectToggle && menuType is not MenuItemType.Toggle and not MenuItemType.Button);
            _propertiesContainer.Visible(_groupType is not AnimationGroupType.ObjectToggle);

            _statesField.SetMaxLength(menuType switch
            {
                MenuItemType.Toggle or MenuItemType.Button => 2,
                MenuItemType.FourAxis => 4,
                _ => -1,
            });

            if (_objectsProperty is not null && _propertiesProperty is not null)
            {
                var moreThanOneObject = _objectsProperty.arraySize > 1;
                var moreThanOneProperty = _propertiesProperty.arraySize > 1;
                _objectsLabel.text = moreThanOneObject ? "Target Objects" : "Target Object";
                _propertiesLabel.text = moreThanOneProperty ? "Target Properties" : "Target Property";
                _objectsSeparator.style.borderLeftColor = moreThanOneObject ? SurgeUI.ButtonBorderColor : SurgeUI.TransparentColor;
                _propertiesSeparator.style.borderLeftColor = moreThanOneProperty ? SurgeUI.ButtonBorderColor : SurgeUI.TransparentColor;
                _objectsContainer.style.marginBottom = moreThanOneObject ? 5f : 0f;
                _propertiesContainer.style.marginBottom = moreThanOneProperty ? 5f : 0f;
            }

            if (_sharedValueTypeProperty is not null && _sharedColorTypeProperty is not null)
            {
                var sharedValueType = (PropertyValueType)_sharedValueTypeProperty.enumValueIndex;
                var sharedColorType = (PropertyColorType)_sharedColorTypeProperty.enumValueIndex;
                var isVectorType = sharedValueType is PropertyValueType.Vector2 or PropertyValueType.Vector3 or PropertyValueType.Vector4 && sharedColorType is PropertyColorType.None;
                _compactVectorKey.Visible(isVectorType); // show key label for vector fields
                if (isVectorType)
                {
                    _propertiesContainer.style.marginBottom = 3f; // no bottom margin on properties container, gap will be too large
                    var isVector4 = sharedValueType is PropertyValueType.Vector4;
                    var isVector3 = sharedValueType is PropertyValueType.Vector3;
                    _compactVectorKey[3].Visible(isVector4); // show or hide 'w' label
                    _compactVectorKey[2].Visible(isVector3 || isVector4); // show or hide 'z' label
                    // if w label not shown, add button length to z label.
                    _compactVectorKey[2].WithWidth(AnimationStateElement.VectorFieldWidth + (isVector4 ? -2f : 20f));
                    // if z label not shown, add button length to y label.
                    _compactVectorKey[1].WithWidth(AnimationStateElement.VectorFieldWidth + (isVector4 || isVector3 ? -2f : 20f));
                }
            }
        }

        private void CreateObject()
        {
            if (_objectsProperty is null)
                return;

            var index = _objectsProperty.arraySize++;
            _objectsProperty.serializedObject.ApplyModifiedProperties();
            _objectsProperty.GetArrayElementAtIndex(index).SetValue(null!); // Create a new object.
        }

        private void CreateState()
        {
            if (_sharedStatesProperty is null)
                return;

            var index = _sharedStatesProperty.arraySize++;
            _sharedStatesProperty.serializedObject.ApplyModifiedProperties();
            _sharedStatesProperty.GetArrayElementAtIndex(index).SetValue(null!); // Create a new state.
        }


        private void CreateProperty()
        {
            if (_propertiesProperty is null)
                return;

            var index = _propertiesProperty.arraySize++;
            _propertiesProperty.serializedObject.ApplyModifiedProperties();
            _propertiesProperty.GetArrayElementAtIndex(index).SetValue(null!); // Create a new property.
        }

        private static AnimationObjectElement CreateObjectElement()
        {
            AnimationObjectElement root = new();

            return root;
        }

        private static AnimationStateElement CreateStateElement()
        {
            AnimationStateElement root = new();

            return root;
        }

        private static AnimationPropertyElement CreatePropertyElement()
        {
            AnimationPropertyElement root = new();

            return root;
        }
    }
}
