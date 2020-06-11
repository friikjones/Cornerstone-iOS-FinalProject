using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Preview.Abstract
{
    public class GridFlowPreviewAbstractGraph : Graph
    {
        public override void OnEnable()
        {
            base.OnEnable();

            hideFlags = HideFlags.HideInHierarchy;
        }
    }
}
