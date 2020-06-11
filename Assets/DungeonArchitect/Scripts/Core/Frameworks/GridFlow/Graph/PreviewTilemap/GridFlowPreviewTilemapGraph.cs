using DungeonArchitect.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Preview.Tilemap
{
    public class GridFlowPreviewTilemapGraph : Graph
    {
        public override void OnEnable()
        {
            base.OnEnable();

            hideFlags = HideFlags.HideInHierarchy;
        }

    }
}
