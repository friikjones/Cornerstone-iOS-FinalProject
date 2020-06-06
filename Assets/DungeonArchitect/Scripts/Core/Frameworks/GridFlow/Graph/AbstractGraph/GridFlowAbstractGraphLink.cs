using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Abstract
{
    [System.Serializable]
    public class GridFlowAbstractGraphLinkState
    {
        public bool Directional = false;
        public bool OneWay = false;
        public List<GridFlowItem> Items = new List<GridFlowItem>();

        public GridFlowAbstractGraphLinkState Clone()
        {
            var newState = new GridFlowAbstractGraphLinkState();
            newState.Directional = Directional;
            newState.OneWay = OneWay;
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

    }

    [System.Serializable]
    public class GridFlowAbstractGraphLink
    {
        public System.Guid LinkId;
        public System.Guid Source;
        public System.Guid Destination;
        public GridFlowAbstractGraphLinkState state = new GridFlowAbstractGraphLinkState();

        public GridFlowAbstractGraphLink()
        {
            LinkId = System.Guid.NewGuid();
        }

        public GridFlowAbstractGraphLink Clone()
        {
            var newLink = new GridFlowAbstractGraphLink();
            newLink.LinkId = LinkId;
            newLink.Source = Source;
            newLink.Destination = Destination;
            newLink.state = state.Clone();
            return newLink;
        }
    }
}
