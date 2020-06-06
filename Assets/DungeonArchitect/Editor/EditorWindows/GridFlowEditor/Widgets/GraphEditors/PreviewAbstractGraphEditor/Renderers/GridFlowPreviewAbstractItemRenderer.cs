using DungeonArchitect.Builders.GridFlow;
using DungeonArchitect.UI;
using DungeonArchitect.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DungeonArchitect.UI.Widgets.GraphEditors;

namespace DungeonArchitect.Editors.GridFlow.Abstract
{
    public delegate void OnGridFlowAbstractGraphItemRendered(GridFlowItem item, Rect screenBounds);
    public class GridFlowPreviewAbstractItemRenderer 
    {
        public static float ItemRadiusFactor { get; private set; }
        public static float ItemHoverScaleFactor { get; private set; }

        static GridFlowPreviewAbstractItemRenderer()
        {
            ItemRadiusFactor = 0.35f;
            ItemHoverScaleFactor = 1.25f;
        }

        public static float GetItemScaleFactor(Vector2 mousePosition, Rect itemBounds)
        {
            var radius = itemBounds.size.x * 0.5f;
            var itemCenter = itemBounds.center;
            var distance = (mousePosition - itemCenter).magnitude - radius;
            distance = 1 - Mathf.Clamp01(distance / (itemBounds.size.x * 0.25f));
            distance = distance * distance;

            var scaleFactor = Mathf.Lerp(1.0f, ItemHoverScaleFactor, distance);
            return scaleFactor;
        }

        public static void DrawItem(UIRenderer renderer, GraphRendererContext rendererContext, GraphCamera camera, GridFlowItem item, Rect itemBounds, float textScaleFactor)
        {
            // Draw the item background circle
            Color colorBackground, colorForeground;
            GetAbstractItemColor(item, out colorBackground, out colorForeground);
            var borderColor = item.editorSelected ? Color.red : Color.black;
            var thickness = item.editorSelected ? 3 : 1;
            GridFlowPreviewAbstractNodeRendererBase.DrawCircle(renderer, itemBounds, colorBackground, borderColor, thickness);

            // Draw the item text
            {
                var text = GetAbstractItemText(item);
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = colorForeground;
                style.alignment = TextAnchor.MiddleCenter;
                style.font = EditorStyles.boldFont;
                float scaledFontSize = (style.fontSize == 0) ? style.font.fontSize : style.fontSize;
                scaledFontSize *= textScaleFactor;
                scaledFontSize = Mathf.Max(1.0f, scaledFontSize / camera.ZoomLevel);
                style.fontSize = Mathf.RoundToInt(scaledFontSize);
                renderer.Label(itemBounds, text, style);
            }
        }

        private static string GetAbstractItemText(GridFlowItem item)
        {
            switch (item.type)
            {
                case GridFlowGraphItemType.Entrace:
                    return "S";

                case GridFlowGraphItemType.Exit:
                    return "G";

                case GridFlowGraphItemType.Enemy:
                    return "E";

                case GridFlowGraphItemType.Key:
                    return "K";

                case GridFlowGraphItemType.Lock:
                    return "L";

                case GridFlowGraphItemType.Bonus:
                    return "B";

                case GridFlowGraphItemType.Custom:
                    return item.customInfo.text;

                default:
                    return "";
            }
        }

        private static void GetAbstractItemColor(GridFlowItem item, out Color colorBackground, out Color colorText)
        {
            switch (item.type)
            {
                case GridFlowGraphItemType.Entrace:
                    colorBackground = new Color(0, 0.3f, 0);
                    colorText = Color.white;
                    break;

                case GridFlowGraphItemType.Exit:
                    colorBackground = new Color(0, 0, 0.3f);
                    colorText = Color.white;
                    break;

                case GridFlowGraphItemType.Enemy:
                    colorBackground = new Color(0.6f, 0, 0);
                    colorText = Color.white;
                    break;

                case GridFlowGraphItemType.Key:
                    colorBackground = Color.yellow;
                    colorText = Color.black;
                    break;

                case GridFlowGraphItemType.Lock:
                    colorBackground = Color.blue;
                    colorText = Color.white;
                    break;

                case GridFlowGraphItemType.Bonus:
                    colorBackground = new Color(0, 0.5f, 1);
                    colorText = Color.white;
                    break;

                case GridFlowGraphItemType.Custom:
                    colorBackground = item.customInfo.backgroundColor;
                    colorText = item.customInfo.textColor;
                    break;

                default:
                    colorBackground = Color.white;
                    colorText = Color.black;
                    break;
            }

        }
    }
}
