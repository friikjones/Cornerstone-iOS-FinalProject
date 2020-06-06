using DungeonArchitect.Builders.GridFlow.Tilemap;
using DungeonArchitect.Utils.Noise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    public enum GridFlowExecNodeHandler_CreateTilemapOverlayGenMethod
    {
        Noise,
        Script
    }

    [System.Serializable]
    public class GridFlowExecNodeTilemapOverlayNoiseSettings
    {
        public int noiseOctaves = 4;
        public float noiseFrequency = 0.15f;
        public float noiseValuePower = 0;
        public float noiseMinValue = 0;
        public float noiseMaxValue = 1.0f;
        public float noiseThreshold = 0.5f;
        public int minDistFromMainPath = 2;
    }

    public interface IGridFlowTilemapOverlayGenerator
    {
        void Init(System.Random random);
        bool Generate(GridFlowTilemapCell cell, GridFlowTilemapCell incomingCell, System.Random random, ref float overlayValue);
    }

    public class NoiseGridFlowTilemapOverlayGenerator : IGridFlowTilemapOverlayGenerator
    {
        GradientNoiseTable noiseTable;
        GridFlowExecNodeTilemapOverlayNoiseSettings noiseSettings;
        public NoiseGridFlowTilemapOverlayGenerator(GridFlowExecNodeTilemapOverlayNoiseSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;
        }

        public void Init(System.Random random)
        {
            noiseTable = new GradientNoiseTable();
            noiseTable.Init(128, random);

            noiseSettings.minDistFromMainPath = Mathf.Max(1, noiseSettings.minDistFromMainPath);
        }

        public bool Generate(GridFlowTilemapCell cell, GridFlowTilemapCell incomingCell, System.Random random, ref float overlayValue)
        {
            var cellCoord = incomingCell.TileCoord;
            var position = cellCoord.ToVector2() * noiseSettings.noiseFrequency;
            var n = noiseTable.GetNoiseFBM(position, noiseSettings.noiseOctaves);
            if (noiseSettings.noiseValuePower > 0.0f)
            {
                n = Mathf.Pow(n, noiseSettings.noiseValuePower);
            }

            n = noiseSettings.noiseMinValue + (noiseSettings.noiseMaxValue - noiseSettings.noiseMinValue) * n;

            if (n > noiseSettings.noiseThreshold)
            {
                var distanceFromMainPath = incomingCell.DistanceFromMainPath;
                float noiseFactor = (n - noiseSettings.noiseThreshold) / (1.0f - noiseSettings.noiseThreshold);
                bool insertOverlay = (noiseFactor * distanceFromMainPath > noiseSettings.minDistFromMainPath);

                if (insertOverlay)
                {
                    overlayValue = n;
                    return true;
                }
            }
            return false;
        }
    }

    [GridFlowExecNodeInfo("Create Tilemap Overlay", "Tilemap/", 2200)]
    public class GridFlowExecNodeHandler_CreateTilemapOverlay : GridFlowExecNodeHandler
    {
        public string markerName = "Tree";
        public Color color = Color.green;
        public bool overlayBlocksTile = true;
        public GridFlowExecNodeHandler_CreateTilemapOverlayGenMethod generationMethod = GridFlowExecNodeHandler_CreateTilemapOverlayGenMethod.Noise;
        public GridFlowExecNodeTilemapOverlayNoiseSettings noiseSettings = new GridFlowExecNodeTilemapOverlayNoiseSettings();
        public GridFlowTilemapCellOverlayMergeConfig mergeConfig = new GridFlowTilemapCellOverlayMergeConfig();
        public string generatorScriptClass;

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
            var generator = createGeneratorInstance();
            if (generator == null)
            {
                errorMessage = "Invalid script reference";
                ExecutionState = null;
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }
            generator.Init(random);

            // Create overlays
            for (int y = 0; y < tilemap.Height; y++)
            {
                for (int x = 0; x < tilemap.Width; x++)
                {
                    var incomingCell = incomingTilemap.Cells[x, y];
                    var cell = tilemap.Cells[x, y];

                    float overlayValue = 0;
                    if (generator.Generate(cell, incomingCell, random, ref overlayValue))
                    {
                        var overlay = new GridFlowTilemapCellOverlay();
                        overlay.markerName = markerName;
                        overlay.color = color;
                        overlay.noiseValue = overlayValue;
                        overlay.mergeConfig = mergeConfig;
                        overlay.tileBlockingOverlay = overlayBlocksTile;

                        cell.Overlay = overlay;
                    }
                }
            }

            return GridFlowExecNodeHandlerResultType.Success;
        }

        IGridFlowTilemapOverlayGenerator createGeneratorInstance()
        {
            IGridFlowTilemapOverlayGenerator generator = null;
            if (generationMethod == GridFlowExecNodeHandler_CreateTilemapOverlayGenMethod.Noise)
            {
                generator = new NoiseGridFlowTilemapOverlayGenerator(noiseSettings);
            }
            else if (generationMethod == GridFlowExecNodeHandler_CreateTilemapOverlayGenMethod.Script)
            {
                if (generatorScriptClass != null)
                {
                    var type = System.Type.GetType(generatorScriptClass);
                    if (type != null)
                    {
                        generator = ScriptableObject.CreateInstance(type) as IGridFlowTilemapOverlayGenerator;
                    }
                }
            }
            return generator;
        }

    }
}
