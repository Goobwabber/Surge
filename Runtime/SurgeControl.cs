using Surge.Models;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Surge
{
    internal class SurgeControl : SurgeModule
    {
        [field: SerializeField]
        public MenuItemInfo MenuItem { get; private set; } = new();

        [field: SerializeField]
        public AnimationGroupCollectionInfo AnimationGroupCollection { get; private set; } = new();

        [field: SerializeField]
        public TagInfo TagInfo { get; private set; } = new();

        [field: SerializeField]
        public SettingsInfo Settings { get; private set; } = new();

        private void OnValidate()
        {
            TagInfo.EnsureValidated(gameObject);

            // TODO
            //foreach (var toggle in ObjectToggleCollection.Toggles)
            //    toggle.EnsureValidated();
        }

        public void GetReferencesNotOnAvatar(ICollection<Object?> references)
        {
            if (this == null || transform == null)
                return;

            var descriptor = transform.GetComponentInParent<VRCAvatarDescriptor>();
            if (!descriptor)
                return;

            var root = descriptor.transform;

            foreach (var group in AnimationGroupCollection.Groups)
            {
                foreach(var item in group.Objects)
                {
                    if (item == null)
                        continue;

                    SearchTransform(root, item is Component c ? c.transform : ((GameObject)item).transform, item, references);
                }
            }
        }

        public void SetName(string newName)
        {
            if (gameObject.GetComponentInParent<SurgeMenu>(true) is null)
                return;

            name = newName;
            MenuItem.Path = newName;
        }

        private static void SearchTransform(Object root, Transform? target, Object? source, ICollection<Object?> references)
        {
            if (target == null)
                return;

            bool isInAvatar = false;
            while (target != target.root)
            {
                target = target.parent;
                if (target != root)
                    continue;

                isInAvatar = true;
                break;
            }

            if (isInAvatar)
                return;

            references.Add(source);
        }
    }
}
