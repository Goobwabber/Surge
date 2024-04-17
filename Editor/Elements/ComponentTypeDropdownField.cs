using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Flare.Editor.Elements
{
    internal class ComponentTypeDropdownField : PopupField<Type>
    {
        public bool RemoveDuplicateTypes { get; set; } = false;
        public bool NullSelection { get; set; } = false;

        public ComponentTypeDropdownField(GameObject? gameObject) : base(
            gameObject.AsNullable()?.GetComponents<Component>()?.Select(c => c.GetType()).ToList() ?? new List<Type>(0),
            0,
            FormatType,
            FormatType
            )
        {
            
        }

        public void Push(Object? target)
        {
            var gameObject = target as GameObject;
            if (gameObject is null && target is Component component)
                gameObject = component.gameObject;
            
            choices = gameObject.AsNullable()?.GetComponents<Component>()?.Select(c => c.GetType()).ToList() ??
                      new List<Type>(0);
        }

        public void Push(Object[]? targets)
        {
            var seenTypes = RemoveDuplicateTypes ? new List<Type?>() : null;

            choices = targets.SelectMany(t =>
            {
                var gameObject = t as GameObject;
                if (gameObject is null && t is Component component)
                    gameObject = component.gameObject;

                return gameObject.AsNullable()?.GetComponents<Component>()?.Select(c => c.GetType()) ?? new List<Type>(0);
            }).Where(t =>
            {
                if (!RemoveDuplicateTypes)
                    return true;
                if (seenTypes!.Contains(t))
                    return false;
                seenTypes!.Add(t);
                return true;
            }).ToList();

            choices.Insert(0, typeof(GameObject));
            
            var rendererIndex = choices.FindIndex(t => typeof(Renderer).IsAssignableFrom(t));
            if (rendererIndex != -1)
                choices.Insert(rendererIndex, typeof(Renderer));

            if (NullSelection)
                choices.Insert(0, null);
        }

        private static string FormatType(Type type)
        {
            if (type == null)
                return "Any " + nameof(Component);

            return type.Name;
        }
    }
}