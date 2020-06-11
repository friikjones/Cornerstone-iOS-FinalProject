using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{

    [GridFlowExecNodeInfo("Merge Tilemaps", "Tilemap/", 2300)]
    public class GridFlowExecNodeHandler_MergeTilemaps : GridFlowExecNodeHandler
    {
        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            GridFlowTilemap tilemap = null;

            var incomingTilemaps = new List<GridFlowTilemap>();
            var incomingStates = GridFlowExecNodeUtils.GetIncomingStates(execNode, context.NodeStates);
            foreach (var incomingState in incomingStates)
            {

                if (incomingState.Tilemap != null)
                {
                    incomingTilemaps.Add(incomingState.Tilemap);
                    if (tilemap == null)
                    {
                        int width = incomingState.Tilemap.Width;
                        int height = incomingState.Tilemap.Height;
                        tilemap = new GridFlowTilemap(width, height);
                    }
                }
            }

            if (tilemap == null || incomingTilemaps.Count == 0)
            {
                errorMessage = "Missing tilemap input";
                ExecutionState = null;
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(execNode, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_Tilemap(tilemap, graph);

            // Assign the valid traversable paths 
            for (int y = 0; y < tilemap.Height; y++)
            {
                for (int x = 0; x < tilemap.Width; x++)
                {
                    int bestWeight = 0;
                    GridFlowTilemapCell bestCell = null;
                    var incomingOverlays = new List<GridFlowTilemapCellOverlay>();
                    foreach (var incomingTilemap in incomingTilemaps)
                    {
                        var weight = 0;
                        var incomingCell = incomingTilemap.Cells[x, y];
                        if (incomingCell.CellType == GridFlowTilemapCellType.Empty)
                        {
                            weight = 1;
                        }
                        else if (incomingCell.CellType == GridFlowTilemapCellType.Custom)
                        {
                            weight = 2;
                        }
                        else
                        {
                            weight = 3;
                        }

                        if (incomingCell.Overlay != null)
                        {
                            incomingOverlays.Add(incomingCell.Overlay);
                        }

                        bool useResult = false;
                        if (weight > bestWeight)
                        {
                            useResult = true;
                        }
                        else if (weight == bestWeight)
                        {
                            if (bestCell != null && incomingCell.Height > bestCell.Height)
                            {
                                useResult = true;
                            }
                        }

                        if (useResult)
                        {
                            bestCell = incomingCell;
                            bestWeight = weight;
                        }
                    }
                    
                    tilemap.Cells[x, y] = bestCell.Clone();
                    var resultCell = tilemap.Cells[x, y];
                    GridFlowTilemapCellOverlay bestOverlay = null;
                    float bestOverlayWeight = 0;
                    foreach (var incomingOverlay in incomingOverlays)
                    {
                        var valid = resultCell.Height >= incomingOverlay.mergeConfig.minHeight
                                   && resultCell.Height <= incomingOverlay.mergeConfig.maxHeight;

                        if (valid)
                        {
                            if (bestOverlay == null || incomingOverlay.noiseValue > bestOverlayWeight)
                            {
                                bestOverlay = incomingOverlay;
                                bestOverlayWeight = incomingOverlay.noiseValue;
                            }
                        }
                    }
                    if (bestOverlay != null)
                    {
                        resultCell.Overlay = bestOverlay.Clone();
                    }
                }
            }

            foreach (var cell in tilemap.Cells)
            {
                if (cell.CellType == GridFlowTilemapCellType.Wall && cell.Overlay != null)
                {
                    if (cell.Overlay.mergeConfig != null)
                    {
                        var wallOverlayRule = cell.Overlay.mergeConfig.wallOverlayRule;
                        if (wallOverlayRule == GridFlowTilemapCellOverlayMergeWallOverlayRule.KeepOverlayRemoveWall)
                        {
                            cell.CellType = GridFlowTilemapCellType.Floor;
                            cell.UseCustomColor = true;
                        }
                        else if (wallOverlayRule == GridFlowTilemapCellOverlayMergeWallOverlayRule.KeepWallRemoveOverlay)
                        {
                            cell.Overlay = null;
                        }
                    }
                }
            }

            return GridFlowExecNodeHandlerResultType.Success;
        }
    }

}
