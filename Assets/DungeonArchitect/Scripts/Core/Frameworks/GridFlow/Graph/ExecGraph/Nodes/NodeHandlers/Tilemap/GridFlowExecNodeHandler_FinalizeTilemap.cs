using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{

    [GridFlowExecNodeInfo("Finalize Tilemap", "Tilemap/", 2500)]
    public class GridFlowExecNodeHandler_FinalizeTilemap : GridFlowExecNodeHandler
    {
        public bool debugUnwalkableCells = false;

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

            var assignStatus = AssignItems(tilemap, graph, context.Random, ref errorMessage);
            return assignStatus;
        }

        List<GridFlowTilemapCell> FilterWalkablePath(List<GridFlowTilemapCell> cells)
        {
            var unreachable = new HashSet<IntVector2>();
            var cellsByCoord = new Dictionary<IntVector2, GridFlowTilemapCell>();

            foreach(var cell in cells)
            {
                unreachable.Add(cell.TileCoord);
                cellsByCoord[cell.TileCoord] = cell;
            }

            var queue = new Queue<GridFlowTilemapCell>();
            foreach (var cell in cells)
            {
                if (cell.MainPath)
                {
                    unreachable.Remove(cell.TileCoord);
                    queue.Enqueue(cell);
                }
            }

            var childOffsets = new int[]
            {
                -1, 0,
                1, 0,
                0, -1,
                0, 1
            };

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var coord = cell.TileCoord;
                for (int i = 0; i < 4; i++)
                {
                    var cx = coord.x + childOffsets[i * 2 + 0];
                    var cy = coord.y + childOffsets[i * 2 + 1];
                    var childCoord = new IntVector2(cx, cy);
                    if (unreachable.Contains(childCoord))
                    {
                        var canTraverse = true;
                        var childCell = cellsByCoord[childCoord];
                        if (childCell.Overlay != null && childCell.Overlay.tileBlockingOverlay)
                        {
                            canTraverse = false;
                        }
                        if (canTraverse)
                        {
                            unreachable.Remove(childCoord);
                            queue.Enqueue(cellsByCoord[childCoord]);
                        }
                    }
                }
            }

            if (debugUnwalkableCells)
            {
                foreach (var unreachableCoord in unreachable)
                {
                    var invalidCell = cellsByCoord[unreachableCoord];
                    invalidCell.CustomColor = Color.red;
                    invalidCell.UseCustomColor = true;
                }
            }

            // Grab all the cells that are not in the unreachable list
            var result = new List<GridFlowTilemapCell>();

            foreach (var cell in cells)
            {
                if (!unreachable.Contains(cell.TileCoord))
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        GridFlowExecNodeHandlerResultType AssignItems(GridFlowTilemap tilemap, GridFlowAbstractGraph graph, System.Random random, ref string errorMessage)
        {
            var nodesByCoord = new Dictionary<IntVector2, GridFlowAbstractGraphNode>();
            var freeTilesByNode = new Dictionary<IntVector2, List<GridFlowTilemapCell>>();

            foreach (var node in graph.Nodes)
            {
                nodesByCoord[node.state.GridCoord] = node;
            }

            foreach (var cell in tilemap.Cells)
            {
                if (cell.CellType == GridFlowTilemapCellType.Floor)
                {
                    var nodeCoord = cell.NodeCoord;
                    if (!freeTilesByNode.ContainsKey(nodeCoord))
                    {
                        freeTilesByNode.Add(nodeCoord, new List<GridFlowTilemapCell>());
                    }
                    if (cell.Item == System.Guid.Empty)
                    {
                        freeTilesByNode[nodeCoord].Add(cell);
                    }
                }
            }

            // Filter walkable paths on the free tiles (some free tile patches may be blocked by overlays like tree lines)
            var nodeKeys = new List<IntVector2>(freeTilesByNode.Keys);
            foreach (var nodeCoord in nodeKeys)
            {
                freeTilesByNode[nodeCoord] = FilterWalkablePath(freeTilesByNode[nodeCoord]);
            }

            var distanceField = new GridFlowTilemapDistanceField(tilemap);
            // Add node items
            foreach (var node in graph.Nodes)
            {
                if (freeTilesByNode.ContainsKey(node.state.GridCoord))
                {
                    var freeTiles = freeTilesByNode[node.state.GridCoord];
                    foreach (var item in node.state.Items)
                    {
                        if (freeTiles.Count == 0)
                        {
                            errorMessage = "Item Placement failed. Insufficient free tiles";
                            return GridFlowExecNodeHandlerResultType.FailRetry;
                        }

                        var freeTileIndex = -1;
                        var context = new GridFlowItemPlacementStrategyContext();
                        context.tilemap = tilemap;
                        context.distanceField = distanceField;
                        context.random = random;

                        string placementErrorMessage = "";
                        if (item.placementSettings != null)
                        {
                            var placementStrategy = GridFlowItemPlacementStrategyFactory.Create(item.placementSettings.placementMethod);
                            var placementSuccess = false;
                            if (placementStrategy != null)
                            {
                                placementSuccess = placementStrategy.PlaceItems(item, freeTiles.ToArray(), item.placementSettings, context, ref freeTileIndex, ref placementErrorMessage);

                                // If we failed, try to fall back to random tile placement, if specified
                                if (!placementSuccess && item.placementSettings.fallbackToRandomPlacement)
                                {
                                    var randomPlacement = GridFlowItemPlacementStrategyFactory.Create(GridFlowItemPlacementMethod.RandomTile);
                                    placementSuccess = randomPlacement.PlaceItems(item, freeTiles.ToArray(), item.placementSettings, context, ref freeTileIndex, ref placementErrorMessage);
                                }
                            }

                            if (!placementSuccess)
                            {
                                errorMessage = "Item Placement failed. " + placementErrorMessage;
                                return GridFlowExecNodeHandlerResultType.FailRetry;
                            }
                            if (freeTileIndex < 0 || freeTileIndex >= freeTiles.Count)
                            {
                                errorMessage = "Item Placement failed. Invalid tile index";
                                return GridFlowExecNodeHandlerResultType.FailRetry;
                            }
                        }
                        else
                        {
                            freeTileIndex = random.Next(freeTiles.Count - 1);
                        }


                        var freeTile = freeTiles[freeTileIndex];
                        freeTile.Item = item.itemId;
                        freeTiles.Remove(freeTile);
                    }
                }
            }

            return GridFlowExecNodeHandlerResultType.Success;
        }
    }
}
