using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Abstract
{
    [System.Serializable]
    public class GridFlowAbstractGraph
    {
        [SerializeField]
        public List<GridFlowAbstractGraphNode> Nodes = new List<GridFlowAbstractGraphNode>();

        [SerializeField]
        public List<GridFlowAbstractGraphLink> Links = new List<GridFlowAbstractGraphLink>();

        public void RemoveNode(GridFlowAbstractGraphNode node)
        {
            BreakAllLinks(node);
            Nodes.Remove(node);
        }

        public void AddNode(GridFlowAbstractGraphNode node)
        {
            Nodes.Add(node);
        }

        public void RemoveLink(GridFlowAbstractGraphLink link)
        {
            Links.Remove(link);
        }

        public GridFlowAbstractGraphNode GetNode(System.Guid nodeId)
        {
            foreach (var node in Nodes)
            {
                if (node.NodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        public GridFlowAbstractNodeState GetNodeState(System.Guid nodeId)
        {
            var node = GetNode(nodeId);
            return node != null ? node.state : null;
        }

        public GridFlowAbstractGraphLink GetLink(GridFlowAbstractGraphNode sourceNode, GridFlowAbstractGraphNode destNode)
        {
            return GetLink(sourceNode, destNode, false);
        }

        public GridFlowAbstractGraphLink GetLink(GridFlowAbstractGraphNode sourceNode, GridFlowAbstractGraphNode destNode, bool ignoreDirection)
        {
            if (sourceNode == null || destNode == null)
            {
                return null;
            }

            foreach (var link in Links)
            {
                if (link.Source == sourceNode.NodeId && link.Destination == destNode.NodeId)
                {
                    return link;
                }
                if (ignoreDirection)
                {
                    if (link.Source == destNode.NodeId && link.Destination == sourceNode.NodeId)
                    {
                        return link;
                    }
                }
            }
            return null;
        }

        public GridFlowAbstractGraphLink MakeLink(GridFlowAbstractGraphNode sourceNode, GridFlowAbstractGraphNode destNode)
        {
            if (sourceNode == null || destNode == null)
            {
                return null;
            }

            // Make sure an existing link doesn't exist
            {
                GridFlowAbstractGraphLink existingLink = GetLink(sourceNode, destNode);
                if (existingLink != null)
                {
                    // Link already exists
                    return null;
                }
            }

            // Create a new link
            var link = new GridFlowAbstractGraphLink();
            link.Source = sourceNode.NodeId;
            link.Destination = destNode.NodeId;
            Links.Add(link);
            return link;
        }

        public void BreakLink(GridFlowAbstractGraphNode sourceNode, GridFlowAbstractGraphNode destNode)
        {
            GridFlowAbstractGraphLink link = GetLink(sourceNode, destNode);
            if (link != null)
            {
                Links.Remove(link);
            }
        }

        public void BreakAllOutgoingLinks(GridFlowAbstractGraphNode node)
        {
            if (node != null)
            {
                var linkArray = Links.ToArray();
                foreach (var link in linkArray)
                {
                    if (link.Source == node.NodeId)
                    {
                        Links.Remove(link);
                    }
                }
            }
        }

        public void BreakAllIncomingLinks(GridFlowAbstractGraphNode node)
        {
            if (node != null)
            {
                var linkArray = Links.ToArray();
                foreach (var link in linkArray)
                {
                    if (link.Destination == node.NodeId)
                    {
                        Links.Remove(link);
                    }
                }
            }
        }

        public void BreakAllLinks(GridFlowAbstractGraphNode node)
        {
            if (node != null)
            {
                var linkArray = Links.ToArray();
                foreach (var link in linkArray)
                {
                    if (link.Source == node.NodeId || link.Destination == node.NodeId)
                    {
                        Links.Remove(link);
                    }
                }
            }
        }

        public void Clear()
        {
            var nodeList = Nodes.ToArray();
            foreach (var node in nodeList)
            {
                RemoveNode(node);
            }
        }

        public GridFlowAbstractGraphNode[] GetOutgoingNodes(GridFlowAbstractGraphNode node)
        {
            var result = new List<GridFlowAbstractGraphNode>();
            if (node != null)
            {
                foreach (var link in Links)
                {
                    if (link.Source == node.NodeId)
                    {
                        result.Add(GetNode(link.Destination));
                    }
                }
            }
            return result.ToArray();
        }

        public GridFlowAbstractGraphNode[] GetIncomingNodes(GridFlowAbstractGraphNode node)
        {
            var result = new List<GridFlowAbstractGraphNode>();
            if (node != null)
            {
                foreach (var link in Links)
                {
                    if (link.Destination == node.NodeId)
                    {
                        result.Add(GetNode(link.Source));
                    }
                }
            }
            return result.ToArray();
        }

        public GridFlowAbstractGraphLink[] GetOutgoingLinks(GridFlowAbstractGraphNode node)
        {
            var result = new List<GridFlowAbstractGraphLink>();
            if (node != null)
            {
                foreach (var link in Links)
                {
                    if (link.Source == node.NodeId)
                    {
                        result.Add(link);
                    }
                }
            }
            return result.ToArray();
        }

        public GridFlowAbstractGraphLink[] GetIncomingLinks(GridFlowAbstractGraphNode node)
        {
            var result = new List<GridFlowAbstractGraphLink>();
            if (node != null)
            {
                foreach (var link in Links)
                {
                    if (link.Destination == node.NodeId)
                    {
                        result.Add(link);
                    }
                }
            }
            return result.ToArray();
        }

        public GridFlowAbstractGraphNode[] GetConnectedNodes(GridFlowAbstractGraphNode node)
        {
            var result = new List<GridFlowAbstractGraphNode>();
            if (node != null)
            {
                foreach (var link in Links)
                {
                    if (link.Destination == node.NodeId)
                    {
                        result.Add(GetNode(link.Source));
                    }
                    else if (link.Source == node.NodeId)
                    {
                        result.Add(GetNode(link.Destination));
                    }
                }
            }
            return result.ToArray();
        }

        public GridFlowAbstractGraph Clone()
        {
            var newGraph = new GridFlowAbstractGraph();

            // Create nodes
            foreach (var oldNode in Nodes)
            {
                var newNode = oldNode.Clone();
                newGraph.Nodes.Add(newNode);
            }

            // Create the links
            foreach (var oldLink in Links)
            {
                var newLink = oldLink.Clone();
                newGraph.Links.Add(newLink);
            }

            return newGraph;
        }

        public GridFlowItem[] GetAllItems()
        {
            var items = new List<GridFlowItem>();

            foreach (var node in Nodes)
            {
                items.AddRange(node.state.Items);
            }

            foreach (var link in Links)
            {
                items.AddRange(link.state.Items);
            }

            return items.ToArray();
        }
    }

}
