using UnityEditor;
using UnityEngine;

namespace Flare.Editor
{
    internal static class FlareUI
    {
        public static readonly Color FullColor = EditorGUIUtility.isProSkin
            ? new Color(0xFF, 0xFF, 0xFF, 0xFF) : new Color32(0x00, 0x00, 0x00, 0xFF);

        public static readonly Color BorderColor = new Color32(0x1A, 0x1A, 0x1A, 0xFF);
        
        public static readonly Color BackgroundColor = EditorGUIUtility.isProSkin
            ? new Color32(0x46, 0x46, 0x46, 0xFF) :  new Color32(0xDE, 0xDE, 0xDE, 0xFF);

        public static readonly Color ButtonColor = EditorGUIUtility.isProSkin
            ? new Color32(0xE4, 0xE4, 0xE4, 0xFF) : new Color32(0x09, 0x09, 0x09, 0xFF);

        public static readonly Color ButtonBorderColor = EditorGUIUtility.isProSkin
            ? new Color32(0x30, 0x30, 0x30, 0xFF) : new Color32(0xB2, 0xB2, 0xB2, 0xFF);

        public static readonly Color ButtonBorder2Color = EditorGUIUtility.isProSkin
            ? new Color32(0x24, 0x24, 0x24, 0xFF) : new Color32(0x93, 0x93, 0x93, 0xFF);

        public static readonly Color ButtonBackgroundColor = EditorGUIUtility.isProSkin
            ? new Color32(0x51, 0x51, 0x51, 0xFF) : new Color32(0xDF, 0xDF, 0xDF, 0xFF);
        
        public static readonly Color EnabledColor = EditorGUIUtility.isProSkin
            ? new Color32(0x5B, 0xDD, 0x55, 0xFF) : new Color32(0x00, 0x7C, 0x02, 0xFF);
        
        public static readonly Color DisabledColor = EditorGUIUtility.isProSkin
            ? new Color32(0xFF, 0x7C, 0x7C, 0xFF) : new Color32(0x93, 0x00, 0x00, 0xFF);

        private static Texture2D? _warningImage;

        public static Texture2D GetWarningImage()
        {
            if (_warningImage)
                return _warningImage;
            _warningImage = (Texture2D)EditorGUIUtility.IconContent("Warning@2x").image;
            return _warningImage;
        }
    }
}