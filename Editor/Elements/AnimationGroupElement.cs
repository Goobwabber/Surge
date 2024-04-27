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
        private SerializedProperty? _isPlatformExclusiveProperty;
        private SerializedProperty? _showSharedCurveProperty;
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
        private readonly Label _curveFieldLabel;
        private readonly CurveField _curveField;

        // settings bar
        private readonly VisualElement _settingsContainer;
        private readonly LabelledEnumField _groupTypeField;
        private readonly LabelledEnumField _platformOnlyField;
        private readonly LabelledEnumField _easingField;
        private readonly SettingLabelElement _curveLabel;

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
            _statesField.style.marginBottom = 5f;
            container.Add(_statesField);

            // Animation States: curve
            _curveFieldLabel = new Label("X Axis: Radial value, Y Axis: Value to set the properties to.").WithMarginLeft(3f);
            container.Add(_curveFieldLabel);
            _curveField = new CurveField().WithHeight(40f);
            _curveField.ranges = new Rect(0, -1, 1, -1);
            _curveField.tooltip = "The curve to control the property values with. X axis is the input value, and the Y axis is the desired value for the properties to be.";
            container.Add(_curveField);



            // Settings bar
            _settingsContainer = container.CreateHorizontal();
            _settingsContainer.style.alignItems = Align.FlexStart;
            _settingsContainer.style.flexWrap = Wrap.Wrap;
            _settingsContainer.style.marginTop = 1f;
            _settingsContainer.style.marginBottom = 2f;

            _groupTypeField = new LabelledEnumField(AnimationGroupType.Normal, "Group Type: ", "The type of this animation group.");
            _settingsContainer.Add(_groupTypeField);
            _groupTypeField.SetEnabled(false); // fuck you if you want to change the group type
            _easingField = new LabelledEnumField(SurgeEasing.Sine, "Easing: ", "Easing is the rate of value change. Basically, this is the way this animation group will smooth between different values.");
            _settingsContainer.Add(_easingField);
            _platformOnlyField = new LabelledEnumField(SurgePlatformType.PC, "Only on: ", "This animation group will only build on a specific platform.");
            _settingsContainer.Add(_platformOnlyField);
            _curveLabel = new SettingLabelElement("Curve", "This animation group uses a curve to determine it's output values.");
            _settingsContainer.Add(_curveLabel);

            ContextualMenuManipulator settingMenu = new(SettingsMenuPopulate);
            settingMenu.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
            _settingsContainer.AddManipulator(settingMenu);
        }

        public void SetBinding(SerializedProperty property)
        {
            // properties
            _groupProperty = property.Copy();
            var groupTypeProperty = property.Property(nameof(AnimationGroupInfo.GroupType));
            _groupType = (AnimationGroupType)groupTypeProperty.enumValueIndex;
            _toggleTypeProperty = property.Property(nameof(AnimationGroupInfo.ToggleType));
            _objectsProperty = property.Property(nameof(AnimationGroupInfo.Objects)).Field("Array");
            _propertiesProperty = property.Property(nameof(AnimationGroupInfo.Properties)).Field("Array");
            _sharedStatesProperty = property.Property(nameof(AnimationGroupInfo.SharedAnimationStates)).Field("Array");
            _sharedValueTypeProperty = property.Property(nameof(AnimationGroupInfo.SharedValueType));
            _sharedColorTypeProperty = property.Property(nameof(AnimationGroupInfo.SharedColorType));
            _sharedObjectTypeProperty = property.Property(nameof(AnimationGroupInfo.SharedObjectType));
            var easingProperty = property.Property(nameof(AnimationGroupInfo.GroupEasing));
            _isPlatformExclusiveProperty = property.Property(nameof(AnimationGroupInfo.IsPlatformExclusive));
            var platformProperty = property.Property(nameof(AnimationGroupInfo.PlatformType));
            _showSharedCurveProperty = property.Property(nameof(AnimationGroupInfo.ShowSharedCurve));
            var sharedCurveProperty = property.Property(nameof(AnimationGroupInfo.SharedCurve));

            // bind properties
            _toggleField.BindProperty(_toggleTypeProperty);
            _objectsField.SetBinding(_objectsProperty!);
            _statesField.SetBinding(_sharedStatesProperty!);
            _curveField.BindProperty(sharedCurveProperty);
            _groupTypeField.BindProperty(groupTypeProperty);
            _easingField.BindProperty(easingProperty);
            _platformOnlyField.BindProperty(platformProperty);


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

            _paneMenu.SetData(evt => 
            {
                SettingsMenuPopulate(evt);
                if (evt.menu.MenuItems().Count > 0)
                    evt.menu.AppendSeparator();
                evt.menu.AppendAction("Remove" + _groupType switch
                {
                    AnimationGroupType.ObjectToggle => " Object Toggle",
                    AnimationGroupType.Normal => " Property Animation",
                    AnimationGroupType.Avatar => " Global Property Animation",
                    _ => ""
                }, _ => onRemoveRequested?.Invoke());
            });

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

        void SettingsMenuPopulate(ContextualMenuPopulateEvent evt)
        {
            if (_menuTypeProperty is null)
                return;

            var enabledStatus = DropdownMenuAction.Status.Checked;
            var disabledStatus = DropdownMenuAction.Status.Normal;
            var lockedStatus = DropdownMenuAction.Status.Disabled;

            var radialMode = _menuTypeProperty.enumValueIndex == (int)MenuItemType.Radial;
            var sharedValueType = (PropertyValueType?)_sharedValueTypeProperty?.enumValueIndex;
            var allowCurve = radialMode && sharedValueType is PropertyValueType.Boolean or PropertyValueType.Integer or PropertyValueType.Float;

            evt.menu.AppendAction("Platform Exclusive",
                evt => UpdateValues(_isPlatformExclusiveProperty),
                _isPlatformExclusiveProperty?.boolValue ?? false ? enabledStatus : disabledStatus);
            evt.menu.AppendAction("Custom Animation Curve",
                evt => UpdateValues(_showSharedCurveProperty),
                !allowCurve ? lockedStatus : _showSharedCurveProperty?.boolValue ?? false ? enabledStatus : disabledStatus);

            void UpdateValues(SerializedProperty? prop)
            {
                if (prop is null)
                    return;
                prop.boolValue = !prop.boolValue;
                prop.serializedObject.ApplyModifiedProperties();
                UpdateUI();
            }
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

            if (_isPlatformExclusiveProperty is not null)
                _platformOnlyField.Visible(_isPlatformExclusiveProperty.boolValue);

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

                if (_showSharedCurveProperty is not null)
                {
                    // we can only allow the user to use an animation curve if the value type is analog.
                    var showSharedCurve = _showSharedCurveProperty.boolValue && sharedValueType is PropertyValueType.Boolean or PropertyValueType.Integer or PropertyValueType.Float;
                    _easingField.Visible(menuType is MenuItemType.Radial && !showSharedCurve);
                    var curveFieldVisible = menuType is MenuItemType.Radial && showSharedCurve;
                    _curveLabel.Visible(curveFieldVisible);
                    _curveFieldLabel.Visible(curveFieldVisible);
                    _curveField.Visible(curveFieldVisible);
                    _statesField.Visible(!curveFieldVisible);
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
