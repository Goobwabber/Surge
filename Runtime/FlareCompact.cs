using Flare.Models;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Flare
{
    internal class FlareCompact : FlareModule
    {
        [field: SerializeField]
        public string Path { get; set; } = string.Empty;

        [field: SerializeField]
        public Texture2D Icon { get; private set; } = null!;

        [field: SerializeField]
        public MenuItemType Type { get; private set; } = MenuItemType.Toggle;

        [field: SerializeField]
        public bool IsSaved { get; private set; } = true;

        [field: SerializeField]
        public ToggleMenuState DefaultState { get; private set; } = ToggleMenuState.Inactive;

        [field: Range(0, 1)]
        [field: SerializeField]
        public float DefaultRadialValue { get; private set; }

        [field: SerializeField]
        public PuppetMenuState DefaultPuppetState { get; private set; }

        [field: SerializeField]
        public InterpolationInfo Interpolation { get; private set; } = new();

        [field: SerializeField]
        public bool ApplyToAvatar { get; private set; } = new();

        [field: SerializeField]
        public bool ShowDefault { get; private set; }

        [field: SerializeField]
        public bool ShowIcon { get; private set; }

        [field: SerializeField]
        public bool ShowDuration { get; private set; }

        [field: SerializeField]
        public ObjectToggleCollectionInfo ObjectToggleCollection { get; private set; } = new();

        [field: SerializeField]
        public PropertyGroupCollectionInfo PropertyGroupCollection { get; private set; } = new();

        [field: SerializeField]
        public TagInfo TagInfo { get; private set; } = new();

        [field: SerializeField]
        public SettingsInfo Settings { get; private set; } = new();

        private void OnValidate()
        {
            TagInfo.EnsureValidated(gameObject);

            foreach (var toggle in ObjectToggleCollection.Toggles)
                toggle.EnsureValidated();
        }

        public void GetReferencesNotOnAvatar(ICollection<Object?> references)
        {
            if (this == null || transform == null)
                return;

            var descriptor = transform.GetComponentInParent<VRCAvatarDescriptor>();
            if (!descriptor)
                return;

            var root = descriptor.transform;

            foreach (var toggle in ObjectToggleCollection.Toggles)
            {
                var target = toggle.GetTargetTransform();
                SearchTransform(root, target, toggle.Target, references);
            }

            foreach (var group in PropertyGroupCollection.Groups)
            {
                var search = group.SelectionType is PropertySelectionType.Normal ? group.Inclusions : group.Exclusions;
                foreach (var item in search)
                {
                    if (item == null)
                        continue;

                    SearchTransform(root, item.transform, item, references);
                }
            }
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
