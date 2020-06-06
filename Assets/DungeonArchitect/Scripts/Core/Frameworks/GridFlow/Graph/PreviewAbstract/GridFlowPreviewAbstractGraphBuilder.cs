using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Graphs;
using DungeonArchitect.UI;
using System.Collections.Generic;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Preview.Abstract
{
    public class GridFlowPreviewAbstractGraphBuilder
    {
        public static void Build(GridFlowAbstractGraph abstractGraph, GraphBuilder graphBuilder)
        {
            if (graphBuilder == null)
            {
                return;
            }

            graphBuilder.DestroyAllNodes(null);

            if (abstractGraph == null)
            {
                return;
            }

            // Create the nodes
            var runtimeToPreviewMap = new Dictionary<System.Guid, GridFlowPreviewAbstractGraphNode>();
            foreach (var runtimeNode in abstractGraph.Nodes)
            {
                var previewNode = graphBuilder.CreateNode<GridFlowPreviewAbstractGraphNode>(null);
                if (previewNode != null)
                {
                    previewNode.AbstractNodeState = runtimeNode.state;
                    previewNode.Position = runtimeNode.Position;
                    runtimeToPreviewMap.Add(runtimeNode.NodeId, previewNode);
                }
            }

            foreach (var link in abstractGraph.Links)
            {
                var startNode = runtimeToPreviewMap[link.Source];
                var endNode = runtimeToPreviewMap[link.Destination];

                if (startNode != null && endNode != null)
                {
                    var outputPin = startNode.OutputPin;
                    var inputPin = endNode.InputPin;

                    if (outputPin != null && inputPin != null)
                    {
                        var previewLink = graphBuilder.LinkNodes<GridFlowPreviewAbstractGraphLink>(startNode.OutputPin, endNode.OutputPin);
                        previewLink.AbstractLinkState = link.state;
                    }
                }

            }
        }
    }


    public class GridFlowPreviewAbstractGraphUtils
    {
        public static GridFlowItem[] GetAllItems(GridFlowPreviewAbstractGraph previewGraph)
        {
            var items = new List<GridFlowItem>();
            foreach (var node in previewGraph.Nodes)
            {
                var previewNode = node as GridFlowPreviewAbstractGraphNode;
                if (previewNode != null)
                {
                    foreach (var item in previewNode.AbstractNodeState.Items)
                    {
                        if (item != null)
                        {
                            items.Add(item);
                        }
                    }
                }
            }

            foreach (var link in previewGraph.Links)
            {
                var previewLink = link as GridFlowPreviewAbstractGraphLink;
                if (previewLink != null)
                {
                    foreach (var item in previewLink.AbstractLinkState.Items)
                    {
                        if (item != null)
                        {
                            items.Add(item);
                        }
                    }
                }
            }

            return items.ToArray();
        }

    }
}
