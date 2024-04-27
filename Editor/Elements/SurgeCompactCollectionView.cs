using System;
using Surge.Editor.Extensions;
using Surge.Editor.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Surge.Editor.Elements
{
    internal class SurgeCompactCollectionView<T> : VisualElement, ISurgeBindable where T : VisualElement, ISurgeBindable
    {
        private readonly Func<T> _builder;
        private readonly VisualElement _container;

        private Action<T, int>? _binder;
        private SerializedProperty? _arrayProperty;
        private string _removeName = string.Empty;
        private int _maxLength = -1;

        public SurgeCompactCollectionView(
            Func<T> builder,
            Action<T, int>? binder = null,
            SerializedProperty? arrayProperty = null)
        {
            _binder = binder;
            _builder = builder;
            _arrayProperty = arrayProperty;
            _container = this.CreateElement();
            
            if (arrayProperty is not null)
                SetBinding(arrayProperty);
        }

        private void BuildCollectionView()
        {
            if (_arrayProperty == null)
                return;
            
            var index = 0;
            var endProperty = _arrayProperty.GetEndProperty();

            // Copy the array property to iterate over it.
            var property = _arrayProperty.Copy();

            // Look at the next property.
            property.NextVisible(true);

            do // If I had a dollar for every time I used a do while loop in a project, I would have $1
            {
                // Stop looking after the end property
                if (SerializedProperty.EqualContents(property, endProperty))
                    break;
                
                // Ignore array size properties.
                if (property.propertyType is SerializedPropertyType.ArraySize)
                    continue;

                CompactCollectionElement element;
                if (index >= _container.childCount)
                {
                    // Add new elements as the array has grown
                    element = new CompactCollectionElement(this);
                }
                else
                {
                    // We don't need to resize for this property.
                    element = (CompactCollectionElement)_container[index];
                }

                var targetIndex = index;
                _binder?.Invoke(element.contents, targetIndex);
                element.SetPosition(targetIndex, _arrayProperty.arraySize);
                element.contents.SetBinding(property);
                index++;
            }
            while (property.NextVisible(false));

            // Remove any extra elements.
            while (_container.childCount > index)
                _container.RemoveAt(_container.childCount - 1);

            // Remove elements that shouldn't be visible
            if (_maxLength != -1)
                while (_container.childCount > _maxLength)
                    _container.RemoveAt(_container.childCount - 1);
        }

        public void SetData(Action<T, int> binder)
        {
            _binder = binder;
        }
        
        /// <summary>
        /// Bind to this collection view.
        /// </summary>
        /// <param name="property">The array property</param>
        public void SetBinding(SerializedProperty property)
        {
            _arrayProperty = property.Copy();
            var arraySizeProperty = _arrayProperty.Field("size");

            this.Unbind();
            this.TrackPropertyValue(arraySizeProperty, _ => BuildCollectionView());
            BuildCollectionView();
        }

        public void SetRemoveName(string removeName)
        {
            _removeName = ' ' + removeName;
        }

        public void SetMaxLength(int maxLength)
        {
            _maxLength = maxLength;
            BuildCollectionView();
        }

        private void ClampMaxLength(int length)
        {
            if (_arrayProperty is null || length == -1 || _arrayProperty.arraySize <= length)
                return;

            for(int i = _arrayProperty.arraySize; i > length; i--)
            {
                _arrayProperty.DeleteArrayElementAtIndex(i - 1);
                _arrayProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnAddRequested()
        {
            var index = _arrayProperty.arraySize++;
            _arrayProperty.serializedObject.ApplyModifiedProperties();
            _arrayProperty.GetArrayElementAtIndex(index).SetValue(null!);
        }

        private void OnRemoveRequested(int index)
        {
            // i just remove the properties here bc when i removed them inside of 'SetMaxLength' it caused gc issues and i AINT dealing with figuring that out.
            // hiding the overflow elements from the user works for now.
            ClampMaxLength(_maxLength);
            if (_maxLength != -1 && index > _maxLength - 1)
                return;
            _arrayProperty.DeleteArrayElementAtIndex(index);
            _arrayProperty.serializedObject.ApplyModifiedProperties();
            _container.RemoveAt(_container.childCount - 1);
        }

        private class CompactCollectionElement : VisualElement
        {
            private readonly SurgeCompactCollectionView<T> _collectionView;
            public readonly T contents;
            public readonly Button addButton;

            private int _collectionIndex = 0;
            private bool _onlyArrayItem = true;

            public CompactCollectionElement(SurgeCompactCollectionView<T> collectionView)
            {
                _collectionView = collectionView;

                this.WithMargin(0f).WithPadding(0f).WithBorderWidth(0f);
                this.style.flexDirection = FlexDirection.Row;
                contents = _collectionView._builder.Invoke().WithGrow(1f);
                this.Add(contents);

                addButton = this.CreateButton("+").WithHeight(20f).WithWidth(20f).WithFontSize(18f).WithFontStyle(FontStyle.Bold);
                addButton.style.paddingTop = 0f;
                addButton.style.paddingBottom = 2f;
                addButton.style.marginLeft = 1f;
                addButton.style.marginRight = 1f;

                addButton.clicked -= _collectionView.OnAddRequested;
                addButton.clicked += _collectionView.OnAddRequested;

                ContextualMenuManipulator rightMenu = new(RightMenuPopulate);
                rightMenu.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
                this.AddManipulator(rightMenu);

                void RightMenuPopulate(ContextualMenuPopulateEvent evt)
                {
                    if (_onlyArrayItem)
                        return;
                    if (evt.menu.MenuItems().Count > 0)
                        evt.menu.AppendSeparator();
                    evt.menu.AppendAction("Remove" + _collectionView._removeName, evt => _collectionView.OnRemoveRequested(_collectionIndex));
                }

                _collectionView._container.Add(this);
            }

            public void SetPosition(int collectionIndex, int collectionLength)
            {
                var maxLength = _collectionView._maxLength;
                _onlyArrayItem = collectionLength == 1;
                _collectionIndex = collectionIndex;
                addButton.Visible(collectionIndex == collectionLength - 1 && collectionIndex != maxLength - 1);
            }
        }
    }
}