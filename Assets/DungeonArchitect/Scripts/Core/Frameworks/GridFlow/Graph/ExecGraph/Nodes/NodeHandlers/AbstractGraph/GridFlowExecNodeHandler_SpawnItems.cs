using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [System.Serializable]
    public enum GridFlowExecNodeHandler_SpawnItemMethod
    {
        RandomRange,
        LinearDifficulty,
        CurveDifficulty
    }


    [GridFlowExecNodeInfo("Spawn Items", "Layout Graph/", 1030)]
    public class GridFlowExecNodeHandler_SpawnItems : GridFlowExecNodeHandler
    {
        public string[] paths = new string[] { "main" };

        public GridFlowGraphItemType itemType = GridFlowGraphItemType.Enemy;
        public string markerName = "";
        public GridFlowGraphItemCustomInfo customItemInfo = GridFlowGraphItemCustomInfo.Default;
        public int minCount = 1;
        public int maxCount = 4;
        public GridFlowExecNodeHandler_SpawnItemMethod spawnMethod = GridFlowExecNodeHandler_SpawnItemMethod.LinearDifficulty;
        public AnimationCurve spawnDistributionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float spawnDistributionVariance = 0.2f;
        public float minSpawnDifficulty = 0.0f;
        public float spawnProbability = 1.0f;

        public GridFlowItemPlacementSettings placementSettings = new GridFlowItemPlacementSettings();

        public bool showDifficulty = false;
        public Color difficultyInfoColor = new Color(0, 0, 0.5f);

        class NodeInfo
        {
            public NodeInfo(GridFlowAbstractGraphNode node, float weight)
            {
                this.node = node;
                this.weight = weight;
            }

            public GridFlowAbstractGraphNode node;
            public float weight;
        }

        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(execNode, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_AbstractGraph(graph);

            var nodesByPath = new Dictionary<string, NodeInfo[]>();
            {
                var weights = GridFlowExecNodeUtils.CalculateWeights(graph, 1);
                var nodesByPathList = new Dictionary<string, List<NodeInfo>>();
                foreach (var entry in weights)
                {
                    var node = entry.Key;
                    var weight = entry.Value;
                    foreach (var pathName in node.state.Tags)
                    {
                        if (paths.Contains(pathName))
                        {
                            if (!nodesByPathList.ContainsKey(pathName))
                            {
                                nodesByPathList.Add(pathName, new List<NodeInfo>());
                            }
                            nodesByPathList[pathName].Add(new NodeInfo(node, weight));
                            break;
                        }
                    }
                }

                // Sort the path list
                foreach (var entry in nodesByPathList)
                {
                    var pathName = entry.Key;
                    var pathList = entry.Value;
                    var sortedPath = pathList.OrderBy(info => info.weight).ToArray();
                    nodesByPath.Add(pathName, sortedPath);
                }
            }

            // Normalize the weights
            foreach (var entry in nodesByPath)
            {
                var pathName = entry.Key;
                var pathNodes = entry.Value;
                if (pathName.Length == 0) continue;

                float minWeight = float.MaxValue;
                float maxWeight = -float.MaxValue;
                foreach(var pathNode in pathNodes)
                {
                    minWeight = Mathf.Min(minWeight, pathNode.weight);
                    maxWeight = Mathf.Max(maxWeight, pathNode.weight);
                }

                foreach (var pathNode in pathNodes)
                {
                    if (Mathf.Abs(maxWeight - minWeight) > 1e-6f)
                    {
                        pathNode.weight = (pathNode.weight - minWeight) / (maxWeight - minWeight);
                    }
                    else
                    {
                        pathNode.weight = 1;
                    }
                }
            }

            foreach (var pathName in paths)
            {
                if (!nodesByPath.ContainsKey(pathName)) continue;
                NodeInfo[] pathNodes = nodesByPath[pathName];

                foreach (var pathNode in pathNodes)
                {
                    if (pathNode.weight < minSpawnDifficulty) continue;
                    int spawnCount = GetSpawnCount(context.Random, pathNode.weight);

                    for (int i = 0; i < spawnCount; i++)
                    {
                        var item = new GridFlowItem();
                        item.type = itemType;
                        item.markerName = markerName;
                        item.customInfo = customItemInfo;
                        if (!GridFlowItemPlacementStrategyUtils.Validate(placementSettings, ref errorMessage))
                        {
                            return GridFlowExecNodeHandlerResultType.FailHalt;
                        }
                        item.placementSettings = placementSettings.Clone();
                        pathNode.node.state.AddItem(item);
                    }
                }

                if (showDifficulty)
                {
                    EmitDebugInfo(pathNodes);
                }
            }

            return GridFlowExecNodeHandlerResultType.Success;
        }

        int GetSpawnCount(System.Random random, float weight)
        {
            weight = Mathf.Clamp01(weight);
            
            if (spawnMethod == GridFlowExecNodeHandler_SpawnItemMethod.CurveDifficulty && spawnDistributionCurve == null)
            {
                spawnMethod = GridFlowExecNodeHandler_SpawnItemMethod.LinearDifficulty;
            }

            int spawnCount = 0;
            if (spawnMethod == GridFlowExecNodeHandler_SpawnItemMethod.RandomRange)
            {
                spawnCount = random.Range(minCount, maxCount);
            }
            else if (spawnMethod == GridFlowExecNodeHandler_SpawnItemMethod.LinearDifficulty)
            {
                var v = random.Range(-spawnDistributionVariance, spawnDistributionVariance);
                var w = Mathf.Clamp01(weight + v);
                spawnCount = Mathf.RoundToInt(minCount + (maxCount - minCount) * w);
            }
            else if (spawnMethod == GridFlowExecNodeHandler_SpawnItemMethod.CurveDifficulty)
            {
                var v = random.Range(-spawnDistributionVariance, spawnDistributionVariance);
                var w = Mathf.Clamp01(weight + v);
                float t = spawnDistributionCurve.Evaluate(w);
                spawnCount = Mathf.RoundToInt(minCount + (maxCount - minCount) * t);
            }

            spawnProbability = Mathf.Clamp01(spawnProbability);
            if (random.NextFloat() > spawnProbability)
            {
                spawnCount = 0;
            }
            return spawnCount;
        }

        void EmitDebugInfo(NodeInfo[] nodes)
        {
            foreach (var nodeInfo in nodes)
            {
                var node = nodeInfo.node;
                var weight = nodeInfo.weight;

                var debugItem = new GridFlowItem();
                debugItem.type = GridFlowGraphItemType.Custom;
                debugItem.customInfo.text = weight.ToString("0.0");
                debugItem.customInfo.backgroundColor = difficultyInfoColor;
                node.state.AddItem(debugItem);
            }
        }

    }
}
