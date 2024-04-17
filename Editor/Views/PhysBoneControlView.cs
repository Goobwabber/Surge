using Surge.Editor.Attributes;
using Surge.Editor.Extensions;
using Surge.Models;
using UnityEditor;
using UnityEngine.UIElements;

namespace Surge.Editor.Views
{
    internal class PhysBoneControlView : IView
    {
        [PropertyName(nameof(PhysBoneInfo.PhysBone))]
        private readonly SerializedProperty _physBoneProperty = null!;

        [PropertyName(nameof(PhysBoneInfo.ParameterType))]
        private readonly SerializedProperty _parameterTypeProperty = null!;
        
        public void Build(VisualElement root)
        {
            root.CreatePropertyField(_physBoneProperty);
            root.CreatePropertyField(_parameterTypeProperty);
            root.CreateHorizontalSpacer(10f);
        }
    }
}