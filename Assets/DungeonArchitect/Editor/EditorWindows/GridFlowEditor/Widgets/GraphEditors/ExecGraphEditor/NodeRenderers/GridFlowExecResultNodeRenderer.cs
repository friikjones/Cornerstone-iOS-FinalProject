using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec;

namespace DungeonArchitect.Editors.GridFlow
{
    public class GridFlowExecResultNodeRenderer : GridFlowExecNodeRendererBase
    {
        protected override string GetCaption(GraphNode node)
        {
            return "Result";
        }

        protected override Color GetPinColor(GraphNode node)
        {
            return new Color(0.1f, 0.4f, 0.2f);
        }

        protected override Color GetBodyColor(GraphNode node)
        {
            return new Color(0.1f, 0.1f, 0.1f, 1);
        }
    }
}
