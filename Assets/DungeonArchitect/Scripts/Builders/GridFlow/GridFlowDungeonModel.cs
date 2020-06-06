using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowDungeonModel : DungeonModel
    {
        /// <summary>
        /// The high level node based layout graph
        /// </summary>
        [HideInInspector]
        public GridFlowAbstractGraph abstractGraph;

        /// <summary>
        /// Rasterized tilemap representation of the abstract graph
        /// </summary>
        [HideInInspector]
        public GridFlowTilemap tilemap;
    }
}
