using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [GridFlowExecNodeInfo("Create Grid", "Layout Graph/", 1000)]
    public class GridFlowExecNodeHandler_CreateGrid : GridFlowExecNodeHandler
    {
        public Vector2Int resolution = new Vector2Int(6, 5);

        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var graph = new GridFlowAbstractGraph();
            ExecutionState = new GridFlowExecNodeState_AbstractGraph(graph);

            int width = resolution.x;
            int height = resolution.y;
            var nodes = new GridFlowAbstractGraphNode[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var node = new GridFlowAbstractGraphNode();
                    node.Position = new Vector2(x, height - y - 1) * 120;
                    node.state.GridCoord = new IntVector2(x, y);
                    nodes[x, y] = node;

                    if (x > 0)
                    {
                        var srcNode = nodes[x - 1, y];
                        var dstNode = nodes[x, y];
                        graph.MakeLink(srcNode, dstNode);
                    }
                    if (y > 0)
                    {
                        var srcNode = nodes[x, y - 1];
                        var dstNode = nodes[x, y];
                        graph.MakeLink(srcNode, dstNode);
                    }

                    graph.AddNode(node);
                }
            }
            return GridFlowExecNodeHandlerResultType.Success;
        }
    }
}
