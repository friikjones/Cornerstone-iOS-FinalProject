using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using DungeonArchitect.Graphs;
using DungeonArchitect.UI;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Preview.Tilemap
{
    public struct GridFlowPreviewTilemapBuildContext
    {
        public GridFlowTilemap tilemap;
        public GridFlowAbstractGraph abstractGraph;
        public GraphBuilder graphBuilder;

        public GridFlowAbstractNodeState selectedNodeState;
        public GridFlowItem selectedItem;
    }

    public class GridFlowPreviewTilemapGraphBuilder
    {
        public static void Build(GridFlowPreviewTilemapBuildContext context)
        {
            if (context.graphBuilder == null)
            {
                return;
            }

            context.graphBuilder.DestroyAllNodes(null);

            if (context.tilemap == null)
            {
                return;
            }

            var previewNode = context.graphBuilder.CreateNode<GridFlowPreviewTilemapGraphNode>(null);
            if (previewNode != null)
            {
                previewNode.SetTilemap(context.tilemap);
                previewNode.AbstractGraph = context.abstractGraph;
                previewNode.SelectedNodeState = context.selectedNodeState;
                previewNode.SelectedItem = context.selectedItem;
            }

        }
    }
}
