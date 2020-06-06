using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [GridFlowExecNodeInfo("Optimize Tilemap", "Tilemap/", 2400)]
    public class GridFlowExecNodeHandler_OptimizeTilemap : GridFlowExecNodeHandler
    {
        public int discardDistanceFromLayout= 3;
        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode node, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var tilemap = GridFlowExecNodeUtils.CloneIncomingTilemap(node, context.NodeStates);
            if (tilemap == null)
            {
                errorMessage = "Missing tilemap input";
                ExecutionState = null;
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(node, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_Tilemap(tilemap, graph);

            DiscardDistantTiles(tilemap);

            return GridFlowExecNodeHandlerResultType.Success;
        }

        void DiscardDistantTiles(GridFlowTilemap tilemap)
        {
            var width = tilemap.Width;
            var height = tilemap.Height;
            var queue = new Queue<GridFlowTilemapCell>();


            var childOffsets = new int[]
            {
                -1, 0,
                1, 0,
                0, -1,
                0, 1
            };

            var distanceFromLayout = new Dictionary<GridFlowTilemapCell, int>();
            foreach (var cell in tilemap.Cells)
            {
                if (cell.LayoutCell)
                {
                    queue.Enqueue(cell);
                    distanceFromLayout[cell] = 0;
                }
            }
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();

                // Traverse the children
                var childDistance = distanceFromLayout[cell] + 1;
                for (int i = 0; i < 4; i++)
                {
                    int nx = cell.TileCoord.x + childOffsets[i * 2 + 0];
                    int ny = cell.TileCoord.y + childOffsets[i * 2 + 1];
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var ncell = tilemap.Cells[nx, ny];
                        if (ncell.LayoutCell) continue;
                        if (!distanceFromLayout.ContainsKey(ncell) || childDistance < distanceFromLayout[ncell])
                        {
                            distanceFromLayout[ncell] = childDistance;
                            queue.Enqueue(ncell);
                        }
                    }
                }
            }
            discardDistanceFromLayout = Mathf.Max(0, discardDistanceFromLayout);
            foreach (var cell in tilemap.Cells)
            {
                if (!distanceFromLayout.ContainsKey(cell)) continue;
                if (cell.LayoutCell) continue;
                if (distanceFromLayout[cell] > discardDistanceFromLayout)
                {
                    cell.Clear();
                }
            }
        }
    }
}
