using Surge.Editor.Extensions;
using Surge.Editor.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Surge.Editor.Elements
{
    internal class FloatSliderField : FloatField, ISurgeBindable
    {
        private readonly bool _allowOverflow;
        private readonly float _minSliderValue;
        private readonly float _maxSliderValue;
        private readonly Slider _slider;

        private SerializedProperty? _property;

        public FloatSliderField(string label, float min = 0f, float max = 10f, bool allowOverflow = false) : base(label)
        {
            _minSliderValue = min;
            _maxSliderValue = max;
            _allowOverflow = allowOverflow;

            _slider = new Slider(min, max).WithHeight(16f).WithGrow(3f).WithMargin(0f);
            _slider.style.marginRight = 4f;
            _slider.style.marginLeft = 1f;
            _slider.style.marginBottom = 1f;
            this.Insert(1, _slider);
            this[2].WithWidth(60f).WithGrow(1f);

            this.RegisterValueChangedCallback(ctx =>
            {
                if (ctx.newValue < _minSliderValue)
                    this.value = _minSliderValue;
                if (!_allowOverflow && ctx.newValue > _maxSliderValue)
                    this.value = _maxSliderValue;
                HandleFloatFieldValue(this.value);
                var clampedValue = ctx.newValue <= 10f ? ctx.newValue : 10f;
                if (_slider.value != clampedValue)
                    _slider.value = clampedValue;
            });

            _slider.RegisterValueChangedCallback(ctx => 
            {
                var clampedValue = _property.floatValue <= 10f ? _property.floatValue : 10f;
                if (_property is null || clampedValue == ctx.newValue)
                    return;
                this.value = ctx.newValue;
            });
        }

        public void SetBinding(SerializedProperty property)
        {
            _property = property;
            this.BindProperty(property);

            if (property?.floatValue is null)
                return;
            HandleFloatFieldValue(property.floatValue);
        }

        private void HandleFloatFieldValue(float newValue)
        {
            if (!_allowOverflow)
                return;
            var sliderEnabled = newValue <= _maxSliderValue;
            _slider.SetEnabled(sliderEnabled);
        }
    }
}
