using Surge.Editor.Attributes;
using Surge.Editor.Extensions;
using Surge.Models;
using UnityEditor;
using UnityEngine.UIElements;

namespace Surge.Editor.Views
{
    internal class ContactControlView : IView
    {
        [PropertyName(nameof(ContactInfo.ContactReceiver))]
        private readonly SerializedProperty _contactProperty = null!;
        
        public void Build(VisualElement root)
        {
            root.CreatePropertyField(_contactProperty);
            root.CreateHorizontalSpacer(10f);
        }
    }
}