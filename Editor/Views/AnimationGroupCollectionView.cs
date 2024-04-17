using Surge.Editor.Attributes;
using Surge.Editor.Extensions;
using Surge.Models;
using UnityEngine.UIElements;

namespace Surge.Editor.Views
{
    internal class AnimationGroupCollectionView : IView
    {
        [PropertyName(nameof(AnimationGroupCollectionInfo.Groups))]
        private readonly AnimationGroupView _animationGroupView = new();

        public void Build(VisualElement root)
        {
            _animationGroupView.Build(root);
            root.CreateHorizontalSpacer(20f);
        }
    }
}