using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DungeonArchitect.Utils;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowDungeonBuilder : DungeonBuilder
    {
        GridFlowDungeonConfig gridFlowConfig;
        GridFlowDungeonModel gridFlowModel;
        GridFlowExecNodeStates execNodeStates = null;
        public GridFlowExecNodeStates ExecNodeStates
        {
            get
            {
                return execNodeStates;
            }
        }

        public override void BuildDungeon(DungeonConfig config, DungeonModel model)
        {
            gridFlowConfig = config as GridFlowDungeonConfig;
            gridFlowModel = model as GridFlowDungeonModel;

            if (gridFlowConfig.flowAsset == null)
            {
                Debug.LogError("Missing grid flow asset");
                return;
            }

            base.BuildDungeon(config, model);

            GenerateLevelLayout();

            var minimap = GetComponent<GridFlowMinimap>();
            if (minimap != null && minimap.initMode == GridFlowMinimapInitMode.OnDungeonRebuild)
            {
                minimap.Initialize();
            }
        }

        public override void EmitMarkers()
        {
            base.EmitMarkers();

            EmitLevelMarkers();

            ProcessMarkerOverrideVolumes();
        }

        void GenerateLevelLayout()
        {
            if (gridFlowConfig == null || gridFlowModel == null || gridFlowConfig.flowAsset == null)
            {
                return;
            }

            gridFlowModel.abstractGraph = null;
            gridFlowModel.tilemap = null;

            var execGraph = gridFlowConfig.flowAsset.execGraph;
            var random = new System.Random((int)gridFlowConfig.Seed);

            GridFlowExecutor executor = new GridFlowExecutor();
            if (!executor.Execute(execGraph, random, 100, out execNodeStates))
            {
                Debug.LogError("Failed to generate level layout. Please check your grid flow graph");
            }
            else
            {
                var resultNode = execGraph.resultNode;
                var execState = execNodeStates.Get(resultNode.Id);

                gridFlowModel.abstractGraph = execState.AbstractGraph;
                gridFlowModel.tilemap = execState.Tilemap;

                if (gridFlowModel.abstractGraph == null || gridFlowModel.tilemap == null)
                {
                    Debug.Log("Failed to generate grid flow tilemap");
                    return;
                }
            }
        }

        bool IsCellOfType(GridFlowTilemap tilemap, int x, int y, GridFlowTilemapCellType[] types)
        {
            var cell = tilemap.Cells.GetCell(x, y);
            if (cell == null) return false;
            return types.Contains(cell.CellType);
        }

        Quaternion GetBaseTransform(GridFlowTilemap tilemap, int x, int y)
        {
            var cellTypesToTransform = new GridFlowTilemapCellType[]
            {
                GridFlowTilemapCellType.Wall,
                GridFlowTilemapCellType.Door
            };

            var cell = tilemap.Cells[x, y];
            if (!cellTypesToTransform.Contains(cell.CellType))
            {
                return Quaternion.identity;
            }

            var validL = IsCellOfType(tilemap, x - 1, y, cellTypesToTransform);
            var validR = IsCellOfType(tilemap, x + 1, y, cellTypesToTransform);
            var validB = IsCellOfType(tilemap, x, y - 1, cellTypesToTransform);
            var validT = IsCellOfType(tilemap, x, y + 1, cellTypesToTransform);

            var angleY = 0;
            if (validL && validR)
            {
                angleY = validT ? 180 : 0;
            }
            else if (validT && validB)
            {
                angleY = validR ? 270 : 90;
            }
            else if (validL && validT) angleY = 180;
            else if (validL && validB) angleY = 90;
            else if (validR && validT) angleY = 270;
            else if (validR && validB) angleY = 0;

            return Quaternion.Euler(0, angleY, 0);
        }

        void EmitLevelMarkers()
        {
            var items = gridFlowModel.abstractGraph.GetAllItems();
            var itemMap = new Dictionary<System.Guid, GridFlowItem>();
            foreach (var item in items)
            {
                itemMap[item.itemId] = item;
            }

            if (gridFlowConfig == null || gridFlowModel == null)
            {
                return;
            }

            var tilemap = gridFlowModel.tilemap;
            if (tilemap == null)
            {
                return;
            }

            var basePosition = transform.position;
            var gridSize = gridFlowConfig.gridSize;
            for (int x = 0; x < tilemap.Width; x++)
            {
                for (int y = 0; y < tilemap.Height; y++)
                {
                    var position = basePosition + Vector3.Scale(new Vector3(x + 0.5f, 0, y + 0.5f), gridSize);
                    var baseRotation = GetBaseTransform(tilemap, x, y);
                    var markerTransform = Matrix4x4.TRS(position, baseRotation, Vector3.one);
                    var cell = tilemap.Cells[x, y];
                    int cellId = tilemap.Width * y + x;

                    if (cell.Item != System.Guid.Empty && itemMap.ContainsKey(cell.Item))
                    {
                        var item = itemMap[cell.Item];
                        if (item.markerName != null && item.markerName.Length > 0 && item.type != GridFlowGraphItemType.Lock)
                        {
                            // Emit this item
                            var itemData = new GridFlowItemMetadata();
                            itemData.itemId = item.itemId;
                            itemData.itemType = item.type;
                            itemData.referencedItems = new List<System.Guid>(item.referencedItemIds).ToArray();

                            EmitMarker(item.markerName, markerTransform, new IntVector(x, 0, y), cellId, itemData);
                        }
                    }

                    bool removeElevationMarker = false;
                    if (cell.Overlay != null && cell.Overlay.markerName != null)
                    {
                        var heightOffset = 0.0f;
                        if (cell.Overlay.mergeConfig != null)
                        {
                            heightOffset += cell.LayoutCell
                                ? cell.Overlay.mergeConfig.markerHeightOffsetForLayoutTiles
                                : cell.Overlay.mergeConfig.markerHeightOffsetForNonLayoutTiles;
                        }

                        var height = cell.Height;
                        var overlayPosition = basePosition + Vector3.Scale(new Vector3(x + 0.5f, height + heightOffset, y + 0.5f), gridSize);
                        var overlayMarkerTransform = Matrix4x4.TRS(overlayPosition, Quaternion.identity, Vector3.one);
                        EmitMarker(cell.Overlay.markerName, overlayMarkerTransform, new IntVector(x, 0, y), cellId);

                        if (cell.Overlay.mergeConfig != null)
                        {
                            removeElevationMarker = cell.Overlay.mergeConfig.removeElevationMarker;
                        }
                    }

                    switch (cell.CellType)
                    {
                        case GridFlowTilemapCellType.Floor:
                            EmitMarker(GridFlowDungeonConstants.MarkerGround, markerTransform, new IntVector(x, 0, y), cellId);
                            break;

                        case GridFlowTilemapCellType.Wall:
                            EmitMarker(GridFlowDungeonConstants.MarkerWall, markerTransform, new IntVector(x, 0, y), cellId);
                            EmitMarker(GridFlowDungeonConstants.MarkerGround, markerTransform, new IntVector(x, 0, y), cellId);
                            break;

                        case GridFlowTilemapCellType.Door:
                            {
                                var doorMarker = GridFlowDungeonConstants.MarkerDoor;
                                var doorData = cell.Userdata as GridFlowTilemapCellDoorInfo;
                                if (doorData != null && doorData.oneWay)
                                {
                                    // One way door
                                    doorMarker = GridFlowDungeonConstants.MarkerDoorOneWay;

                                    // Apply the correct one-way direction
                                    var flipDirection = (doorData.nodeA.x > doorData.nodeB.x) || (doorData.nodeA.y > doorData.nodeB.y);
                                    if (!flipDirection)
                                    {
                                        var doorRotation = baseRotation * Quaternion.Euler(0, 180, 0);
                                        markerTransform = Matrix4x4.TRS(position, doorRotation, Vector3.one);
                                    }
                                }

                                GridFlowItemMetadata lockItemData = null;
                                if (cell.Item != System.Guid.Empty && itemMap.ContainsKey(cell.Item))
                                {
                                    var item = itemMap[cell.Item];
                                    if (item != null && item.type == GridFlowGraphItemType.Lock)
                                    {
                                        // Turn this into a locked door (lock marker will be spawned instead of a door (or a one way door)
                                        doorMarker = item.markerName;

                                        lockItemData = new GridFlowItemMetadata();
                                        lockItemData.itemId = item.itemId;
                                        lockItemData.itemType = item.type;
                                        lockItemData.referencedItems = new List<System.Guid>(item.referencedItemIds).ToArray();
                                    }
                                }

                                EmitMarker(doorMarker, markerTransform, new IntVector(x, 0, y), cellId, lockItemData);
                                EmitMarker(GridFlowDungeonConstants.MarkerGround, markerTransform, new IntVector(x, 0, y), cellId);
                            }
                            break;

                        case GridFlowTilemapCellType.Custom:
                            if (cell.CustomCellInfo != null && !removeElevationMarker)
                            {
                                var markerName = cell.CustomCellInfo.name;
                                var height = cell.Height;
                                var customPosition = basePosition + Vector3.Scale(new Vector3(x + 0.5f, height, y + 0.5f), gridSize);
                                var customMarkerTransform = Matrix4x4.TRS(customPosition, Quaternion.identity, Vector3.one);

                                EmitMarker(markerName, customMarkerTransform, new IntVector(x, 0, y), cellId);
                            }
                            break;

                    }
                }
            }
        }


        public override void DebugDraw()
        {
            
        }
    }

    public class GridFlowDungeonConstants
    {
        public static readonly string MarkerGround = "Ground";
        public static readonly string MarkerWall = "Wall";
        public static readonly string MarkerDoor = "Door";
        public static readonly string MarkerDoorOneWay = "DoorOneWay";

    }

}
