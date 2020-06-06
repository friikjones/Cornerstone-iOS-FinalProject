using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    [System.Serializable]
    public enum GridFlowGraphItemType
    {
        Key,
        Lock,
        Enemy,
        Bonus,
        Entrace,
        Exit,
        Custom
    }

    [System.Serializable]
    public class GridFlowItem
    {
        public System.Guid itemId;

        /// <summary>
        /// The item type
        /// </summary>
        public GridFlowGraphItemType type;

        public string markerName = "";

        /// <summary>
        /// Reference to other items (e.g. key locks from other nodes)
        /// </summary>
        public List<System.Guid> referencedItemIds = new List<System.Guid>();

        public bool editorSelected = false;

        public GridFlowGraphItemCustomInfo customInfo = GridFlowGraphItemCustomInfo.Default;

        public GridFlowItemPlacementSettings placementSettings = new GridFlowItemPlacementSettings();

        public GridFlowItem Clone()
        {
            var newItem = new GridFlowItem();
            newItem.itemId = itemId;
            newItem.type = type;
            newItem.markerName = markerName;
            newItem.referencedItemIds = new List<System.Guid>(referencedItemIds);
            newItem.customInfo = customInfo;
            newItem.placementSettings = (placementSettings != null) ? placementSettings.Clone() : null;
            return newItem;
        }

        public GridFlowItem()
        {
            itemId = System.Guid.NewGuid();
        }

    }

    [System.Serializable]
    public struct GridFlowGraphItemCustomInfo
    {
        public string itemType;
        public string text;
        public Color textColor;
        public Color backgroundColor;

        public static readonly GridFlowGraphItemCustomInfo Default = new GridFlowGraphItemCustomInfo("custom", "", Color.white, Color.black);

        public GridFlowGraphItemCustomInfo(string itemType, string text, Color textColor, Color backgroundColor)
        {
            this.itemType = itemType;
            this.text = text;
            this.textColor = textColor;
            this.backgroundColor = backgroundColor;
        }
    }
}
