using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowItemMetadataComponent : MonoBehaviour
    {
        public GridFlowGraphItemType itemType;

        public string itemId;

        public string[] referencedItemIds = new string[0];
    }
}
