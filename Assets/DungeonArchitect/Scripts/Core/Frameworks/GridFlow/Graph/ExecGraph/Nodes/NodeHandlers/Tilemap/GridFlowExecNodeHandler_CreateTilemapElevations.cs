using DungeonArchitect.Builders.GridFlow.Tilemap;
using DungeonArchitect.Utils.Noise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{

    [GridFlowExecNodeInfo("Create Tilemap Elevations", "Tilemap/", 2100)]
    public class GridFlowExecNodeHandler_CreateTilemapElevations : GridFlowExecNodeHandler
    {
        public string markerName = "Rock";

        public int noiseOctaves = 4;
        public float noiseFrequency = 0.01f;
        public float noiseValuePower = 0;
        public int numSteps = 4;

        public float minHeight = -20;
        public float maxHeight = -5;
        public float seaLevel = -10;

        public Color landColor = new Color(0.4f, 0.2f, 0);
        public Color seaColor = new Color(0, 0, 0.4f);
        public float minColorMultiplier = 0.1f;

        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            GridFlowTilemap tilemap = null;
            GridFlowTilemap incomingTilemap = null;
            var incomingStates = GridFlowExecNodeUtils.GetIncomingStates(execNode, context.NodeStates);
            if (incomingStates.Length > 0)
            {
                var incomingState = incomingStates[0];
                if (incomingState.Tilemap != null)
                {
                    incomingTilemap = incomingState.Tilemap;
                    int width = incomingTilemap.Width;
                    int height = incomingTilemap.Height;
                    tilemap = new GridFlowTilemap(width, height);
                }
            }

            if (incomingTilemap == null)
            {
                errorMessage = "Missing tilemap input";
                ExecutionState = null;
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(execNode, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_Tilemap(tilemap, graph);

            var random = context.Random;
            var noiseTable = new GradientNoiseTable();
            noiseTable.Init(128, random);

            // Assign the valid traversable paths 
            for (int y = 0; y < tilemap.Height; y++)
            {
                for (int x = 0; x < tilemap.Width; x++)
                {
                    var incomingCell = incomingTilemap.Cells[x, y];
                    var cell = tilemap.Cells[x, y];
                    float cellHeight = 0;
                    if (incomingCell.CellType == GridFlowTilemapCellType.Empty)
                    {
                        var position = new Vector2(x, y) * noiseFrequency;
                        var n = noiseTable.GetNoiseFBM(position, noiseOctaves);
                        if (noiseValuePower > 1e-6f)
                        {
                            n = Mathf.Pow(n, noiseValuePower);
                        }
                        n = Mathf.Floor(n * numSteps) / numSteps;
                        cellHeight = minHeight + n * (maxHeight - minHeight);
                    }

                    cell.CellType = GridFlowTilemapCellType.Custom;
                    cell.CustomCellInfo = new GridFlowTilemapCustomCellInfo();
                    cell.CustomCellInfo.name = markerName;
                    cell.Height = cellHeight;
                    var color = (cell.Height <= seaLevel) ? seaColor : landColor;
                    var minColor = color * minColorMultiplier;
                    var colorBrightness = 1.0f;
                    if (Mathf.Abs(maxHeight - minHeight) > 1e-6f)
                    {
                        colorBrightness = (cell.Height - minHeight) / (maxHeight - minHeight);
                    }
                    cell.CustomCellInfo.defaultColor = Color.Lerp(minColor, color, colorBrightness);
                }
            }

            return GridFlowExecNodeHandlerResultType.Success;
        }
    }
}
