using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.UI.Widgets
{
    public class DebugWidget : WidgetBase
    {
        Color debugColor = Color.red;
        string caption = "Panel";

        public DebugWidget() : this("", new Color(0.1f, 0.1f, 0.1f))
        {
        }
        public DebugWidget(string caption, Color color)
        {
            this.debugColor = color;
            this.caption = caption;
            ShowFocusHighlight = true;
        }

        public override void Draw(UISystem uiSystem, UIRenderer renderer)
        {
            var bounds = new Rect(Vector2.zero, WidgetBounds.size);
            renderer.Box(bounds, new GUIContent(caption));

            renderer.DrawRect(bounds, debugColor);
        }

    }
}
