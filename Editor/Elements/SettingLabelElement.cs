using Flare.Editor.Extensions;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flare.Editor.Elements
{
    internal class SettingLabelElement : VisualElement
    {
        public string Value { get => _label.text; set => _label.text = value; }

        private VisualElement? _icon;
        private Label _label;

        public SettingLabelElement(string labelText, float height = 20f)
        {
            this.WithHeight(height).WithFontSize(12f).WithTextAlign(TextAnchor.MiddleLeft).WithBorderWidth(1f).WithColor(FlareUI.ButtonColor)
            .WithBackgroundColor(FlareUI.ButtonBackgroundColor).WithBorderColor(FlareUI.ButtonBorderColor).WithBorderRadius(3f);
            style.borderBottomColor = FlareUI.ButtonBorder2Color;
            style.borderRightColor = FlareUI.ButtonBorder2Color;
            style.paddingTop = 1f;
            style.paddingBottom = 1f;
            style.paddingLeft = 4f;
            style.paddingRight = 4f;
            style.marginTop = 1f;
            style.marginBottom = 1f;
            style.marginLeft = 2f;
            style.marginRight = 1f;
            _label = new Label(labelText).WithMargin(0f).WithPadding(0f);
            _label.Q<TextElement>().WithHeight(height - 4f);
            Add(_label);
        }

        public SettingLabelElement(string labelText, string tooltip) : this(labelText)
        {
            this.tooltip = tooltip;
        }

        public SettingLabelElement(string labelText, Color32 color) : this(labelText)
        {
            _label.WithColor(color);
        }

        public SettingLabelElement(string labelText, string tooltip, Color32 color) : this(labelText, tooltip)
        {
            _label.WithColor(color);
        }

        public SettingLabelElement(string labelText, string tooltip, Texture2D icon) : this(labelText, tooltip)
        {
            _icon = new VisualElement().WithMargin(2f).WithWidth(14f).WithHeight(13f);
            _icon.style.backgroundImage = icon;
            Insert(0, _icon);
            style.flexDirection = FlexDirection.Row;
            style.paddingLeft = 2f;
        }
    }
}
