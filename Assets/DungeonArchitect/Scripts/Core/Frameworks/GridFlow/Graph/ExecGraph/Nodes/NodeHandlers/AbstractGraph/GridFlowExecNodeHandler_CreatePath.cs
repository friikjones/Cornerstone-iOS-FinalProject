using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [GridFlowExecNodeInfo("Create Path", "Layout Graph/", 1020)]
    public class GridFlowExecNodeHandler_CreatePath : GridFlowExecNodeHandler
    {
        public int minPathSize = 3;
        public int maxPathSize = 3;
        public string pathName = "branch";
        public Color nodeColor = new Color(1, 0.5f, 0);

        public string startFromPath = "main";
        public string endOnPath = "";

        public bool drawDebug = false;

        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(execNode, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_AbstractGraph(graph);
            maxPathSize = Mathf.Max(maxPathSize, minPathSize);

            if (minPathSize <= 0)
            {
                errorMessage = "Invalid path size";
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }
            if (graph == null || graph.Nodes.Count == 0)
            {
                errorMessage = "Missing graph input";
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }


            var sourceNodes =  (from n in graph.Nodes
                                where n.state.Tags.Contains(startFromPath)
                                select n).ToArray();

            GridFlowAbstractGraphNode[] sinkNodes = null;
            if (endOnPath.Length > 0)
            {
                sinkNodes = (from n in graph.Nodes
                                 where n.state.Tags.Contains(endOnPath)
                                 select n).ToArray();
            }


            int numSourceNodes = sourceNodes.Length;
            int[] sourceNodeIndices = MathUtils.GetShuffledIndices(numSourceNodes, context.Random);
            foreach (var sourceNodeIndex in sourceNodeIndices)
            {
                var headNode = sourceNodes[sourceNodeIndex];

                foreach (var startNode in graph.GetConnectedNodes(headNode))
                {
                    if (!startNode.state.Active)
                    {
                        var state = new GrowthState();
                        state.visited = new HashSet<GridFlowAbstractGraphNode>();
                        state.path = new List<GridFlowAbstractGraphNode>();

                        var staticState = new StaticGrowthState();
                        staticState.graph = graph;
                        staticState.headNode = headNode;
                        staticState.sinkNodes = sinkNodes;
                        staticState.random = context.Random;

                        if (GrowPath(startNode, staticState, state))
                        {
                            // Found the path.  Finalize and return
                            FinalizePath(staticState, state);
                            return GridFlowExecNodeHandlerResultType.Success;
                        }
                    }
                }
            }

            errorMessage = "Cannot find path";
            return GridFlowExecNodeHandlerResultType.FailRetry;
        }

        class StaticGrowthState
        {
            public GridFlowAbstractGraph graph;
            public GridFlowAbstractGraphNode headNode;
            public GridFlowAbstractGraphNode[] sinkNodes;
            public System.Random random;
        }

        class GrowthState
        {
            public List<GridFlowAbstractGraphNode> path;
            public HashSet<GridFlowAbstractGraphNode> visited;
            public GridFlowAbstractGraphNode tailNode;
        }

        bool GrowPath(GridFlowAbstractGraphNode currentNode, StaticGrowthState staticState, GrowthState state)
        {
            state.visited.Add(currentNode);
            state.path.Add(currentNode);

            if (state.path.Count >= minPathSize && state.path.Count <= maxPathSize)
            {
                // Check if we are near the sink nodes, if any
                if (staticState.sinkNodes == null)
                {
                    // No sink nodes constraints defined
                    return true;
                }

                var nearbyNodes = new HashSet<GridFlowAbstractGraphNode>(staticState.graph.GetConnectedNodes(currentNode));
                foreach (var sinkNode in staticState.sinkNodes)
                {
                    if (state.path.Count == 1 && sinkNode == staticState.headNode)
                    {
                        // If the path node size is 1, we don't want to connect back to the head node
                        continue;
                    }
                    if (nearbyNodes.Contains(sinkNode))
                    {
                        state.tailNode = sinkNode;
                        return true;
                    }
                }

                if (state.path.Count == maxPathSize)
                {
                    // no sink nodes nearby and we've reached the max path size
                    state.visited.Remove(currentNode);
                    state.path.Remove(currentNode);
                    return false;
                }
            }

            var connectedNodes = staticState.graph.GetConnectedNodes(currentNode);
            int[] connectedNodeIndices = MathUtils.GetShuffledIndices(connectedNodes.Length, staticState.random);
            foreach (var connectedNodeIndex in connectedNodeIndices)
            {
                var connectedNode = connectedNodes[connectedNodeIndex];
                if (connectedNode.state.Active)
                {
                    continue;
                }

                if (state.visited.Contains(connectedNode))
                {
                    continue;
                }

                if (GrowPath(connectedNode, staticState, state))
                {
                    return true;
                }
            }

            // Child branches failed to produce a valid path. find another path
            state.visited.Remove(currentNode);
            state.path.Remove(currentNode);
            return false;
        }

        private void FinalizePath(StaticGrowthState staticState, GrowthState state)
        {
            var path = state.path;
            var graph = staticState.graph;

            if (path.Count == 0) return;
            for (int i = 0; i < state.path.Count; i++)
            {
                var pathNode = path[i];
                pathNode.state.Active = true;
                pathNode.state.Color = nodeColor;
                pathNode.state.AddTag(pathName);

                if (drawDebug)
                {
                    var debugItem = new GridFlowItem();
                    debugItem.type = GridFlowGraphItemType.Custom;
                    debugItem.customInfo.itemType = "debug";
                    debugItem.customInfo.text = i.ToString();
                    pathNode.state.AddItem(debugItem);
                }

                // Link them
                if (i > 0)
                {
                    var prevPathNode = path[i - 1];
                    var link = graph.GetLink(prevPathNode, pathNode, true);
                    if (link != null)
                    {
                        link.state.Directional = true;
                        link.Source = prevPathNode.NodeId;
                        link.Destination = pathNode.NodeId;
                    }
                }
            }

            
            // Setup the branch start / end links
            var headLink = graph.GetLink(staticState.headNode, path[0], true);
            if (headLink != null)
            {
                headLink.state.Directional = true;
                headLink.Source = staticState.headNode.NodeId;
                headLink.Destination = path[0].NodeId;
            }

            // Find the end node, if any so that it can merge back to the specified branch (specified in variable endOnPath)
            if (state.tailNode != null)
            {
                var tailLink = graph.GetLink(path[path.Count - 1], state.tailNode, true);
                if (tailLink != null)
                {
                    tailLink.state.Directional = true;
                    tailLink.Source = path[path.Count - 1].NodeId;
                    tailLink.Destination = state.tailNode.NodeId;
                }
            }
        }

    }
}
