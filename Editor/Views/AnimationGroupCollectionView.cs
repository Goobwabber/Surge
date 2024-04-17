using Flare.Editor.Attributes;
using Flare.Editor.Extensions;
using Flare.Models;
using UnityEngine.UIElements;

namespace Flare.Editor.Views
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