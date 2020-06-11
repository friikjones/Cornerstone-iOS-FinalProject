using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Tilemap
{
    [System.Serializable]
    public enum GridFlowTilemapCellType
    {
        Empty,
        Floor,
        Wall,
        Door,
        Custom
    }

    [System.Serializable]
    public class GridFlowTilemapCustomCellInfo
    {
        public string name;
        public Color defaultColor = Color.white;

        public override string ToString()
        {
            if (name.Length == 0)
            {
                return base.ToString();
            }
            return name;
        }
    }

    [System.Serializable]
    public enum GridFlowTilemapCellCategory
    {
        Layout,
        Biome,
        Elevation
    }

    [System.Serializable]
    public class GridFlowTilemapCellOverlay
    {
        public string markerName;
        public Color color;
        public float noiseValue { get; set; }

        /// <summary>
        /// Specifies if the overlay blocks the tile (like a rock overlay) or doesn't block a tile (like grass overlay), so items can be placed on top of it
        /// </summary>
        public bool tileBlockingOverlay = true;

        public GridFlowTilemapCellOverlayMergeConfig mergeConfig;

        public GridFlowTilemapCellOverlay Clone()
        {
            var newOverlay = new GridFlowTilemapCellOverlay();
            newOverlay.markerName = markerName;
            newOverlay.color = color;
            newOverlay.noiseValue = noiseValue;
            newOverlay.tileBlockingOverlay = tileBlockingOverlay;
            newOverlay.mergeConfig = mergeConfig.Clone();

            return newOverlay;
        }
    }

    [System.Serializable]
    public enum GridFlowTilemapCellOverlayMergeWallOverlayRule
    {
        KeepWallAndOverlay,
        KeepWallRemoveOverlay,
        KeepOverlayRemoveWall
    }

    [System.Serializable]
    public class GridFlowTilemapCellOverlayMergeConfig
    {
        public float minHeight = 0;
        public float maxHeight = 0;
        public GridFlowTilemapCellOverlayMergeWallOverlayRule wallOverlayRule = GridFlowTilemapCellOverlayMergeWallOverlayRule.KeepWallAndOverlay;
        public float markerHeightOffsetForLayoutTiles = 0;
        public float markerHeightOffsetForNonLayoutTiles = 0;
        public bool removeElevationMarker = false;

        public GridFlowTilemapCellOverlayMergeConfig Clone()
        {
            var newConfig = new GridFlowTilemapCellOverlayMergeConfig();
            newConfig.minHeight = minHeight;
            newConfig.maxHeight = maxHeight;
            newConfig.wallOverlayRule = wallOverlayRule;
            newConfig.markerHeightOffsetForLayoutTiles = markerHeightOffsetForLayoutTiles;
            newConfig.markerHeightOffsetForNonLayoutTiles = markerHeightOffsetForNonLayoutTiles;
            newConfig.removeElevationMarker = removeElevationMarker;
            return newConfig;
        }
    }

    [System.Serializable]
    public class GridFlowTilemapCell
    {
        public GridFlowTilemapCellType CellType = GridFlowTilemapCellType.Empty;
        public GridFlowTilemapCustomCellInfo CustomCellInfo = null;
        public System.Guid Item = System.Guid.Empty;
        public string[] Tags = new string[0];
        public GridFlowTilemapCellOverlay Overlay;
        public IntVector2 NodeCoord;
        public IntVector2 TileCoord;
        public bool UseCustomColor = false;
        public Color CustomColor = Color.white;
        public bool MainPath = false;
        public bool LayoutCell = false;
        public int DistanceFromMainPath = int.MaxValue;
        public float Height = 0;
        public object Userdata = null;

        public GridFlowTilemapCell Clone()
        {
            var newCell = new GridFlowTilemapCell();
            newCell.CellType = CellType;
            newCell.CustomCellInfo = CustomCellInfo;
            newCell.Item = Item;
            newCell.Tags = new List<string>(Tags).ToArray();
            newCell.Overlay = (Overlay != null) ? Overlay.Clone() : null;
            newCell.NodeCoord = NodeCoord;
            newCell.TileCoord = TileCoord;
            newCell.UseCustomColor = UseCustomColor;
            newCell.CustomColor = CustomColor;
            newCell.MainPath = MainPath;
            newCell.LayoutCell = LayoutCell;
            newCell.DistanceFromMainPath = DistanceFromMainPath;
            newCell.Height = Height;

            if (Userdata != null && Userdata is System.ICloneable)
            {
                newCell.Userdata = (Userdata as System.ICloneable).Clone();
            }
            return newCell;
        }

        public void Clear()
        {
            CellType = GridFlowTilemapCellType.Empty;
            CustomCellInfo = null;
            Item = System.Guid.Empty;
            Tags = new string[0];
            Overlay = null;
            UseCustomColor = false;
            MainPath = false;
            LayoutCell = false;
            DistanceFromMainPath = int.MaxValue;
            Height = 0;
            Userdata = null;
        }
    }

    [System.Serializable]
    public class GridFlowTilemapCellDoorInfo : System.ICloneable
    {
        public bool locked = false;
        public bool oneWay = false;
        public IntVector2 nodeA;
        public IntVector2 nodeB;

        public object Clone()
        {
            var newObj = new GridFlowTilemapCellDoorInfo();
            newObj.locked = locked;
            newObj.oneWay = oneWay;
            newObj.nodeA = nodeA;
            newObj.nodeB = nodeB;
            return newObj;
        }
    }

    [System.Serializable]
    public class GridFlowTilemapCellWallInfo : System.ICloneable
    {
        public List<IntVector2> owningNodes = new List<IntVector2>();

        public object Clone()
        {
            var newObj = new GridFlowTilemapCellWallInfo();
            newObj.owningNodes = new List<IntVector2>(owningNodes);
            return newObj;
        }
    }

    // Disabling serialization as this is making things slow
    // TODO: Fix serialization issue
    //[System.Serializable]
    public class GridFlowTilemap
    {
        public int Width;
        public int Height;

        [SerializeField]
        public GridFlowTilemapCellDatabase Cells;

        public GridFlowTilemap(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            Cells = new GridFlowTilemapCellDatabase(Width, Height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new GridFlowTilemapCell();
                    cell.TileCoord = new IntVector2(x, y);
                    Cells[x, y] = cell;
                }
            }
        }

        public GridFlowTilemap Clone()
        {
            var newTilemap = new GridFlowTilemap(Width, Height);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newTilemap.Cells[x, y] = Cells[x, y].Clone();
                }
            }
            return newTilemap;
        }

    }

    public class GridFlowTilemapCellDatabaseEnumerator : IEnumerator<GridFlowTilemapCell>
    {
        int position = -1;
        GridFlowTilemapCell[] cells = null;
        GridFlowTilemapCell current;

        public GridFlowTilemapCellDatabaseEnumerator(GridFlowTilemapCell[] cells)
        {
            this.cells = cells;
        }

        public void Dispose()
        {
            cells = null;
            current = null;
        }

        public bool MoveNext()
        {
            if (++position >= cells.Length)
            {
                return false;
            }

            // Set current box to next item in collection.
            current = cells[position];
            return true;
        }

        public void Reset()
        {
            position = -1;
            current = null;
        }

        public GridFlowTilemapCell Current
        {
            get { return current; }
        }


        object IEnumerator.Current
        {
            get { return Current; }
        }
    }

    /// <summary>
    /// This class gives a 2D grid view for an underlying 1D array
    /// Unity serialization requires a 1-dimensional array, hence the need for this class
    /// </summary>
    [System.Serializable]
    public class GridFlowTilemapCellDatabase : IEnumerable<GridFlowTilemapCell>
    {
        [SerializeField]
        private GridFlowTilemapCell[] cells;

        [SerializeField]
        private int width;

        [SerializeField]
        private int height;

        public GridFlowTilemapCellDatabase(int width, int height)
        {
            this.width = width;
            this.height = height;
            cells = new GridFlowTilemapCell[width * height];
        }

        public GridFlowTilemapCell this[int x, int y]
        {
            get
            {
                return cells[Index(x, y)];
            }
            set
            {
                cells[Index(x, y)] = value;
            }
        }

        public GridFlowTilemapCell GetCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return null;
            return this[x, y];
        }

        private int Index(int x, int y)
        {
            return y * width + x;
        }

        IEnumerator<GridFlowTilemapCell> IEnumerable<GridFlowTilemapCell>.GetEnumerator()
        {
            return new GridFlowTilemapCellDatabaseEnumerator(cells);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new GridFlowTilemapCellDatabaseEnumerator(cells);
        }
    }


    public class GridFlowTilemapDistanceFieldCell
    {
        public int DistanceFromEdge = int.MaxValue;
        public int DistanceFromDoor = int.MaxValue;
    }

    public class GridFlowTilemapDistanceField
    {
        GridFlowTilemap tilemap;
        public GridFlowTilemapDistanceFieldCell[,] distanceCells;

        public GridFlowTilemapDistanceField(GridFlowTilemap tilemap)
        {
            this.tilemap = tilemap;
            distanceCells = new GridFlowTilemapDistanceFieldCell[tilemap.Width, tilemap.Height];
            for (int y = 0; y < tilemap.Height; y++)
            {
                for (int x = 0; x < tilemap.Width; x++)
                {
                    distanceCells[x, y] = new GridFlowTilemapDistanceFieldCell();
                }
            }


            Build();
        }

        private static int[] childOffsets = new int[]
        {
                -1, 0,
                1, 0,
                0, -1,
                0, 1
        };

        void Build()
        {
            FindDistanceFromEdge();
            FindDistanceFromDoor();
        }

        void FindDistanceFromEdge()
        {
            var queue = new Queue<GridFlowTilemapCell>();
            for (int y = 0; y < tilemap.Height; y++)
            {
                for (int x = 0; x < tilemap.Width; x++)
                {
                    var cell = tilemap.Cells[x, y];
                    if (cell.CellType == GridFlowTilemapCellType.Floor)
                    {
                        bool allNeighborsWalkable = true;
                        for (int i = 0; i < 4; i++)
                        {
                            var cx = x + childOffsets[i * 2 + 0];
                            var cy = y + childOffsets[i * 2 + 1];
                            var ncell = tilemap.Cells.GetCell(cx, cy);
                            if (ncell == null) continue;

                            if (ncell.CellType != GridFlowTilemapCellType.Floor)
                            {
                                allNeighborsWalkable = false;
                                break;
                            }

                            // Check if there's a blocking overlay
                            if (cell.Overlay != null && cell.Overlay.tileBlockingOverlay)
                            {
                                allNeighborsWalkable = false;
                                break;
                            }
                        }

                        if (!allNeighborsWalkable)
                        {
                            queue.Enqueue(cell);
                            distanceCells[x, y].DistanceFromEdge = 0;
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();

                var x = cell.TileCoord.x;
                var y = cell.TileCoord.y;
                var ndist = distanceCells[x, y].DistanceFromEdge + 1;

                for (int i = 0; i < 4; i++)
                {
                    var nx = x + childOffsets[i * 2 + 0];
                    var ny = y + childOffsets[i * 2 + 1];
                    var ncell = tilemap.Cells.GetCell(nx, ny);
                    if (ncell == null) continue;
                    var walkableTile = (ncell.CellType == GridFlowTilemapCellType.Floor);
                    if (walkableTile && cell.Overlay != null && cell.Overlay.tileBlockingOverlay)
                    {
                        walkableTile = false;
                    }

                    if (walkableTile && ndist < distanceCells[nx, ny].DistanceFromEdge)
                    {
                        distanceCells[nx, ny].DistanceFromEdge = ndist;
                        queue.Enqueue(ncell);
                    }
                }
            }
        }

        void FindDistanceFromDoor()
        {

            var queue = new Queue<GridFlowTilemapCell>();
            for (int y = 0; y < tilemap.Height; y++)
            {
                for (int x = 0; x < tilemap.Width; x++)
                {
                    var cell = tilemap.Cells[x, y];
                    if (cell.CellType == GridFlowTilemapCellType.Door)
                    {
                        queue.Enqueue(cell);
                        distanceCells[x, y].DistanceFromDoor = 0;
                    }
                }
            }

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();

                var x = cell.TileCoord.x;
                var y = cell.TileCoord.y;
                var ndist = distanceCells[x, y].DistanceFromDoor + 1;

                for (int i = 0; i < 4; i++)
                {
                    var nx = x + childOffsets[i * 2 + 0];
                    var ny = y + childOffsets[i * 2 + 1];
                    var ncell = tilemap.Cells.GetCell(nx, ny);
                    if (ncell == null) continue;
                    var walkableTile = (ncell.CellType == GridFlowTilemapCellType.Floor);
                    if (walkableTile && cell.Overlay != null && cell.Overlay.tileBlockingOverlay)
                    {
                        walkableTile = false;
                    }

                    if (walkableTile && ndist < distanceCells[nx, ny].DistanceFromDoor)
                    {
                        distanceCells[nx, ny].DistanceFromDoor = ndist;
                        queue.Enqueue(ncell);
                    }
                }
            }
        }
    }
}
