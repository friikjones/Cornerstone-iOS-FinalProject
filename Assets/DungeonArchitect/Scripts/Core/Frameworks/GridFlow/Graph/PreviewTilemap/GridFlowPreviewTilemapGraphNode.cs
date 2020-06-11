using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using DungeonArchitect.Graphs;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Preview.Tilemap
{
    public class GridFlowPreviewTilemapGraphNode : GraphNode
    {
        public int tileRenderSize = 12;
        public GridFlowTilemap Tilemap { get; private set; }
        public GridFlowAbstractGraph AbstractGraph { get; set; }
        public GridFlowAbstractNodeState SelectedNodeState { get; set; }
        public GridFlowItem SelectedItem { get; set; }
        public bool RequestRecreatePreview { get; set; }

        public GridFlowPreviewTilemapGraphNode()
        {
            RequestRecreatePreview = false;
        }

        public override void Initialize(string id, Graph graph)
        {
            base.Initialize(id, graph);
        }

        public void SetTilemap(GridFlowTilemap tilemap)
        {
            this.Tilemap = tilemap;

            // Update the bounds
            var size = new Vector2(tilemap.Width, tilemap.Height) * tileRenderSize;
            bounds = new Rect(Vector2.zero, size);
        }
    }
}
