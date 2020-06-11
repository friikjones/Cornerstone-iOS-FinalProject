using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowItemPlacementStrategy_Script : IGridFlowItemPlacementStrategy
    {
        public bool PlaceItems(GridFlowItem item, GridFlowTilemapCell[] freeCells, GridFlowItemPlacementSettings settings,
                GridFlowItemPlacementStrategyContext context, ref int outFreeTileIndex, ref string errorMessage)
        {
            if (settings.placementScriptClass != null && settings.placementScriptClass.Length > 0)
            {
                var type = System.Type.GetType(settings.placementScriptClass);
                if (type != null)
                {
                    var script = ScriptableObject.CreateInstance(type) as IGridFlowItemPlacementStrategy;
                    if (script != null)
                    {
                        return script.PlaceItems(item, freeCells, settings, context, ref outFreeTileIndex, ref errorMessage);
                    }
                }
            }

            errorMessage = "Invalid script reference";
            return false;
        }

    }
}
