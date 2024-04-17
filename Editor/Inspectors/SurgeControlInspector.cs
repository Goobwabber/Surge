using System.Text;
using Surge.Editor.Attributes;
using Surge.Editor.Extensions;
using Surge.Editor.Views;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Surge.Editor.Inspectors
{
    [CustomEditor(typeof(SurgeControl))]
    internal class SurgeControlInspector : SurgeInspector
    {
        [PropertyName(nameof(SurgeControl.MenuItem))]
        private readonly MenuItemControlView _menuItemView = new();

        [PropertyName(nameof(SurgeControl.AnimationGroupCollection))]
        private readonly AnimationGroupCollectionView _animationGroupCollectionView = new();

        [PropertyName(nameof(SurgeControl.TagInfo))]
        private readonly TagInfoView _tagInfoView = new();

        protected override void OnInitialization()
        {
            if (target is not SurgeControl control)
                return;
        }

        protected override VisualElement BuildUI(VisualElement root)
        {
            HelpBox misconfigurationErrorBox = new();
            root.Add(misconfigurationErrorBox);
            misconfigurationErrorBox.Visible(false);

            // Menu
            _menuItemView.Build(root);

            // Animations
            _animationGroupCollectionView.Build(root);

            // Display warning for if any object references are not on this avatar.
            root.schedule.Execute(() =>
            {
                if (target is not SurgeControl control)
                    return;

                var notOnAvatar = ListPool<Object?>.Get();
                control.GetReferencesNotOnAvatar(notOnAvatar);

                var errors = notOnAvatar.Count is not 0;
                misconfigurationErrorBox.Visible(errors);
                if (errors)
                {
                    StringBuilder sb = new();
                    sb.AppendLine("Some references are not located under the current avatar.");

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < notOnAvatar.Count; i++)
                    {
                        var invalidObject = notOnAvatar[i];
                        if (invalidObject != null)
                            sb.AppendLine(invalidObject.ToString());
                    }

                    misconfigurationErrorBox.messageType = HelpBoxMessageType.Error;
                    misconfigurationErrorBox.text = sb.ToString();
                }

                ListPool<Object?>.Release(notOnAvatar);

            }).Every(20);

            return root;
        }
    }
}