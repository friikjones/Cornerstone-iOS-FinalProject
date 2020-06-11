using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using DungeonArchitect.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [GridFlowExecNodeInfo("Initialize Tilemap", "Tilemap/", 2000)]
    public class GridFlowExecNodeHandler_InitializeTilemap : GridFlowExecNodeHandler
    {
        public int tilemapSizePerNode = 10;
        public float perturbAmount = 3;
        public float corridorLaneWidth = 2;
        public int layoutPadding = 0;

        public int caveAutomataNeighbors = 5;
        public int caveAutomataIterations = 4;
        public float caveThickness = 2.25f;

        public float roomColorSaturation = 0.3f;
        public float roomColorBrightness = 1.5f;

        private DoorInfo[] doors = new DoorInfo[0];
        private int nodeWidth;
        private int nodeHeight;

        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(execNode, context.NodeStates);

            if (graph == null)
            {
                errorMessage = "Missing graph input";
                ExecutionState = null;
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            IntVector2 abstractGridSize;
            if (!GetGraphSize(graph, out abstractGridSize))
            {
                errorMessage = "Invalid graph input";
                ExecutionState = null;
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            nodeWidth = abstractGridSize.x;
            nodeHeight = abstractGridSize.y;
            var tilemapWidth = nodeWidth * tilemapSizePerNode;
            var tilemapHeight = nodeHeight * tilemapSizePerNode;
            var tilemap = new GridFlowTilemap(tilemapWidth, tilemapHeight);
            tilemap = BuildTilemap(tilemap, graph, context.Random);

            ExecutionState = new GridFlowExecNodeState_Tilemap(tilemap, graph);

            return GridFlowExecNodeHandlerResultType.Success;
        }

        GridFlowTilemap BuildTilemap(GridFlowTilemap tilemap, GridFlowAbstractGraph graph, System.Random random)
        {
            var tileNodes = new GridFlowTilemapNodeInfo[nodeWidth, nodeHeight];
            for (int ny = 0; ny < nodeHeight; ny++)
            {
                for (int nx = 0; nx <nodeWidth; nx++)
                {
                    var x0 = nx * tilemapSizePerNode;
                    var y0 = ny * tilemapSizePerNode;
                    var x1 = x0 + tilemapSizePerNode;
                    var y1 = y0 + tilemapSizePerNode;
                    var node = new GridFlowTilemapNodeInfo(x0, y0, x1, y1);
                    tileNodes[nx, ny] = node;
                }
            }
            foreach (var node in graph.Nodes)
            {
                var coord = node.state.GridCoord;
                var tileNode = tileNodes[coord.x, coord.y];
                tileNode.node = node;
            }

            PerturbRoomSizes(tilemap, graph, tileNodes, random);
            FixCorridorSizes(tileNodes, graph);

            RasterizeRoomCorridors(tileNodes, tilemap);
            RasterizeBaseCaveBlocks(tileNodes, tilemap, graph);

            GenerateMainPath(tileNodes, tilemap, graph);
            BuildCaves(tileNodes, tilemap, graph, random);
            BuildDoors(tileNodes, tilemap, graph);

            tilemap = CropTilemap(tilemap);

            CalculateDistanceFromMainPath(tileNodes, tilemap, new GridFlowAbstractNodeRoomType[] { GridFlowAbstractNodeRoomType.Cave });
            CalculateDistanceFromMainPath(tileNodes, tilemap, new GridFlowAbstractNodeRoomType[] {
                GridFlowAbstractNodeRoomType.Room,
                GridFlowAbstractNodeRoomType.Corridor
            });
            CalculateDistanceFromMainPathOnEmptyArea(tilemap);

            DebugPostProcess(tileNodes, tilemap, graph);

            return tilemap;
        }

        GridFlowTilemap CropTilemap(GridFlowTilemap oldTilemap)
        {
            if (oldTilemap.Width == 0 || oldTilemap.Height == 0)
            {
                return oldTilemap;
            }

            int x0 = 0;
            int x1 = 0;
            int y0 = 0;
            int y1 = 0;
            bool foundFirstCell = false;
            foreach (var cell in oldTilemap.Cells)
            {
                bool layoutTile = cell.CellType == GridFlowTilemapCellType.Floor
                    || cell.CellType == GridFlowTilemapCellType.Wall
                    || cell.CellType == GridFlowTilemapCellType.Door;

                if (layoutTile)
                {
                    var x = cell.TileCoord.x;
                    var y = cell.TileCoord.y;
                    if (!foundFirstCell)
                    {
                        foundFirstCell = true;
                        x0 = x1 = x;
                        y0 = y1 = y;
                    }
                    else
                    {
                        x0 = Mathf.Min(x0, x);
                        x1 = Mathf.Max(x1, x);
                        y0 = Mathf.Min(y0, y);
                        y1 = Mathf.Max(y1, y);
                    }
                }
            }

            var p = layoutPadding;
            var layoutWidth = x1 - x0 + 1;
            var layoutHeight = y1 - y0 + 1;
            var tilemap = new GridFlowTilemap(
                    layoutWidth + p * 2,
                    layoutHeight + p * 2);

            for (int y = 0; y < layoutHeight; y++)
            {
                for (int x = 0; x < layoutWidth; x++)
                {
                    var ix = x + p;
                    var iy = y + p;

                    tilemap.Cells[ix, iy] = oldTilemap.Cells[x + x0, y + y0].Clone();
                    tilemap.Cells[ix, iy].TileCoord = new IntVector2(ix, iy);
                }
            }

            return tilemap;
        }

        #region Room / Corridor Generation Functions
        void PerturbRoomSizes(GridFlowTilemap tilemap, GridFlowAbstractGraph graph, GridFlowTilemapNodeInfo[,] tileNodes, System.Random random)
        {
            perturbAmount = Mathf.Min(perturbAmount, tilemapSizePerNode * 0.5f - corridorLaneWidth);
            perturbAmount = Mathf.Max(0, perturbAmount);
            // Perturb horizontally
            for (int ny = 0; ny < nodeHeight; ny++)
            {
                for (int nx = -1; nx < nodeWidth; nx++)
                {
                    var nodeA = (nx >= 0) ? tileNodes[nx, ny] : null;
                    var nodeB = (nx + 1 < nodeWidth) ? tileNodes[nx + 1, ny] : null;

                    bool connected = false;
                    if (nodeA != null && nodeB != null)
                    {
                        var link = graph.GetLink(nodeA.node, nodeB.node, true);
                        connected = (link != null && link.state.Directional);
                    }

                    if (connected)
                    {
                        float perturb = random.Range(-perturbAmount, perturbAmount);
                        if (nodeA != null) nodeA.x1 += perturb;
                        if (nodeB != null) nodeB.x0 += perturb;
                    }
                    else
                    {
                        float perturbA = perturbAmount * random.NextFloat();
                        float perturbB = perturbAmount * random.NextFloat();
                        if (nodeA != null) nodeA.x1 -= perturbA;
                        if (nodeB != null) nodeB.x0 += perturbB;
                    }
                }
            }

            // Perturb vertically
            for (int nx = 0; nx < nodeWidth; nx++)
            {
                for (int ny = -1; ny < nodeHeight; ny++)
                {
                    var nodeA = (ny >= 0) ? tileNodes[nx, ny] : null;
                    var nodeB = (ny + 1 < nodeHeight) ? tileNodes[nx, ny + 1] : null;

                    bool connected = false;
                    if (nodeA != null && nodeB != null)
                    {
                        var link = graph.GetLink(nodeA.node, nodeB.node, true);
                        connected = (link != null && link.state.Directional);
                    }

                    if (connected)
                    {
                        bool canMoveDown = (nodeA.x0 >= nodeB.x0 && nodeA.x1 <= nodeB.x1);
                        bool canMoveUp = (nodeB.x0 >= nodeA.x0 && nodeB.x1 <= nodeA.x1);

                        if (!canMoveUp && !canMoveDown) continue;

                        if (canMoveUp && canMoveDown)
                        {
                            // Move randomly on either one direction
                            if (random.NextFloat() < 0.5f)
                            {
                                canMoveUp = false;
                            }
                            else
                            {
                                canMoveDown = false;
                            }
                        }

                        float perturbDirection = (canMoveUp ? -1 : 1);
                        float perturb = random.NextFloat() * perturbAmount * perturbDirection;
                        nodeA.y1 += perturb;
                        nodeB.y0 += perturb;
                    }
                    else
                    {
                        float perturbA = perturbAmount * random.NextFloat();
                        float perturbB = perturbAmount * random.NextFloat();
                        if (nodeA != null) nodeA.y1 -= perturbA;
                        if (nodeB != null) nodeB.y0 += perturbB;
                    }
                }
            }
        }
        void FixCorridorSizes(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowAbstractGraph graph)
        {
            foreach (var tileNode in tileNodes)
            {
                var node = tileNode.node;
                if (node.state.RoomType == GridFlowAbstractNodeRoomType.Corridor)
                {
                    var incomingNodes = graph.GetIncomingNodes(node);
                    var outgoingNodes = graph.GetOutgoingNodes(node);
                    if (incomingNodes.Length == 0 || outgoingNodes.Length == 0) continue;

                    var incomingNode = incomingNodes[0];
                    var outgoingNode = outgoingNodes[0];
                    var inCoord = incomingNode.state.GridCoord;
                    var outCoord = outgoingNode.state.GridCoord;
                    var vertical = inCoord.x == outCoord.x;
                    if (vertical)
                    {
                        tileNode.x0 = Mathf.Max(tileNode.x0, tileNode.midX - corridorLaneWidth);
                        tileNode.x1 = Mathf.Min(tileNode.x1, tileNode.midX + corridorLaneWidth);
                    }
                    else
                    {
                        tileNode.y0 = Mathf.Max(tileNode.y0, tileNode.midY - corridorLaneWidth);
                        tileNode.y1 = Mathf.Min(tileNode.y1, tileNode.midY + corridorLaneWidth);
                    }
                }
            }
        }
        void RasterizeRoomCorridors(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap)
        {
            foreach (var tileNode in tileNodes)
            {
                if (!tileNode.node.state.Active) continue;
                var b = NodeTilemapBounds.Build(tileNode, tilemap.Width, tilemap.Height);

                if (tileNode.node.state.RoomType == GridFlowAbstractNodeRoomType.Cave)
                {
                    // Render the caves in another pass
                    continue;
                }

                for (int y = b.y0; y <= b.y1; y++)
                {
                    for (int x = b.x0; x <= b.x1; x++)
                    {
                        var cell = tilemap.Cells[x, y];
                        cell.LayoutCell = true;

                        cell.NodeCoord = tileNode.node.state.GridCoord;
                        cell.CellType = (x == b.x0 || x == b.x1 || y == b.y0 || y == b.y1)
                            ? GridFlowTilemapCellType.Wall
                            : GridFlowTilemapCellType.Floor;

                        var nodeColor = tileNode.node.state.Color;
                        cell.CustomColor = ColorUtils.BrightenColor(nodeColor, roomColorSaturation, roomColorBrightness);

                        if (cell.CellType == GridFlowTilemapCellType.Floor)
                        {
                            cell.UseCustomColor = true;
                        }

                        else if (cell.CellType == GridFlowTilemapCellType.Wall)
                        {
                            GridFlowTilemapCellWallInfo wallInfo = cell.Userdata as GridFlowTilemapCellWallInfo;
                            if (wallInfo == null)
                            {
                                wallInfo = new GridFlowTilemapCellWallInfo();
                                cell.Userdata = wallInfo;
                            }

                            wallInfo.owningNodes.Add(tileNode.node.state.GridCoord);
                        }
                    }
                }
            }
        }
        #endregion

        #region Cave Generation Functions
        private void RasterizeBaseCaveBlocks(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap, GridFlowAbstractGraph graph)
        {
            foreach (var tileNode in tileNodes)
            {
                if (!tileNode.node.state.Active) continue;
                var b = NodeTilemapBounds.Build(tileNode, tilemap.Width, tilemap.Height);

                if (tileNode.node.state.RoomType != GridFlowAbstractNodeRoomType.Cave)
                {
                    // Only build the caves in this pass
                    continue;
                }

                var caveNode = tileNode.node;
                var blockLeft = ShouldBlockCaveBoundary(graph, caveNode, -1, 0);
                var blockRight = ShouldBlockCaveBoundary(graph, caveNode, 1, 0);
                var blockTop = ShouldBlockCaveBoundary(graph, caveNode, 0, -1);
                var blockBottom = ShouldBlockCaveBoundary(graph, caveNode, 0, 1);
                for (int y = b.y0; y <= b.y1; y++)
                {
                    for (int x = b.x0; x <= b.x1; x++)
                    {
                        var cell = tilemap.Cells[x, y];
                        if (cell.CellType == GridFlowTilemapCellType.Empty)
                        {
                            var makeFloor = true;
                            if (blockLeft && x == b.x0) makeFloor = false;
                            if (blockRight && x == b.x1) makeFloor = false;
                            if (blockTop && y == b.y0) makeFloor = false;
                            if (blockBottom && y == b.y1) makeFloor = false;

                            if (makeFloor)
                            {
                                cell.NodeCoord = tileNode.node.state.GridCoord;
                                cell.CellType = GridFlowTilemapCellType.Floor;
                                cell.UseCustomColor = true;
                                var nodeColor = tileNode.node.state.Color;
                                cell.CustomColor = ColorUtils.BrightenColor(nodeColor, roomColorSaturation, roomColorBrightness);
                            }
                        }
                    }
                }
            }
        }
        private void BuildCaves(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap, GridFlowAbstractGraph graph, System.Random random)
        {
            CalculateDistanceFromMainPath(tileNodes, tilemap, new GridFlowAbstractNodeRoomType[] { GridFlowAbstractNodeRoomType.Cave });
            var caveMap = GenerateCaveBuildMap(tileNodes, tilemap, graph);
            BuildCaveStep_BuildRocks(caveMap, tilemap, random);
            BuildCaveStep_SimulateGrowth(caveMap, tilemap, random);
            BuildCaveStep_Cleanup(caveMap, tileNodes, tilemap);
        }
        private CaveCellBuildTile[,] GenerateCaveBuildMap(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap, GridFlowAbstractGraph graph)
        {
            var caveMap = new CaveCellBuildTile[tilemap.Width, tilemap.Height];

            foreach (var cell in tilemap.Cells)
            {
                var caveTile = new CaveCellBuildTile();
                caveTile.tileCoord = cell.TileCoord;
                var tileNode = tileNodes[cell.NodeCoord.x, cell.NodeCoord.y];
                caveTile.valid = (tileNode.node.state.RoomType == GridFlowAbstractNodeRoomType.Cave && tileNode.node.state.Active);
                caveMap[cell.TileCoord.x, cell.TileCoord.y] = caveTile;
            }

            return caveMap;
        }
        private void BuildCaveStep_BuildRocks(CaveCellBuildTile[,] caveMap, GridFlowTilemap tilemap, System.Random random)
        {
            foreach (var caveCell in caveMap)
            {
                var tileCell = tilemap.Cells[caveCell.tileCoord.x, caveCell.tileCoord.y];
                if (caveThickness > 0)
                {
                    var rockProbability = Mathf.Exp(-tileCell.DistanceFromMainPath / caveThickness);
                    caveCell.rockTile = random.NextFloat() < rockProbability;
                }
                else
                {
                    caveCell.rockTile = (tileCell.DistanceFromMainPath == 0);
                }
            }
        }
        private void BuildCaveStep_SimulateGrowth(CaveCellBuildTile[,] caveMap, GridFlowTilemap tilemap, System.Random random)
        {
            var width = caveMap.GetLength(0);
            var height = caveMap.GetLength(1);

            for (int i = 0; i < caveAutomataIterations; i++)
            {
                CaveCellBuildTile[,] oldMap = new CaveCellBuildTile[width, height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        oldMap[x, y] = caveMap[x, y].Clone();
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int nrocks = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                int nx = x + dx;
                                int ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                                if (oldMap[nx, ny].rockTile)
                                {
                                    nrocks++;
                                }
                            }
                        }


                        if (nrocks >= caveAutomataNeighbors)
                        {
                            caveMap[x, y].rockTile = true;
                        }
                    }
                }

            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (caveMap[x, y].valid && !caveMap[x, y].rockTile)
                    {
                        tilemap.Cells[x, y].CellType = GridFlowTilemapCellType.Empty;
                        tilemap.Cells[x, y].UseCustomColor = false;
                        caveMap[x, y].valid = false;
                    }
                }
            }
        }
        private void BuildCaveStep_Cleanup(CaveCellBuildTile[,] caveMap, GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap)
        {
            var width = tilemap.Width;
            var height = tilemap.Height;
            var traversibleCaveTiles = new bool[width, height];

            var childOffsets = new int[]
            {
                -1, 0,
                1, 0,
                0, -1,
                0, 1
            };

            foreach (var tileNode in tileNodes)
            {
                if (tileNode.node.state.RoomType != GridFlowAbstractNodeRoomType.Cave)
                {
                    // Only process the caves
                    continue;
                }

                if (!tileNode.node.state.Active)
                {
                    // Do not process inactive nodes
                    continue;
                }

                var tileCenter = NodeCoordToTileCoord(tileNode.node.state.GridCoord);

                if (traversibleCaveTiles[tileCenter.x, tileCenter.y])
                {
                    // Already processed from another adjacent node
                    continue;
                }

                // Flood fill from the center of this node
                var queue = new Queue<IntVector2>();
                queue.Enqueue(tileCenter);
                while (queue.Count > 0)
                {
                    var front = queue.Dequeue();
                    if (traversibleCaveTiles[front.x, front.y])
                    {
                        // Already processed
                        continue;
                    }

                    traversibleCaveTiles[front.x, front.y] = true;

                    // Traverse the children
                    for (int i = 0; i < 4; i++)
                    {
                        var childCoord = new IntVector2(
                            front.x + childOffsets[i * 2 + 0],
                            front.y + childOffsets[i * 2 + 1]);

                        if (childCoord.x >= 0 && childCoord.y >= 0 && childCoord.x < width && childCoord.y < height)
                        {
                            if (caveMap[childCoord.x, childCoord.y].valid)
                            {
                                var visited = traversibleCaveTiles[childCoord.x, childCoord.y];
                                if (!visited)
                                {
                                    queue.Enqueue(childCoord);
                                }
                            }
                        }
                    }
                }
            }

            // Assign the valid traversable paths 
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = tilemap.Cells[x, y];
                    var nodeCoord = cell.NodeCoord;
                    var tileNode = tileNodes[nodeCoord.x, nodeCoord.y];
                    if (tileNode.node.state.RoomType == GridFlowAbstractNodeRoomType.Cave)
                    {
                        var valid = traversibleCaveTiles[x, y];
                        caveMap[x, y].valid = valid;
                        if (!valid)
                        {
                            tilemap.Cells[x, y].CellType = GridFlowTilemapCellType.Empty;
                            tilemap.Cells[x, y].UseCustomColor = false;
                        }
                        else
                        {
                            tilemap.Cells[x, y].LayoutCell = true;
                        }
                    }
                }
            }
        }
        private void DebugPostProcess(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap, GridFlowAbstractGraph graph)
        {
            bool debugMainPathDistance = false;
            var width = tilemap.Width;
            var height = tilemap.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tileCell = tilemap.Cells[x, y];
                    var tileNode = tileNodes[tileCell.NodeCoord.x, tileCell.NodeCoord.y];
                    var node = tileNode.node.state;

                    if (debugMainPathDistance)
                    {
                        var startColor = new Color(1.0f, 0.4f, 0.4f);
                        var endColor = new Color(0.25f, 0.1f, 0.1f);
                        if (tileCell.CellType != GridFlowTilemapCellType.Empty)
                        {
                            if (node.RoomType == GridFlowAbstractNodeRoomType.Cave)
                            {
                                startColor = new Color(0.4f, 0.4f, 1.0f);
                                endColor = new Color(0.1f, 0.1f, 0.25f);
                            }
                            else if (node.RoomType == GridFlowAbstractNodeRoomType.Room || node.RoomType == GridFlowAbstractNodeRoomType.Corridor)
                            {
                                startColor = new Color(0.4f, 1.0f, 0.4f);
                                endColor = new Color(0.1f, 0.25f, 0.1f);
                            }
                        }

                        var distanceFactor = Mathf.Exp(-tileCell.DistanceFromMainPath / 5.0f);
                        var debugColor = Color.Lerp(endColor, startColor, distanceFactor);
                        tileCell.CustomColor = debugColor;
                        tileCell.UseCustomColor = true;
                    }
                }
            }
        }
        bool ShouldBlockCaveBoundary(GridFlowAbstractGraph graph, GridFlowAbstractGraphNode caveNode, int dx, int dy)
        {
            var coord = caveNode.state.GridCoord;
            var otherCoord = coord + new IntVector2(dx, dy);
            GridFlowAbstractGraphNode otherNode = null;
            foreach (var node in graph.Nodes)
            {
                if (node.state.GridCoord.Equals(otherCoord))
                {
                    otherNode = node;
                    break;
                }
            }
            if (otherNode == null || !otherNode.state.Active)
            {
                // a node in this location doesn't exist
                return false;
            }

            // Check if we have a link between these nodes. If we don't, then block it
            var link = graph.GetLink(caveNode, otherNode);
            if (link == null)
            {
                // No link exists. we should block this
                return true;
            }

            // We have a link to the other node.   block only if they it is a non-cave nodes
            return otherNode.state.RoomType != GridFlowAbstractNodeRoomType.Cave;
        }
        #endregion

        #region Common Functions
        private void GenerateMainPath(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap, GridFlowAbstractGraph graph)
        {
            foreach (var link in graph.Links)
            {
                var nodeA = graph.GetNodeState(link.Source);
                var nodeB = graph.GetNodeState(link.Destination);

                var tileCenterA = NodeCoordToTileCoord(nodeA.GridCoord);
                var tileCenterB = NodeCoordToTileCoord(nodeB.GridCoord);
                if (tileCenterA.x == tileCenterB.x)
                {
                    var x = tileCenterA.x;
                    int y0 = Mathf.Min(tileCenterA.y, tileCenterB.y);
                    int y1 = Mathf.Max(tileCenterA.y, tileCenterB.y);
                    for (int y = y0; y <= y1; y++)
                    {
                        var cell = tilemap.Cells[x, y];
                        cell.MainPath = true;
                        cell.DistanceFromMainPath = 0;
                    }
                }
                else if (tileCenterA.y == tileCenterB.y)
                {
                    var y = tileCenterA.y;
                    int x0 = Mathf.Min(tileCenterA.x, tileCenterB.x);
                    int x1 = Mathf.Max(tileCenterA.x, tileCenterB.x);
                    for (int x = x0; x <= x1; x++)
                    {
                        var cell = tilemap.Cells[x, y];
                        cell.MainPath = true;
                        cell.DistanceFromMainPath = 0;
                    }
                }
                else
                {
                    Debug.Log("invalid input");
                }
            }

            if (graph.Links.Count == 0 && graph.Nodes.Count == 1)
            {
                var node = graph.Nodes[0];
                var tc = NodeCoordToTileCoord(node.state.GridCoord);
                tilemap.Cells[tc.x, tc.y].MainPath = true;
                tilemap.Cells[tc.x, tc.y].DistanceFromMainPath = 0;

            }
        }
        private void CalculateDistanceFromMainPathOnEmptyArea(GridFlowTilemap tilemap)
        {
            var width = tilemap.Width;
            var height = tilemap.Height;
            var queue = new Queue<GridFlowTilemapCell>();

            var childOffsets = new int[]
            {
                -1, 0,
                1, 0,
                0, -1,
                0, 1
            };

            foreach (var cell in tilemap.Cells)
            {
                if (cell.CellType != GridFlowTilemapCellType.Empty)
                {
                    continue;
                }

                var validStartNode = false;

                for (int i = 0; i < 4; i++)
                {
                    int nx = cell.TileCoord.x + childOffsets[i * 2 + 0];
                    int ny = cell.TileCoord.y + childOffsets[i * 2 + 1];
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var ncell = tilemap.Cells[nx, ny];
                        if (ncell.CellType != GridFlowTilemapCellType.Empty)
                        {
                            validStartNode = true;
                            cell.DistanceFromMainPath = Mathf.Min(cell.DistanceFromMainPath, ncell.DistanceFromMainPath);
                        }
                    }
                }

                if (validStartNode)
                {
                    queue.Enqueue(cell);
                }
            }


            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var ndist = cell.DistanceFromMainPath + 1;

                for (int i = 0; i < 4; i++)
                {
                    int nx = cell.TileCoord.x + childOffsets[i * 2 + 0];
                    int ny = cell.TileCoord.y + childOffsets[i * 2 + 1];
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var ncell = tilemap.Cells[nx, ny];

                        if (ncell.CellType == GridFlowTilemapCellType.Empty)
                        {
                            if (ndist < ncell.DistanceFromMainPath)
                            {
                                ncell.DistanceFromMainPath = ndist;
                                queue.Enqueue(ncell);
                                
                            }
                        }
                    }
                }
            }
        }
        private void CalculateDistanceFromMainPath(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap, GridFlowAbstractNodeRoomType[] allowedRoomTypes)
        {
            var width = tilemap.Width;
            var height = tilemap.Height;
            var queue = new Queue<GridFlowTilemapCell>();

            foreach (var cell in tilemap.Cells)
            {
                var tileNode = tileNodes[cell.NodeCoord.x, cell.NodeCoord.y];
                var roomType = tileNode.node.state.RoomType;
                if (!allowedRoomTypes.Contains(roomType))
                {
                    continue;
                }

                if (cell.MainPath)
                {
                    queue.Enqueue(cell);
                }
            }

            var childOffsets = new int[]
            {
                -1, 0,
                1, 0,
                0, -1,
                0, 1
            };

            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();

                // Traverse the children
                var childDistance = tile.DistanceFromMainPath + 1;
                for (int i = 0; i < 4; i++)
                {
                    int nx = tile.TileCoord.x + childOffsets[i * 2 + 0];
                    int ny = tile.TileCoord.y + childOffsets[i * 2 + 1];
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var ncell = tilemap.Cells[nx, ny];
                        var ntileNode = tileNodes[ncell.NodeCoord.x, ncell.NodeCoord.y];
                        var nroomType = ntileNode.node.state.RoomType;
                        if (!allowedRoomTypes.Contains(nroomType))
                        {
                            continue;
                        }

                        if (childDistance < ncell.DistanceFromMainPath)
                        {
                            ncell.DistanceFromMainPath = childDistance;
                            queue.Enqueue(ncell);
                        }
                    }
                }
            }
        }
        void BuildDoors(GridFlowTilemapNodeInfo[,] tileNodes, GridFlowTilemap tilemap, GridFlowAbstractGraph graph)
        {
            // Build the doors
            var doorList = new List<DoorInfo>();
            foreach (var tileNode in tileNodes)
            {
                if (!tileNode.node.state.Active) continue;
                var b = NodeTilemapBounds.Build(tileNode, tilemap.Width, tilemap.Height);

                var node = tileNode.node;
                var nodeCoord = node.state.GridCoord;
                foreach (var link in graph.GetOutgoingLinks(tileNode.node))
                {
                    if (!link.state.Directional) continue;
                    GridFlowTilemapCell doorCell = null;

                    var otherNode = graph.GetNode(link.Destination);
                    if (node.state.RoomType == GridFlowAbstractNodeRoomType.Cave && otherNode.state.RoomType == GridFlowAbstractNodeRoomType.Cave)
                    {
                        // We don't need a door between two cave nodes
                        continue;
                    }

                    var otherCoord = otherNode.state.GridCoord;
                    if (nodeCoord.x == otherCoord.x)
                    {
                        // Vertical link
                        var y = (nodeCoord.y < otherCoord.y) ? b.y1 : b.y0;
                        doorCell = tilemap.Cells[b.mx, y];
                    }
                    else if (nodeCoord.y == otherCoord.y)
                    {
                        // Horizontal link
                        var x = (nodeCoord.x < otherCoord.x) ? b.x1 : b.x0;
                        doorCell = tilemap.Cells[x, b.my];
                    }

                    if (doorCell != null)
                    {
                        var sourceNode = graph.GetNode(link.Source);
                        var destNode = graph.GetNode(link.Destination);
                        doorCell.CellType = GridFlowTilemapCellType.Door;
                        var doorMeta = new GridFlowTilemapCellDoorInfo();
                        doorMeta.oneWay = link.state.OneWay;
                        doorMeta.nodeA = sourceNode.state.GridCoord;
                        doorMeta.nodeB = destNode.state.GridCoord;
                        int numLockedItems = link.state.Items.Count(i => i.type == GridFlowGraphItemType.Lock);
                        doorMeta.locked = numLockedItems > 0;
                        doorCell.Userdata = doorMeta;

                        var doorInfo = new DoorInfo();
                        doorInfo.Link = link;
                        doorInfo.CellCoord = doorCell.TileCoord;
                        doorList.Add(doorInfo);
                    }
                }
            }
            doors = doorList.ToArray();

            // Add door items
            foreach (var door in doors)
            {
                var items = door.Link.state.Items;
                var doorItem = items.Count > 0 ? items[0] : null;
                if (doorItem == null) continue;

                // TODO: Add metadata in the model with source/destination, one-way etc

                var cellCoord = door.CellCoord;
                var cell = tilemap.Cells[cellCoord.x, cellCoord.y];
                cell.Item = doorItem.itemId;
            }
        }
        #endregion

        #region Utility Functions
        private IntVector2 NodeCoordToTileCoord(IntVector2 nodeCoord)
        {
            var tileCoord = nodeCoord * tilemapSizePerNode;
            tileCoord += new IntVector2(tilemapSizePerNode, tilemapSizePerNode) / 2;
            return tileCoord;
        }
        bool GetGraphSize(GridFlowAbstractGraph graph, out IntVector2 size)
        {
            if (graph.Nodes.Count == 0)
            {
                size = IntVector2.Zero;
                return false;
            }

            int width = -int.MaxValue;
            int height = -int.MaxValue;

            foreach (var node in graph.Nodes)
            {
                width = Mathf.Max(width, node.state.GridCoord.x);
                height = Mathf.Max(height, node.state.GridCoord.y);
            }

            size = new IntVector2(width + 1, height + 1);
            return true;
        }
        GridFlowTilemapCellType GetCellType(GridFlowTilemap tilemap, int x, int y)
        {
            if (x < 0 || y < 0 || x >= tilemap.Width || y >= tilemap.Height)
            {
                return GridFlowTilemapCellType.Empty;
            }
            return tilemap.Cells[x, y].CellType;
        }
        #endregion

    }


    #region Data structures
    struct DoorInfo
    {
        public GridFlowAbstractGraphLink Link { get; set; }
        public IntVector2 CellCoord { get; set; }
    }

    struct NodeTilemapBounds
    {
        public int x0, y0, x1, y1, mx, my;
        public static NodeTilemapBounds Build(GridFlowTilemapNodeInfo tileNode, int tilemapWidth, int tilemapHeight)
        {
            var b = new NodeTilemapBounds();
            b.x0 = Mathf.FloorToInt(tileNode.x0);
            b.y0 = Mathf.FloorToInt(tileNode.y0);
            b.x1 = Mathf.FloorToInt(tileNode.x1);
            b.y1 = Mathf.FloorToInt(tileNode.y1);
            b.mx = Mathf.FloorToInt(tileNode.midX);
            b.my = Mathf.FloorToInt(tileNode.midY);


            b.x0 = Mathf.Clamp(b.x0, 0, tilemapWidth - 1);
            b.x1 = Mathf.Clamp(b.x1, 0, tilemapWidth - 1);
            b.y0 = Mathf.Clamp(b.y0, 0, tilemapHeight - 1);
            b.y1 = Mathf.Clamp(b.y1, 0, tilemapHeight - 1);
            b.mx = Mathf.Clamp(b.mx, 0, tilemapWidth - 1);
            b.my = Mathf.Clamp(b.my, 0, tilemapHeight - 1);

            return b;
        }
    }

    class CaveCellBuildTile
    {
        public IntVector2 tileCoord;
        public bool valid = false;
        public bool rockTile = false;

        public CaveCellBuildTile Clone()
        {
            var tile = new CaveCellBuildTile();
            tile.tileCoord = tileCoord;
            tile.valid = valid;
            tile.rockTile = rockTile;
            return tile;
        }
    }

    public class GridFlowTilemapNodeInfo
    {
        public GridFlowTilemapNodeInfo(float x0, float y0, float x1, float y1)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;

            midX = (x0 + x1) * 0.5f;
            midY = (y0 + y1) * 0.5f;
        }

        public float x0;
        public float x1;
        public float y0;
        public float y1;

        public float midX;
        public float midY;
        public GridFlowAbstractGraphNode node;
    }
    #endregion
}
