using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    [SerializeField]
    public class GridFlowItemMetadata
    {
        public GridFlowGraphItemType itemType;
        public System.Guid itemId = System.Guid.Empty;
        public System.Guid[] referencedItems = new System.Guid[0];
    }

    public class GridFlowItemMetadataHandler : DungeonItemSpawnListener
    {
        T FindOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        public override void SetMetadata(GameObject dungeonItem, DungeonNodeSpawnData spawnData)
        {
            var marker = spawnData.socket;
            if (marker.metadata is GridFlowItemMetadata)
            {
                var itemData = marker.metadata as GridFlowItemMetadata;
                var component = FindOrAddComponent<GridFlowItemMetadataComponent>(dungeonItem);
                component.itemType = itemData.itemType;
                component.itemId = itemData.itemId.ToString();

                var referencedIds = new List<string>();
                foreach (var referencedGuidId in itemData.referencedItems)
                {
                    referencedIds.Add(referencedGuidId.ToString());
                }
                component.referencedItemIds = referencedIds.ToArray();
            }
        }
    }
}
