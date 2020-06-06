using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Graphs.Preview.Abstract;
using DungeonArchitect.UI;
using DungeonArchitect.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.UI.Widgets.GraphEditors;

namespace DungeonArchitect.Editors.GridFlow.Abstract
{
    public class GridFlowPreviewAbstractNodeRenderer : GridFlowPreviewAbstractNodeRendererBase
    {
        public event OnGridFlowAbstractGraphItemRendered GridFlowAbstractGraphItemRendered;
        protected override string GetCaption(GraphNode node)
        {
            var caption = "";
            var previewNode = node as GridFlowPreviewAbstractGraphNode;
            if (previewNode != null && previewNode.AbstractNodeState.Active)
            {
                switch (previewNode.AbstractNodeState.RoomType)
                {
                    case GridFlowAbstractNodeRoomType.Room:
                        caption = "R";
                        break;

                    case GridFlowAbstractNodeRoomType.Corridor:
                        caption = "Co";
                        break;

                    case GridFlowAbstractNodeRoomType.Cave:
                        caption = "Ca";
                        break;
                }
            }

            return caption;
        }

        GridFlowAbstractNodeState GetNodeState(GraphNode node)
        {
            var previewNode = node as GridFlowPreviewAbstractGraphNode;
            if (previewNode != null)
            {
                return previewNode.AbstractNodeState;
            }
            return null;
        }

        protected override Color GetBodyColor(GraphNode node)
        {
            var bodyColor = base.GetBodyColor(node);
            
            var nodeState = GetNodeState(node);
            if (nodeState != null && nodeState.Active)
            {
                bodyColor = nodeState.Color;
            }
            return bodyColor;
        }



        protected override void DrawNodeBody(UIRenderer renderer, GraphRendererContext rendererContext, GraphNode node, GraphCamera camera)
        {
            base.DrawNodeBody(renderer, rendererContext, node, camera);

            var nodeState = GetNodeState(node);
            if (nodeState != null)
            {
                int numItems = nodeState.Items.Count;
                if (numItems > 0)
                {
                    var hostPosScreen = camera.WorldToScreen(node.Position);
                    var hostSizeScreen = node.Bounds.size / camera.ZoomLevel;
                    var hostCenterScreen = hostPosScreen + hostSizeScreen * 0.5f;
                    var hostRadiusScreen = Mathf.Min(hostSizeScreen.x, hostSizeScreen.y) * 0.5f;

                    var itemRadiusScreen = hostRadiusScreen * GridFlowPreviewAbstractItemRenderer.ItemRadiusFactor;
                    var itemCenterDistance = hostRadiusScreen - itemRadiusScreen;

                    for (int i = 0; i < numItems; i++)
                    {
                        var item = nodeState.Items[i];
                        if (item == null) continue;
                        var angle = Mathf.PI * 2 * i / numItems - Mathf.PI * 0.5f;
                        var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        var itemCenterScreen = hostCenterScreen + direction * itemCenterDistance;
                        var itemBoundsSize = new Vector2(itemRadiusScreen, itemRadiusScreen) * 2;
                        var itemBounds = new Rect(itemCenterScreen - itemBoundsSize * 0.5f, itemBoundsSize);
                        
                        float scaleFactor = GridFlowPreviewAbstractItemRenderer.GetItemScaleFactor(rendererContext.GraphEditor.LastMousePosition, itemBounds);
                        var scaledSize = itemBounds.size * scaleFactor;
                        itemBounds = new Rect(itemBounds.center - scaledSize * 0.5f, scaledSize);

                        GridFlowPreviewAbstractItemRenderer.DrawItem(renderer, rendererContext, camera, item, itemBounds, scaleFactor);
                        if (GridFlowAbstractGraphItemRendered != null)
                        {
                            GridFlowAbstractGraphItemRendered.Invoke(item, itemBounds);
                        }
                    }
                }
            }
        }


    }
}
