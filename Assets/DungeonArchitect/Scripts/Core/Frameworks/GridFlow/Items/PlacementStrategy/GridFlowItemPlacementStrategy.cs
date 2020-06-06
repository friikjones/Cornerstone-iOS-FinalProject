using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowItemPlacementStrategyContext
    {
        public GridFlowTilemap tilemap;
        public GridFlowTilemapDistanceField distanceField;
        public System.Random random;
    }

    public interface IGridFlowItemPlacementStrategy
    {
        bool PlaceItems(GridFlowItem item, GridFlowTilemapCell[] freeCells, GridFlowItemPlacementSettings settings, GridFlowItemPlacementStrategyContext context, ref int outFreeTileIndex, ref string errorMessage);
    }

    [System.Serializable]
    public enum GridFlowItemPlacementMethod
    {
        RandomTile,
        NearEdges,
        Script
    }

    [System.Serializable]
    public class GridFlowItemPlacementSettings
    {
        public GridFlowItemPlacementMethod placementMethod = GridFlowItemPlacementMethod.RandomTile;
        public bool avoidPlacingNextToDoors = true;
        public string placementScriptClass = "";
        public bool fallbackToRandomPlacement = true;

        public GridFlowItemPlacementSettings Clone()
        {
            var newObj = new GridFlowItemPlacementSettings();
            newObj.placementMethod = placementMethod;
            newObj.avoidPlacingNextToDoors = avoidPlacingNextToDoors;
            newObj.placementScriptClass = placementScriptClass;
            newObj.fallbackToRandomPlacement = fallbackToRandomPlacement;
            return newObj;
        }
    }

    public class GridFlowItemPlacementStrategyFactory
    {
        public static IGridFlowItemPlacementStrategy Create(GridFlowItemPlacementMethod method)
        {
            if (method == GridFlowItemPlacementMethod.NearEdges)
            {
                return new GridFlowItemPlacementStrategy_NearEdge();
            }
            else if (method == GridFlowItemPlacementMethod.Script)
            {
                return new GridFlowItemPlacementStrategy_Script();
            }
            else if (method == GridFlowItemPlacementMethod.RandomTile)
            {
                return new GridFlowItemPlacementStrategy_Random();
            }
            else
            {
                return new GridFlowItemPlacementStrategy_Random();
            }
        }
    }

    public class GridFlowItemPlacementStrategyUtils
    {
        public static bool Validate(GridFlowItemPlacementSettings settings, ref string errorMessage)
        {
            if (settings.placementMethod == GridFlowItemPlacementMethod.Script)
            {
                if (settings.placementScriptClass == null || settings.placementScriptClass.Length == 0)
                {
                    errorMessage = "Invalid script reference";
                    return false;
                }
            }
            return true;
        }
    }
}
