using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [GridFlowExecNodeInfo("Create Main Path", "Layout Graph/", 1010)]
    public class GridFlowExecNodeHandler_CreateMainPath : GridFlowExecNodeHandler
    {
        public int pathSize = 12;
        public string pathName = "main";
        public Color nodeColor = Color.green;
        public string startMarkerName = "SpawnPoint";
        public string goalMarkerName = "LevelGoal";
        public string startNodePathName = "main_start";
        public string goalNodePathName = "main_goal";
        public GridFlowItemPlacementSettings startPlacementSettings = new GridFlowItemPlacementSettings();
        public GridFlowItemPlacementSettings goalPlacementSettings = new GridFlowItemPlacementSettings();
        public bool drawDebug = false;


        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(execNode, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_AbstractGraph(graph);

            if (pathSize <= 0)
            {
                errorMessage = "Invalid path size";
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }
            if (graph == null || graph.Nodes.Count == 0)
            {
                errorMessage = "Missing graph input";
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            if (!GridFlowItemPlacementStrategyUtils.Validate(startPlacementSettings, ref errorMessage))
            {
                errorMessage = "Start Item: " + errorMessage;
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            if (!GridFlowItemPlacementStrategyUtils.Validate(goalPlacementSettings, ref errorMessage))
            {
                errorMessage = "Goal Item: " + errorMessage;
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            int numNodes = graph.Nodes.Count;
            int[] shuffledEntranceIndices = MathUtils.GetShuffledIndices(numNodes, context.Random);
            foreach (var entranceIndex in shuffledEntranceIndices)
            {
                var entranceNode = graph.Nodes[entranceIndex];

                var visited = new HashSet<GridFlowAbstractGraphNode>();
                var path = new List<GridFlowAbstractGraphNode>();

                if (GrowPath(graph, entranceNode, context.Random, path, visited))
                {
                    // Found the path.  Finalize and return
                    FinalizePath(graph, path);
                    return GridFlowExecNodeHandlerResultType.Success;
                }
            }

            errorMessage = "Cannot find path";
            return GridFlowExecNodeHandlerResultType.FailRetry;
        }

        bool GrowPath(GridFlowAbstractGraph graph, GridFlowAbstractGraphNode currentNode, System.Random random, List<GridFlowAbstractGraphNode> path, HashSet<GridFlowAbstractGraphNode> visited)
        {
            visited.Add(currentNode);
            path.Add(currentNode);

            if (path.Count == pathSize)
            {
                return true;
            }

            var connectedNodes = graph.GetConnectedNodes(currentNode);
            int[] connectedNodeIndices = MathUtils.GetShuffledIndices(connectedNodes.Length, random);
            foreach (var connectedNodeIndex in connectedNodeIndices)
            {
                var connectedNode = connectedNodes[connectedNodeIndex];
                if (connectedNode.state.Active)
                {
                    continue;
                }

                if (visited.Contains(connectedNode))
                {
                    continue;
                }

                if (GrowPath(graph, connectedNode, random, path, visited))
                {
                    return true;
                }
            }

            // Child branches failed to produce a valid path. find another path
            visited.Remove(currentNode);
            path.Remove(currentNode);
            return false;
        }

        private void FinalizePath(Abstract.GridFlowAbstractGraph graph, List<GridFlowAbstractGraphNode> path)
        {
            if (path.Count == 0) return;

            for (int i = 0; i < path.Count; i++)
            {
                var pathNode = path[i];
                pathNode.state.Active = true;
                pathNode.state.Color = nodeColor;

                var nodePath = pathName;
                if (i == 0 && startNodePathName.Length > 0) nodePath = startNodePathName;
                if (i == path.Count - 1 && goalNodePathName.Length > 0) nodePath = goalNodePathName;
                pathNode.state.AddTag(nodePath);

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

            // Add an entry item to the first node in the main branch
            {
                var entranceNode = path[0];
                var entryItem = new GridFlowItem();
                entryItem.type = GridFlowGraphItemType.Entrace;
                entryItem.markerName = startMarkerName;
                entryItem.placementSettings = (startPlacementSettings != null) ? startPlacementSettings.Clone() : null;
                entranceNode.state.AddItem(entryItem);
            }

            // Add an exit to the last node
            {
                var exitNode = path[path.Count - 1];
                var exitItem = new GridFlowItem();
                exitItem.type = GridFlowGraphItemType.Exit;
                exitItem.markerName = goalMarkerName;
                exitItem.placementSettings = (goalPlacementSettings != null) ? goalPlacementSettings.Clone() : null;
                exitNode.state.AddItem(exitItem);
            }
        }

    }
}

