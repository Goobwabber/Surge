using System.Linq;
using UnityEngine.UIElements;

namespace Surge.Editor.Extensions
{
    internal static class UIToolkitExtensions
    {
        public static Toggle GetToggle(this Foldout foldout)
        {
            return (foldout.hierarchy.Children().First() as Toggle)!;
        }
    }
}