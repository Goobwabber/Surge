using Flare.Editor.Attributes;
using Flare.Editor.Elements;
using Flare.Editor.Extensions;
using Flare.Models;
using UnityEditor;
using UnityEngine.UIElements;

namespace Flare.Editor.Views
{
    internal class AnimationGroupView : IView
    {
        [PropertyName(nameof(FlareControl.MenuItem))]
        private readonly SerializedProperty _menuProperty = null!;

        [PropertyName("Array")]
        private readonly SerializedProperty _arrayProperty = null!;

        public void Build(VisualElement root)
        {
            FlareCollectionView<AnimationGroupElement> view = new(CreateGroupElement, (e, i) =>
            {
                e.SetData(_menuProperty, () =>
                {
                    _arrayProperty.DeleteArrayElementAtIndex(i);
                    _arrayProperty.serializedObject.ApplyModifiedProperties();
                });
            }, _arrayProperty);

            root.Add(view);
            var groupButton = root.CreateButton("Add New Animation Group");

            ContextualMenuManipulator groupMenu = new(GroupMenuPopulate);
            groupMenu.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            groupButton.AddManipulator(groupMenu);

            void GroupMenuPopulate(ContextualMenuPopulateEvent evt)
            {
                evt.menu.AppendAction("Object Toggle", evt => CreateGroup(AnimationGroupType.ObjectToggle));
                evt.menu.AppendAction("Property", evt => CreateGroup(AnimationGroupType.Normal));
                evt.menu.AppendAction("Avatar Property", evt => CreateGroup(AnimationGroupType.Avatar));

                void CreateGroup(AnimationGroupType type)
                {
                    var index = _arrayProperty.arraySize++;
                    _arrayProperty.serializedObject.ApplyModifiedProperties();
                    _arrayProperty.GetArrayElementAtIndex(index).SetValue(new AnimationGroupInfo(type)); // Create a new group.
                }
            }
        }

        private static AnimationGroupElement CreateGroupElement()
        {
            AnimationGroupElement root = new();

            root
                .WithBorderColor(FlareUI.BorderColor)
                .WithBorderRadius(3f)
                .WithBorderWidth(1f)
                .WithMarginTop(5f)
                .WithPadding(5f);

            return root;
        }
    }
}