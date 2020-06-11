using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs;

namespace DungeonArchitect.UI.Widgets.GraphEditors
{
    public class EditorGraphBuilder : GraphBuilder
    {
        private Object assetObject;
        private UIPlatform platform;

        public EditorGraphBuilder(Graph graph, Object assetObject, UIPlatform platform) 
            : base(graph)
        {
            this.assetObject = assetObject;
            this.platform = platform;
        }

        public override GraphNode CreateNode(System.Type nodeType, UIUndoSystem undo)
        {
            var node = GraphOperations.CreateNode(graph, nodeType, undo);
            node.Position = Vector2.zero;
            if (assetObject != null)
            {
                GraphEditorUtils.AddToAsset(platform, assetObject, node);
            }
            return node;
        }

        public override void DestroyNode(GraphNode node, UIUndoSystem undo)
        {
            GraphOperations.DestroyNode(node, undo);
        }

        public override TLink LinkNodes<TLink>(GraphPin outputPin, GraphPin inputPin)
        {
            // Make sure a link doesn't already exists
            foreach (var existingLink in graph.Links)
            {
                if (existingLink.Input == inputPin && existingLink.Output == outputPin)
                {
                    return null;
                }
            }

            TLink link = GraphOperations.CreateLink<TLink>(graph);
            link.Input = inputPin;
            link.Output = outputPin;

            if (assetObject != null)
            {
                GraphEditorUtils.AddToAsset(platform, assetObject, link);
            }
            return link;
        }

    }
}
