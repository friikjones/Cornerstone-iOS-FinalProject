using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs.Layouts;
using DungeonArchitect.Grammar;
using DungeonArchitect.RuntimeGraphs;

namespace DungeonArchitect.Graphs.Layouts
{
    public class RuntimeGraphLayoutNodeActions : IGraphLayoutNodeActions<RuntimeGraphNode<GrammarRuntimeGraphNodeData>>
    {
        public void SetNodePosition(RuntimeGraphNode<GrammarRuntimeGraphNodeData> node, Vector2 position)
        {
            node.Position = position;
        }

        public Vector2 GetNodePosition(RuntimeGraphNode<GrammarRuntimeGraphNodeData> node)
        {
            return node.Position;
        }

        public RuntimeGraphNode<GrammarRuntimeGraphNodeData>[] GetOutgoingNodes(RuntimeGraphNode<GrammarRuntimeGraphNodeData> node)
        {
            return node.Outgoing.ToArray();
        }
    }
}


