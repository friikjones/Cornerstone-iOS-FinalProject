using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [GridFlowExecNodeInfo("Result", "", 3000)]
    public class GridFlowExecNodeHandler_Result : GridFlowExecNodeHandler
    {
        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode node, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var tilemap = GridFlowExecNodeUtils.CloneIncomingTilemap(node, context.NodeStates);
            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(node, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_Tilemap(tilemap, graph);

            return GridFlowExecNodeHandlerResultType.Success;
        }
    }
}
