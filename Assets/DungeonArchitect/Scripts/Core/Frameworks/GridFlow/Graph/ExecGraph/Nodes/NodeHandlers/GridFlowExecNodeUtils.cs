using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    public class GridFlowExecNodeUtils 
    {
        public static GridFlowExecNodeState[] GetIncomingStates(GridFlowExecRuleGraphNode currentNode, GridFlowExecNodeStates nodeStates)
        {
            var incomingStates = new List<GridFlowExecNodeState>();
            var incomingNodes = GridFlowExecGraphUtils.GetIncomingNodes(currentNode);
            foreach (var incomingNode in incomingNodes)
            {
                var incomingExecState = nodeStates.Get(incomingNode.Id);
                if (incomingExecState != null)
                {
                    incomingStates.Add(incomingExecState);
                }
            }
            return incomingStates.ToArray();
        }

        public static GridFlowAbstractGraph CloneIncomingAbstractGraph(GridFlowExecRuleGraphNode currentNode, GridFlowExecNodeStates nodeStates)
        {
            var incomingStates = GetIncomingStates(currentNode, nodeStates);
            if (incomingStates.Length > 0)
            {
                var incomingState = incomingStates[0];
                if (incomingState.AbstractGraph != null)
                {
                    return incomingState.AbstractGraph.Clone();
                }
            }
            return null;
        }

        public static GridFlowTilemap CloneIncomingTilemap(GridFlowExecRuleGraphNode currentNode, GridFlowExecNodeStates nodeStates)
        {
            var incomingStates = GetIncomingStates(currentNode, nodeStates);
            if (incomingStates.Length > 0)
            {
                var incomingState = incomingStates[0];
                if (incomingState.Tilemap != null)
                {
                    return incomingState.Tilemap.Clone();
                }
            }
            return null;
        }

        public static GridFlowAbstractGraphNode FindNodeWithItemType(GridFlowAbstractGraph graph, GridFlowGraphItemType itemType)
        {
            foreach (var node in graph.Nodes)
            {
                foreach (var item in node.state.Items)
                {
                    if (item.type == itemType)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        public static GridFlowAbstractGraphNode[] FilterNodes(GridFlowAbstractGraphNode[] nodes, int minWeight, int maxWeight, Dictionary<GridFlowAbstractGraphNode, int> weights)
        {
            var validNodes = new List<GridFlowAbstractGraphNode>();
            foreach (var node in nodes)
            {
                var weight = weights[node];
                if (weight >= minWeight && weight <= maxWeight)
                {
                    validNodes.Add(node);
                }
            }
            return validNodes.ToArray();
        }

        public static bool ContainsItem(GridFlowItem[] items, GridFlowGraphItemType itemType)
        {
            foreach (var item in items)
            {
                if (item.type == itemType)
                {
                    return true;
                }
            }

            return false;
        }

        protected struct NodeWeightAssignInfo
        {
            public GridFlowAbstractGraphNode node;
            public int weight;

            public NodeWeightAssignInfo(GridFlowAbstractGraphNode node, int weight)
            {
                this.node = node;
                this.weight = weight;
            }
        }

        public static Dictionary<GridFlowAbstractGraphNode, int> CalculateWeights(GridFlowAbstractGraph graph, int lockedWeight)
        {
            var weights = new Dictionary<GridFlowAbstractGraphNode, int>();

            // Find the start node
            GridFlowAbstractGraphNode startNode = FindNodeWithItemType(graph, GridFlowGraphItemType.Entrace);
            if (startNode != null)
            {
                var visited = new HashSet<GridFlowAbstractGraphNode>();
                var queue = new Queue<NodeWeightAssignInfo>();
                queue.Enqueue(new NodeWeightAssignInfo(startNode, 0));
                visited.Add(startNode);

                while (queue.Count > 0)
                {
                    var front = queue.Dequeue();
                    visited.Add(front.node);
                    if (weights.ContainsKey(front.node))
                    {
                        weights[front.node] = Mathf.Min(weights[front.node], front.weight);
                    }
                    else
                    {
                        weights.Add(front.node, front.weight);
                    }

                    // Traverse the children
                    foreach (var outgoingLink in graph.GetOutgoingLinks(front.node))
                    {
                        if (!outgoingLink.state.Directional) continue;
                        var outgoingNode = graph.GetNode(outgoingLink.Destination);
                        if (!outgoingNode.state.Active) continue;
                        bool traverseChild = true;
                        if (visited.Contains(outgoingNode))
                        {
                            // The child node has already been traversed.  Do not traverse if the child's weight
                            // is less than the current weight
                            var currentWeight = front.weight;
                            var childWeight = weights[outgoingNode];
                            if (currentWeight > childWeight)
                            {
                                traverseChild = false;
                            }
                        }
                        if (traverseChild)
                        {
                            var nodeWeight = 1;
                            if (ContainsItem(outgoingLink.state.Items.ToArray(), GridFlowGraphItemType.Lock))
                            {
                                nodeWeight = lockedWeight;
                            }

                            queue.Enqueue(new NodeWeightAssignInfo(outgoingNode, front.weight + nodeWeight));
                        }
                    }
                }
            }
            return weights;
        }
    }
}
