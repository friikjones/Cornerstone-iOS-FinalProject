using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec
{
    public enum GridFlowGraphNodeExecutionStage
    {
        NotExecuted,
        WaitingToExecute,       // Dependent nodes are being processed
        Executed                // The node has been executed
    }
    public class GridFlowGraphNodeExecutionStatus
    {
        public GridFlowGraphNodeExecutionStage ExecutionStage { get; set; }
        public GridFlowExecNodeHandlerResultType Success { get; set; }
        public string ErrorMessage { get; set; }

        public GridFlowGraphNodeExecutionStatus()
        {
            ExecutionStage = GridFlowGraphNodeExecutionStage.NotExecuted;
            Success = GridFlowExecNodeHandlerResultType.FailHalt;
            ErrorMessage = "";
        }
    }

    public class GridFlowExecRuleGraphNode : GridFlowExecGraphNodeBase
    {
        public GridFlowExecNodeHandler nodeHandler;
        public GridFlowGraphNodeExecutionStatus executionStatus;
    }
}
