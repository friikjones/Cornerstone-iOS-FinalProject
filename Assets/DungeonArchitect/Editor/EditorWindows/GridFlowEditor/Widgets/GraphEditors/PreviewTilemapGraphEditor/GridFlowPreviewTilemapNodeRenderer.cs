using DungeonArchitect.Builders.GridFlow.Graphs.Preview.Tilemap;
using DungeonArchitect.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonArchitect.Builders.GridFlow;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using DungeonArchitect.Editors.GridFlow.Abstract;
using DungeonArchitect.Utils;
using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets.GraphEditors;

namespace DungeonArchitect.Editors.GridFlow.Tilemap
{
    public class GridFlowPreviewTilemapNodeRenderer : GraphNodeRenderer
    {
        string tilemapNodeId;
        RenderTexture tilemapTexture;
        public event OnGridFlowAbstractGraphItemRendered GridFlowAbstractGraphItemRendered;


        private bool IsCellSelected(GridFlowTilemapCell cell, GridFlowPreviewTilemapGraphNode node)
        {
            if (node == null || node.SelectedNodeState == null || cell == null) return false;

            return node.SelectedNodeState.GridCoord.Equals(cell.NodeCoord);
        }


        private void RecreateTilemapTexture(GridFlowTilemapRenderResources resources, GridFlowPreviewTilemapGraphNode node)
        {
            if (tilemapTexture != null) {
                tilemapTexture.Release();
                tilemapTexture = null;
            }

            node.RequestRecreatePreview = false;
            tilemapNodeId = node.Id;
            var tileSize = node.tileRenderSize;
            var tilemap = node.Tilemap;
            var textureSize = new IntVector2(tilemap.Width, tilemap.Height) * tileSize;
            tilemapTexture = new RenderTexture(textureSize.x, textureSize.y, 0);
            node.Bounds = new Rect(0, 0, textureSize.x, textureSize.y);

            GridFlowTilemapRenderer.Render(tilemapTexture, tilemap, tileSize, resources, cell => IsCellSelected(cell, node));
        }

        public override void Draw(UIRenderer renderer, GraphRendererContext rendererContext, GraphNode node, GraphCamera camera)
        {
            var guiState = new GUIState(renderer);

            var tilemapNode = node as GridFlowPreviewTilemapGraphNode;
            if (tilemapNode == null) return;

            if (tilemapTexture == null || tilemapNode.Id != tilemapNodeId || tilemapNode.RequestRecreatePreview)
            {
                var tileShader = Shader.Find("DungeonArchitect/UnlitColorShader");
                var resources = new GridFlowTilemapRenderResources();
                resources.materials = new TexturedMaterialInstances(tileShader);
                resources.iconOneWayDoor = renderer.GetResource<Texture2D>(UIResourceLookup.ICON_MOVEUP_16x) as Texture2D;
                RecreateTilemapTexture(resources, tilemapNode);
            }

            if (tilemapTexture == null)
            {
                return;
            }

            // Draw the tilemap texture
            {
                Rect tileBounds;
                var positionScreen = camera.WorldToScreen(node.Position);
                var sizeScreen = node.Size / camera.ZoomLevel;
                tileBounds = new Rect(positionScreen, sizeScreen);
                renderer.color = Color.white;
                renderer.DrawTexture(tileBounds, tilemapTexture);
            }

            // Draw the items
            {
                DrawItems(renderer, rendererContext, tilemapNode, camera);
            }

            guiState.Restore();
        }

        void DrawItems(UIRenderer renderer, GraphRendererContext rendererContext, GridFlowPreviewTilemapGraphNode node, GraphCamera camera)
        {
            if (node == null || node.AbstractGraph == null || node.Tilemap == null) return;
            var offset = node.Position;
            var tilemap = node.Tilemap;
            var abstractGraph = node.AbstractGraph;

            var items = abstractGraph.GetAllItems();
            var itemMap = new Dictionary<System.Guid, GridFlowItem>();
            foreach (var item in items)
            {
                itemMap[item.itemId] = item;
            }

            foreach (var cell in tilemap.Cells)
            {
                if (cell.Item == System.Guid.Empty) continue;
                var item = itemMap.ContainsKey(cell.Item) ? itemMap[cell.Item] : null;
                if (item == null) continue;

                var tileCoord = cell.TileCoord;
                tileCoord.y = tilemap.Height - tileCoord.y - 1;
                var position = offset + tileCoord.ToVector2() * node.tileRenderSize;
                var size = new Vector2(node.tileRenderSize, node.tileRenderSize);

                position = camera.WorldToScreen(position);
                size /= camera.ZoomLevel;

                var itemBounds = new Rect(position, size);

                float scaleFactor = GridFlowPreviewAbstractItemRenderer.GetItemScaleFactor(rendererContext.GraphEditor.LastMousePosition, itemBounds);
                var scaledSize = itemBounds.size * scaleFactor;
                itemBounds = new Rect(itemBounds.center - scaledSize * 0.5f, scaledSize);

                float preferedNodeSizeForText = 14.0f;
                float textScaleFactor = node.tileRenderSize / preferedNodeSizeForText;
                GridFlowPreviewAbstractItemRenderer.DrawItem(renderer, rendererContext, camera, item, itemBounds, scaleFactor * textScaleFactor);
                if (GridFlowAbstractGraphItemRendered != null)
                {
                    GridFlowAbstractGraphItemRendered.Invoke(item, itemBounds);
                }
            }
        }

        public override void Release()
        {
            base.Release();
            if (tilemapTexture != null)
            {
                tilemapTexture.Release();
                tilemapTexture = null;
            }
        }
    }
}
