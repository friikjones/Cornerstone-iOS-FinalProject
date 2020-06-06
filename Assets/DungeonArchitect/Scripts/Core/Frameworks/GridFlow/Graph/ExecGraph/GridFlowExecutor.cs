using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers;
using DungeonArchitect.Graphs;
using DungeonArchitect.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowExecutionContext
    {
        public System.Random Random { get; set; }
        public GridFlowExecGraph ExecGraph { get; set; }
        public GridFlowExecNodeStates NodeStates { get; set; }
        public HashSet<GridFlowExecRuleGraphNode> Visited { get; private set; }

        public GridFlowExecutionContext()
        {
            Visited = new HashSet<GridFlowExecRuleGraphNode>();
        }
    }

    public class GridFlowExecutor
    {
        public bool Execute(GridFlowExecGraph execGraph, System.Random random, int numTries, out GridFlowExecNodeStates nodeStates)
        {
            if (execGraph == null || random == null)
            {
                Debug.LogError("Invalid asset state");
                nodeStates = null;
                return false;
            }

            if (execGraph.resultNode == null)
            {
                Debug.LogError("Cannot find result node in Execution Graph");
                nodeStates = null;
                return false;
            }

            int tries = 0;
            GridFlowExecNodeHandlerResultType lastRunStatus = GridFlowExecNodeHandlerResultType.FailHalt;
            while (tries < numTries) {
                tries++;

                var context = new GridFlowExecutionContext();
                context.ExecGraph = execGraph;
                context.Random = random;
                context.NodeStates = new GridFlowExecNodeStates(); 

                lastRunStatus = ExecuteGraph(context);
                if (lastRunStatus == GridFlowExecNodeHandlerResultType.Success)
                {
                    //if (tries > 1) {
                    //    Debug.Log("Num Tries: " + tries);
                    //}
                    nodeStates = context.NodeStates;
                    return true;
                }
                else if (lastRunStatus == GridFlowExecNodeHandlerResultType.FailHalt)
                {
                    break;
                }
            }

            nodeStates = null;
            return false;
        }

        private GridFlowExecNodeHandlerResultType ExecuteGraph(GridFlowExecutionContext context)
        {
            foreach (var node in context.ExecGraph.Nodes)
            {
                var execNode = node as GridFlowExecRuleGraphNode;
                if (execNode != null)
                {
                    execNode.executionStatus = new GridFlowGraphNodeExecutionStatus();
                }
            }
            return ExecuteNode(context, context.ExecGraph.resultNode);
        }

        private GridFlowExecNodeHandlerResultType ExecuteNode(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode)
        {
            context.Visited.Add(execNode);

            execNode.executionStatus.ExecutionStage = GridFlowGraphNodeExecutionStage.WaitingToExecute;

            var incomingNodes = GridFlowExecGraphUtils.GetIncomingNodes(execNode);
            foreach (var incomingNode in incomingNodes)
            {
                if (!context.Visited.Contains(incomingNode))
                {
                    var status = ExecuteNode(context, incomingNode);
                    if (status != GridFlowExecNodeHandlerResultType.Success)
                    {
                        return status;
                    }
                }
            }

            GridFlowExecNodeState executionState;
            string errorMessage = "Error";
            var success = execNode.nodeHandler.Execute(context, execNode, out executionState, ref errorMessage);
            execNode.executionStatus.ErrorMessage = errorMessage;
            execNode.executionStatus.Success = success;
            execNode.executionStatus.ExecutionStage = GridFlowGraphNodeExecutionStage.Executed;

            context.NodeStates.Register(execNode.Id, executionState);

            return success;
        }
    }
}
