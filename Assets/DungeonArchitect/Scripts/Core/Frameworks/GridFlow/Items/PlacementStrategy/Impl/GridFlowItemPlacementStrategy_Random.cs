using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowItemPlacementStrategy_Random : IGridFlowItemPlacementStrategy
    {
        public bool PlaceItems(GridFlowItem item, GridFlowTilemapCell[] freeCells, GridFlowItemPlacementSettings settings, GridFlowItemPlacementStrategyContext context, ref int outFreeTileIndex, ref string errorMessage)
        {
            var freeCellIndexRef = new List<int>();
            for (int i = 0; i < freeCells.Length; i++) 
            {
                var freeCell = freeCells[i];
                var x = freeCell.TileCoord.x;
                var y = freeCell.TileCoord.y;
                var distanceCell = context.distanceField.distanceCells[x, y];
                if (!settings.avoidPlacingNextToDoors || distanceCell.DistanceFromDoor > 1)
                {
                    freeCellIndexRef.Add(i);
                }
            }

            if (freeCellIndexRef.Count == 0)
            {
                // Not enough free cells for placing the items
                errorMessage = "Insufficient free tiles";
                return false;
            }

            var freeCellTableIndex = context.random.Next(freeCellIndexRef.Count - 1);
            outFreeTileIndex = freeCellIndexRef[freeCellTableIndex];
            return true;
        }
    }
}
