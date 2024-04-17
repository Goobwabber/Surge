using UnityEngine;
using UnityEngine.UIElements;

namespace Surge.Editor.Elements
{
    /// <summary>
    /// Custom label entirely dedicated to fixing UI Toolkit tooltips
    /// </summary>
    internal class SurgeLabel : Label
    {
        public SurgeLabel(string text) : base(text)
        {
            
        }
        
        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);

            if (evt.eventTypeId != TooltipEvent.TypeId())
                return;

            var e = (TooltipEvent)evt;
            
            e.rect = new Rect(
                new Vector2(-100f, e.rect.y - 8f),
                e.rect.size
            );

            
            e.StopPropagation();
        }
    }
}