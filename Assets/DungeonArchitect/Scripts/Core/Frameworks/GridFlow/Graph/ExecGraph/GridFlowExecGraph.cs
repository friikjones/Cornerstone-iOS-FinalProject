using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs;
using DungeonArchitect.Utils;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec
{
    public class GridFlowExecGraph : Graph
    {
        [SerializeField]
        public GridFlowExecResultGraphNode resultNode;

        public override void OnEnable()
        {
            base.OnEnable();

            hideFlags = HideFlags.HideInHierarchy;
        }
    }



    public class GridFlowExecGraphUtils
    {
        public static GridFlowExecRuleGraphNode[] GetIncomingNodes(GridFlowExecRuleGraphNode node)
        {
            var result = new List<GridFlowExecRuleGraphNode>();
            var incomingNodes = GraphUtils.GetIncomingNodes(node);
            foreach (var incomingNode in incomingNodes)
            {
                var incomingExecNode = incomingNode as GridFlowExecRuleGraphNode;
                if (incomingExecNode != null)
                {
                    result.Add(incomingExecNode);
                }
            }
            return result.ToArray();
        }

    }
}
