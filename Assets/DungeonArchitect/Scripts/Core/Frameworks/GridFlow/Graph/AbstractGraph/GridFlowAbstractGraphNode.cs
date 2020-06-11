using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Abstract
{
    [System.Serializable]
    public enum GridFlowAbstractNodeRoomType
    {
        Unknown,
        Room,
        Corridor,
        Cave
    }

    [System.Serializable]
    public class GridFlowAbstractNodeState
    {
        public bool Active = false;
        public Color Color = Color.green;
        public IntVector2 GridCoord = IntVector2.Zero;
        public HashSet<string> Tags = new HashSet<string>();
        public List<GridFlowItem> Items = new List<GridFlowItem>();
        public GridFlowAbstractNodeRoomType RoomType = GridFlowAbstractNodeRoomType.Unknown;

        public GridFlowAbstractNodeState Clone()
        {
            var newState = new GridFlowAbstractNodeState();
            newState.Active = Active;
            newState.Color = Color;
            newState.GridCoord = GridCoord;
            newState.RoomType = RoomType;

            foreach (var tag in Tags)
            {
                newState.AddTag(tag);
            }
            foreach (var item in Items)
            {
                newState.AddItem(item.Clone());
            }
            return newState;
        }

        public void AddItem(GridFlowItem item)
        {
            Items.Add(item);
        }

        public void AddTag(string tag)
        {
            Tags.Add(tag);
        }
        public void AddTags(string[] tags)
        {
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }
    }

    [System.Serializable]
    public class GridFlowAbstractGraphNode
    {
        public GridFlowAbstractGraphNode()
        {
            NodeId = System.Guid.NewGuid();
        }

        public GridFlowAbstractGraphNode Clone()
        {
            var newNode = new GridFlowAbstractGraphNode();
            newNode.NodeId = NodeId;
            newNode.Position = Position;
            newNode.state = state.Clone();
            return newNode;
        }

        public System.Guid NodeId;
        public Vector2 Position = Vector2.zero;
        public GridFlowAbstractNodeState state = new GridFlowAbstractNodeState();
    }
}
