using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [GridFlowExecNodeInfo("Create Key Lock", "Layout Graph/", 1040)]
    public class GridFlowExecNodeHandler_CreateKeyLock : GridFlowExecNodeHandler
    {
        public string keyBranch = "main";
        public string lockBranch = "main";
        public string keyMarkerName = "Key";
        public string lockMarkerName = "Lock";
        public GridFlowItemPlacementSettings placementSettings = new GridFlowItemPlacementSettings();

        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode node, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(node, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_AbstractGraph(graph);

            GridFlowAbstractGraphNode keyNode, lockNode;
            if (FindKeyLockNodes(context, graph, out keyNode, out lockNode))
            {
                var keyItem = new GridFlowItem();
                keyItem.type = GridFlowGraphItemType.Key;
                keyItem.markerName = keyMarkerName;
                if (!GridFlowItemPlacementStrategyUtils.Validate(placementSettings, ref errorMessage)) 
                {
                    return GridFlowExecNodeHandlerResultType.FailHalt;
                }
                keyItem.placementSettings = placementSettings.Clone();
                keyNode.state.AddItem(keyItem);

                var lockItem = new GridFlowItem();
                lockItem.type = GridFlowGraphItemType.Lock;
                lockItem.markerName = lockMarkerName;
                lockNode.state.AddItem(lockItem);
                keyItem.referencedItemIds.Add(lockItem.itemId);

                return GridFlowExecNodeHandlerResultType.Success;
            }

            errorMessage = "Cannot find key-lock";
            return GridFlowExecNodeHandlerResultType.FailRetry;
        }


        private bool FindKeyLockNodes(GridFlowExecutionContext context, GridFlowAbstractGraph graph, out GridFlowAbstractGraphNode keyNode, out GridFlowAbstractGraphNode lockNode)
        {
            var weights = GridFlowExecNodeUtils.CalculateWeights(graph, 1);

            var keyNodes = (from n in graph.Nodes
                            where n.state.Tags.Contains(keyBranch)
                            select n).ToArray();

            var lockNodes = (from n in graph.Nodes
                             where n.state.Tags.Contains(lockBranch)
                             select n).ToArray();

            int minKeyWeight = int.MaxValue;
            int maxKeyWeight = -int.MaxValue;

            int minLockWeight = int.MaxValue;
            int maxLockWeight = -int.MaxValue;

            foreach (var node in keyNodes)
            {
                minKeyWeight = Mathf.Min(minKeyWeight, weights[node]);
                maxKeyWeight = Mathf.Max(maxKeyWeight, weights[node]);
            }
            foreach (var node in lockNodes)
            {
                minLockWeight = Mathf.Min(minLockWeight, weights[node]);
                maxLockWeight = Mathf.Max(maxLockWeight, weights[node]);
            }

            
            if (maxLockWeight < minKeyWeight || maxKeyWeight < minKeyWeight || maxLockWeight < minLockWeight)
            {
                keyNode = null;
                lockNode = null;
                return false;
            }

            minLockWeight = Mathf.Max(minLockWeight, minKeyWeight);
            maxKeyWeight = Mathf.Min(maxKeyWeight, maxLockWeight);

            keyNodes = GridFlowExecNodeUtils.FilterNodes(keyNodes, minKeyWeight, maxKeyWeight, weights);

            keyNode = keyNodes[context.Random.Range(0, keyNodes.Length - 1)];
            var keyWeight = weights[keyNode];

            lockNodes = GridFlowExecNodeUtils.FilterNodes(lockNodes, keyWeight, maxLockWeight, weights);

            lockNode = lockNodes[context.Random.Range(0, lockNodes.Length - 1)];
            return true;
        }
    }
}
