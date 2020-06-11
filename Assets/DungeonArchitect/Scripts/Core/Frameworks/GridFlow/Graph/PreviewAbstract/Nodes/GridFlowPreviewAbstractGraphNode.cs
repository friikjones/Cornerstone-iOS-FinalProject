using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs;
using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Preview.Abstract
{
    public class GridFlowPreviewAbstractGraphNode : GraphNode
    {
        public GridFlowAbstractNodeState AbstractNodeState { get; set; }

        public override void Initialize(string id, Graph graph)
        {
            base.Initialize(id, graph);
            Size = new Vector2(54, 54);

            var pinPosition = Size * 0.5f;
            // Create an input pin on the top
            CreatePinOfType<GridFlowPreviewAbstractGraphNodePin>(GraphPinType.Input,
                        pinPosition,
                        Rect.zero,
                        new Vector2(0, -1));

            // Create an output pin at the bottom
            CreatePinOfType<GridFlowPreviewAbstractGraphNodePin>(GraphPinType.Output,
                        pinPosition,
                        Rect.zero,
                        new Vector2(0, -1));

        }
    }
}
