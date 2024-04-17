using Flare.Editor.Extensions;
using Flare.Editor.Models;
using Flare.Models;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flare.Editor.Elements
{
    internal class AnimationObjectElement : VisualElement, IFlareBindable
    {
        private AnimationGroupType _groupType;
        private bool _onlyArrayItem;

        private readonly ObjectField _objectField;
        private readonly ComponentDropdownField _componentField;
        private readonly Button _addButton;

        public AnimationObjectElement()
        {
            this.style.flexDirection = FlexDirection.Row;

            _objectField = new ObjectField().WithFontSize(12f).WithGrow(1f).WithHeight(20f);
            _objectField.RegisterValueChangedCallback(ObjectFieldValueChanged);
            _objectField.style.marginRight = 1f;
            _objectField.style.flexBasis = 1f;
            this.Add(_objectField);

            _componentField = new ComponentDropdownField(null).WithHeight(20f);
            _componentField.style.marginLeft = 1f;
            _componentField.style.marginRight = 1f;
            this.Add(_componentField);

            _addButton = this.CreateButton("+").WithHeight(20f).WithWidth(20f).WithFontSize(18f).WithFontStyle(FontStyle.Bold);
            _addButton.style.paddingTop = 0f;
            _addButton.style.paddingBottom = 2f;
            _addButton.style.marginLeft = 1f;
            _addButton.style.marginRight = 1f;
        }

        public void SetBinding(SerializedProperty property)
        {
            _objectField.BindProperty(property);
            _componentField.BindProperty(property);
        }

        public void SetData(Action? onAddRequested, Action? onRemoveRequested, bool lastArrayItem, bool onlyArrayItem, AnimationGroupType type)
        {
            _groupType = type;
            _onlyArrayItem = onlyArrayItem;
            _addButton.clicked -= onAddRequested;
            _addButton.clicked += onAddRequested;
            _componentField.Visible(type == AnimationGroupType.ObjectToggle);
            _addButton.Visible(lastArrayItem);

            ContextualMenuManipulator rightMenu = new(RightMenuPopulate);
            rightMenu.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
            this.AddManipulator(rightMenu);

            void RightMenuPopulate(ContextualMenuPopulateEvent evt)
            {
                if (!_onlyArrayItem)
                    evt.menu.AppendAction("Remove", evt => onRemoveRequested?.Invoke());
            }
        }

        private void ObjectFieldValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            _componentField.Visible(evt.newValue != null && _groupType is AnimationGroupType.ObjectToggle);
            _componentField.Push(evt.newValue);
        }
    }
}
