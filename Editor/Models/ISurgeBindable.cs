using UnityEditor;

namespace Surge.Editor.Models
{
    internal interface ISurgeBindable
    {
        void SetBinding(SerializedProperty property);
    }
}