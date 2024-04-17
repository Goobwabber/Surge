using UnityEngine.UIElements;

namespace Surge.Editor.Views
{
    internal interface IView
    {
        void Build(VisualElement root);
    }
}