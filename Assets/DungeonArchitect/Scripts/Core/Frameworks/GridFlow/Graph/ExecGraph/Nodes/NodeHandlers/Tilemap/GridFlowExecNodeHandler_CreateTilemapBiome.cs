using DungeonArchitect.Builders.GridFlow.Tilemap;
using DungeonArchitect.Utils.Noise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    /*
    [System.Serializable]
    public class GridFlowTilemapBackgroundLayer
    {
        [SerializeField]
        public GridFlowTilemapCustomCellInfo cellType = new GridFlowTilemapCustomCellInfo();

        public int noiseOctaves = 6;
        public float noiseFrequency = 0.01f;
        public float noiseThreshold = 0.5f;
        public float noiseValuePower = 1.0f;
    }

    [GridFlowExecNodeInfo("Create Tilemap Biome", "Tilemap/")]
    public class GridFlowExecNodeHandler_CreateTilemapBiome : GridFlowExecNodeHandler
    {
        [SerializeField]
        public GridFlowTilemapBackgroundLayer[] layers = new GridFlowTilemapBackgroundLayer[0];

        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, ref string errorMessage)
        {
            GridFlowTilemap tilemap = null;
            var incomingStates = GridFlowExecNodeUtils.GetIncomingStates(execNode);
            if (incomingStates.Length > 0)
            {
                var incomingState = incomingStates[0];
                if (incomingState.Tilemap != null)
                {
                    int width = incomingState.Tilemap.Width;
                    int height = incomingState.Tilemap.Height;
                    tilemap = new GridFlowTilemap(width, height);
                }
            }

            if (tilemap == null)
            {
                errorMessage = "Missing tilemap input";
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(execNode);
            ExecutionState = new GridFlowExecNodeState_Tilemap(tilemap, graph);

            foreach (var layer in layers)
            {
                var random = context.Random;
                var noiseTable = new GradientNoiseTable();
                noiseTable.Init(128, random);

                // Assign the valid traversable paths 
                for (int y = 0; y < tilemap.Height; y++)
                {
                    for (int x = 0; x < tilemap.Width; x++)
                    {
                        var cell = tilemap.Cells[x, y];
                        var position = new Vector2(x, y) * layer.noiseFrequency;
                        var n = noiseTable.GetNoiseFBM(position, layer.noiseOctaves);
                        n = Mathf.Pow(n, layer.noiseValuePower);
                        if (n > layer.noiseThreshold)
                        {
                            cell.CellType = GridFlowTilemapCellType.Custom;
                            cell.CustomCellInfo = layer.cellType;
                        }
                        //cell.UseCustomColor = true;
                        //cell.CustomColor = new Color(n, n, n);
                    }
                }
            }

            return GridFlowExecNodeHandlerResultType.Success;
        }
    }
    */
}
