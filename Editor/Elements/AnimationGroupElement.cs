using Flare.Editor.Extensions;
using Flare.Editor.Models;
using Flare.Editor.Services;
using Flare.Models;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using AnimationPropertyInfo = Flare.Models.AnimationPropertyInfo;

namespace Flare.Editor.Elements
{
    internal class AnimationGroupElement : VisualElement, IFlareBindable
    {
        // constants
        private const float labelWidth = 105f;
        public readonly Type[] AvatarSearchTypes = { typeof(Renderer), typeof(Behaviour) };

        // properties
        private SerializedProperty? _groupProperty;
        private SerializedProperty? _objectsProperty;
        private SerializedProperty? _sharedStatesProperty;
        private SerializedProperty? _propertiesProperty;
        private SerializedProperty? _toggleTypeProperty;
        private SerializedProperty? _menuTypeProperty;
        private SerializedProperty? _sharedValueTypeProperty;
        private SerializedProperty? _sharedColorTypeProperty;
        private SerializedProperty? _sharedObjectTypeProperty;

        // vars
        private AnimationGroupType _groupType;
        private BindingService _bindingService;

        // objects section
        private readonly Label _objectsLabel;
        private readonly Label _exclusionsLabel;
        private readonly LabelledEnumField _toggleField;
        private readonly VisualElement _objectsSeparator;
        private readonly FlareCollectionView<AnimationObjectElement> _objectsField;
        private readonly PaneMenu _paneMenu;

        // properties section
        private readonly VisualElement _propertyContainer;
        private readonly Label _propertiesLabel;
        private readonly VisualElement _propertiesSeparator;
        private readonly FlareCollectionView<AnimationPropertyElement> _propertiesField;

        // states section
        private readonly FlareCollectionView<AnimationStateElement> _statesField;

        public AnimationGroupElement()
        {
            _paneMenu = new PaneMenu().WithWidth(10f).WithHeight(20f);
            _paneMenu.style.marginLeft = 5f;

            _objectsField = new(CreateObjectElement, (e, i) => e.SetData(CreateObject, () =>
            {
                _objectsProperty?.DeleteArrayElementAtIndex(i);
                _objectsProperty?.serializedObject?.ApplyModifiedProperties();
            }, _objectsProperty?.arraySize - 1 == i, _objectsProperty?.arraySize == 1, _groupType));
            _objectsField.WithGrow(1f);

            _objectsSeparator = new VisualElement().WithMargin(0f).WithPadding(0f).WithBorderWidth(0f);
            _objectsSeparator.WithBorderColor(FlareUI.ButtonBorderColor);
            _objectsSeparator.style.borderLeftWidth = 2f;
            _objectsSeparator.style.marginLeft = 2f;

            _objectsLabel = new Label("Target Objects");
            _objectsLabel.style.alignSelf = Align.FlexStart;
            _objectsLabel.style.marginTop = 1f;
            _objectsLabel.style.paddingTop = 2f;
            _objectsLabel.style.width = labelWidth;
            _objectsLabel.tooltip = "The objects to be animated.";

            _exclusionsLabel = new Label("Excluded Objects");
            _exclusionsLabel.style.alignSelf = Align.FlexStart;
            _exclusionsLabel.style.marginTop = 1f;
            _exclusionsLabel.style.paddingTop = 2f;
            _exclusionsLabel.style.width = labelWidth;
            _exclusionsLabel.tooltip = "The objects to be excluded from this avatar-wide animation.";

            _toggleField = new LabelledEnumField(AnimationToggleType.TurnOn, "When Active: ",
                "Whether to enable these objects or disable them when the menu item is active.",
                value => value == (int)AnimationToggleType.TurnOff ? FlareUI.DisabledColor : FlareUI.EnabledColor);
            _toggleField.style.width = 160f;

            // Create the container for the object picker
            var objectsContainer = this.CreateHorizontal();
            objectsContainer.Add(_objectsLabel);
            objectsContainer.Add(_exclusionsLabel);
            objectsContainer.Add(_toggleField);
            objectsContainer.Add(_objectsSeparator);
            objectsContainer.Add(_objectsField);
            objectsContainer.Add(_paneMenu);



            // Animation Properties
            _propertiesField = new(CreatePropertyElement, (e, i) => e.SetData(CreateProperty, () =>
            {
                _propertiesProperty?.DeleteArrayElementAtIndex(i);
                _propertiesProperty?.serializedObject?.ApplyModifiedProperties();
            }, _propertiesProperty?.arraySize - 1 == i, _propertiesProperty?.arraySize == 1, 
            _objectsProperty, _sharedValueTypeProperty, _sharedColorTypeProperty, _sharedObjectTypeProperty, _bindingService, _groupType));
            _propertiesField.WithGrow(1f);

            _propertiesLabel = new Label("Target Properties");
            _propertiesLabel.style.alignSelf = Align.FlexStart;
            _propertiesLabel.style.marginTop = 1f;
            _propertiesLabel.style.paddingTop = 2f;
            _propertiesLabel.style.width = labelWidth;
            _propertiesLabel.tooltip = "The properties to be animated.";

            _propertiesSeparator = new VisualElement().WithMargin(0f).WithPadding(0f).WithBorderWidth(0f);
            _propertiesSeparator.WithBorderColor(FlareUI.ButtonBorderColor);
            _propertiesSeparator.style.borderLeftWidth = 2f;
            _propertiesSeparator.style.marginLeft = 2f;

            var propertiesMargin = new VisualElement().WithWidth(15f);

            // Create the container for animation properties
            _propertyContainer = this.CreateHorizontal();
            _propertyContainer.Add(_propertiesLabel);
            _propertyContainer.Add(_propertiesSeparator);
            _propertyContainer.Add(_propertiesField);
            _propertyContainer.Add(propertiesMargin);



            // Animation States
            _statesField = new(CreateStateElement, (e, i) => e.SetData(CreateState, () =>
            {
                _sharedStatesProperty?.DeleteArrayElementAtIndex(i);
                _sharedStatesProperty?.serializedObject?.ApplyModifiedProperties();
            }, _sharedStatesProperty?.arraySize - 1 == i, _sharedStatesProperty?.arraySize ?? 0, _menuTypeProperty, _groupType));
            _statesField.WithGrow(1f);

            var statesMargin = new VisualElement().WithWidth(15f);

            // Create the container for animation states 
            var statesContainer = this.CreateHorizontal().WithBorderColor(FlareUI.BorderColor);
            statesContainer.Add(_statesField);
            statesContainer.Add(statesMargin);
        }

        public void SetBinding(SerializedProperty property)
        {
            // properties
            _groupProperty = property.Copy();
            _groupType = (AnimationGroupType)property.Property(nameof(AnimationGroupInfo.AnimationType)).enumValueIndex;
            _objectsProperty = property.Property(nameof(AnimationGroupInfo.Objects)).Field("Array");
            _propertiesProperty = property.Property(nameof(AnimationGroupInfo.Properties)).Field("Array");
            _sharedStatesProperty = property.Property(nameof(AnimationGroupInfo.SharedAnimationStates)).Field("Array");
            _toggleTypeProperty = property.Property(nameof(AnimationGroupInfo.ToggleType));

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

            // first property carries the shared value type
            UpdateSharedValueType();
            this.TrackPropertyValue(_objectsProperty, _ => UpdateSharedValueType());
            void UpdateSharedValueType()
            {
                var firstPropertyProperty = _propertiesProperty!.GetArrayElementAtIndex(0)!;
                _sharedValueTypeProperty = firstPropertyProperty.Property(nameof(AnimationPropertyInfo.ValueType));
                _sharedObjectTypeProperty = firstPropertyProperty.Property(nameof(AnimationPropertyInfo.ObjectValueType));
                _sharedColorTypeProperty = firstPropertyProperty.Property(nameof(AnimationPropertyInfo.ColorType));
            }

            // create binding service to search for and validate animation bindings
            if (property.serializedObject.targetObject is not FlareControl flareControl)
                return;
            var descriptor = flareControl.GetComponentInParent<VRC_AvatarDescriptor>();
            if (!descriptor)
                return;
            var avatarObject = descriptor.gameObject;
            var targetObjects = GetTargetObjects();
            _bindingService = _groupType is AnimationGroupType.Normal ?
                new BindingService(avatarObject, targetObjects)
                : new BindingService(avatarObject, targetObjects, AvatarSearchTypes);

            // bind properties property after getting shared value type properties and binding service creation
            _propertiesField.SetBinding(_propertiesProperty!);
            // TODO: make this update if elements get reordered (reordering not implemented for now, doesnt matter)
            //_propertiesField.RegisterSizeChangedCallback(() => UpdateSharedValueType());

            // invalidate bindings data when target objects change
            // TODO: make not invalidate when objects are reordered (reordering not implemented for now, doesnt matter)
            this.TrackPropertyValue(_objectsProperty!, _ => _bindingService.SetTargetObjects(GetTargetObjects()));

            // update ui thingies when ui thingies change
            this.TrackPropertyValue(_objectsProperty!.Field("size"), _ => UpdateUI());
            this.TrackPropertyValue(_propertiesProperty!.Field("size"), _ => UpdateUI());
            UpdateUI();
        }

        public void SetData(SerializedProperty menuProperty, Action? onRemoveRequested = null)
        {
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
            _statesField.Visible(_groupType is AnimationGroupType.Normal || _groupType is AnimationGroupType.ObjectToggle && menuType is not MenuItemType.Toggle and not MenuItemType.Button);
            _propertyContainer.Visible(_groupType is not AnimationGroupType.ObjectToggle);

            if (_objectsProperty is not null && _propertiesProperty is not null)
            {
                var moreThanOneObject = _objectsProperty.arraySize > 1;
                var moreThanOneProperty = _propertiesProperty.arraySize > 1;
                _objectsLabel.text = moreThanOneObject ? "Target Objects" : "Target Object";
                _propertiesLabel.text = moreThanOneProperty ? "Target Properties" : "Target Property";
                _objectsSeparator.style.borderLeftColor = moreThanOneObject ? FlareUI.ButtonBorderColor : FlareUI.TransparentColor;
                _propertiesSeparator.style.borderLeftColor = moreThanOneObject ? FlareUI.ButtonBorderColor : FlareUI.TransparentColor;
                _propertyContainer.style.marginTop = moreThanOneObject ? 5f : 0f;
                _statesField.style.marginTop = moreThanOneObject || moreThanOneProperty ? 5f : 0f;
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

        private void CreateProperty() => CreateProperty(true);

        private void CreateProperty(bool isNull = true)
        {
            if (_propertiesProperty is null)
                return;

            var index = _propertiesProperty.arraySize++;
            _propertiesProperty.serializedObject.ApplyModifiedProperties();
            _propertiesProperty.GetArrayElementAtIndex(index).SetValue(isNull ? null! : CreatePropertyElement()); // Create a new property.
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
