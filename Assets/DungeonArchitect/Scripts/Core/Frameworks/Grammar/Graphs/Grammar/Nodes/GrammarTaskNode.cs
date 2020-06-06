using DungeonArchitect.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DMathUtils = DungeonArchitect.Utils.MathUtils;

namespace DungeonArchitect.Grammar
{
    public class GrammarTaskNode : GrammarNodeBase
    {
        public GrammarNodeType NodeType;
        public int executionIndex = 0;
        public bool DisplayExecutionIndex = true;

        public override void Initialize(string id, Graph graph)
        {
            base.Initialize(id, graph);
            Size = new Vector2(120, 120);

            // Create an input pin on the top
            CreatePinOfType<GrammarNodePin>(GraphPinType.Input,
                        Vector2.zero,
                        Rect.zero,
                        new Vector2(0, -1));

            // Create an output pin at the bottom
            CreatePinOfType<GrammarNodePin>(GraphPinType.Output,
                        Vector2.zero,
                        Rect.zero,
                        new Vector2(0, -1));

        }

        public override void CopyFrom(GraphNode otherNode) {
            base.CopyFrom(otherNode);

            if (otherNode is GrammarTaskNode)
            {
                var otherTaskNode = otherNode as GrammarTaskNode;
                NodeType = otherTaskNode.NodeType;
                executionIndex = otherTaskNode.executionIndex;
                DisplayExecutionIndex = otherTaskNode.DisplayExecutionIndex;
            }
        }
    }
}
