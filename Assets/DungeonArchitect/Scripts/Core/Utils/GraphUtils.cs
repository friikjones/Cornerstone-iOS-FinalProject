//$ Copyright 2016, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//

using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect;
using DungeonArchitect.Graphs;

namespace DungeonArchitect.Utils
{
    /// <summary>
    /// Theme graph utility functions
    /// </summary>
    public class GraphUtils
    {
        private static GraphNode[] GetDirectionalNodes(GraphPin pin, bool isIncoming)
        {
            var result = new List<GraphNode>();
            var hostNode = pin.Node;
            if (hostNode && hostNode.Graph)
            {
                var graph = hostNode.Graph;
                foreach (var link in graph.Links)
                {
                    if (isIncoming && link.Input == pin)
                    {
                        var otherNode = link.Output.Node;
                        result.Add(otherNode);
                    }
                    else if (!isIncoming && link.Output == pin)
                    {
                        var otherNode = link.Input.Node;
                        result.Add(otherNode);
                    }
                }
            }
            return result.ToArray();
        }

        private static GraphNode[] GetDirectionalNodes(GraphNode hostNode, bool isIncoming)
        {
            var result = new List<GraphNode>();
            if (hostNode && hostNode.Graph)
            {
                var graph = hostNode.Graph;
                foreach (var link in graph.Links)
                {
                    if (isIncoming && link.Input.Node == hostNode)
                    {
                        var otherNode = link.Output.Node;
                        result.Add(otherNode);
                    }
                    else if (!isIncoming && link.Output.Node == hostNode)
                    {
                        var otherNode = link.Input.Node;
                        result.Add(otherNode);
                    }
                }
            }
            return result.ToArray();
        }

        public static GraphNode[] GetIncomingNodes(GraphPin pin)
        {
            return GetDirectionalNodes(pin, true);
        }

        public static GraphNode[] GetOutgoingNodes(GraphPin pin)
        {
            return GetDirectionalNodes(pin, false);
        }

        public static GraphNode[] GetIncomingNodes(GraphNode node)
        {
            return GetDirectionalNodes(node, true);
        }

        public static GraphNode[] GetOutgoingNodes(GraphNode node)
        {
            return GetDirectionalNodes(node, false);
        }


    }
}
