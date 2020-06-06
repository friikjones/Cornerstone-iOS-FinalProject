using DungeonArchitect.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Graphs
{
    public abstract class GraphBuilder 
    {
        protected Graph graph;
        public Graph Graph { get { return graph; } }

        public GraphBuilder(Graph graph)
        {
            this.graph = graph;
        }

        public abstract void DestroyNode(GraphNode node, UIUndoSystem undo);
        public abstract GraphNode CreateNode(System.Type nodeType, UIUndoSystem undo);
        public abstract TLink LinkNodes<TLink>(GraphPin outputPin, GraphPin inputPin) where TLink : GraphLink;
        public T CreateNode<T>(UIUndoSystem undo) where T : GraphNode
        {
            return CreateNode(typeof(T), undo) as T;
        }

        public void DestroyAllNodes(UIUndoSystem undo)
        {
            var nodes = graph.Nodes.ToArray();
            foreach (var node in nodes)
            {
                DestroyNode(node, undo);
            }
        }
    }
}
