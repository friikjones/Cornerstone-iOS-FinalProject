using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    [GridFlowExecNodeInfo("Finalize Graph", "Layout Graph/", 1050)]
    public class GridFlowExecNodeHandler_FinalizeGraph : GridFlowExecNodeHandler
    {
        public bool debugDraw = false;
        public int oneWayDoorPromotionWeight = 0;


        struct ItemInfo
        {
            public ItemInfo(GridFlowItem item, GridFlowAbstractGraphNode node, GridFlowAbstractGraphLink link)
            {
                this.item = item;
                this.node = node;
                this.link = link;
            }

            public object GetParent()
            {
                if (node == null) return link;
                return node;
            }

            public GridFlowItem item;
            public GridFlowAbstractGraphNode node;
            public GridFlowAbstractGraphLink link;
        }

        public override GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode execNode, out GridFlowExecNodeState ExecutionState, ref string errorMessage)
        {
            var graph = GridFlowExecNodeUtils.CloneIncomingAbstractGraph(execNode, context.NodeStates);
            ExecutionState = new GridFlowExecNodeState_AbstractGraph(graph);

            if (graph == null)
            {
                errorMessage = "Missing graph input";
                return GridFlowExecNodeHandlerResultType.FailHalt;
            }

            var itemMap = new Dictionary<System.Guid, ItemInfo>();
            foreach (var node in graph.Nodes)
            {
                foreach (var item in node.state.Items)
                {
                    itemMap.Add(item.itemId, new ItemInfo(item, node, null));
                }
            }

            foreach (var link in graph.Links)
            {
                foreach (var item in link.state.Items)
                {
                    itemMap.Add(item.itemId, new ItemInfo(item, null, link));
                }
            }

            
            foreach (var entry in itemMap)
            {
                var itemInfo = entry.Value;
                var item = itemInfo.item;
                if (item.type == GridFlowGraphItemType.Key)
                {
                    var keyInfo = itemInfo;
                    var referencedLockItems = keyInfo.item.referencedItemIds.ToArray();
                    foreach (var lockId in referencedLockItems)
                    {
                        if (itemMap.ContainsKey(lockId))
                        {
                            var lockInfo = itemMap[lockId];
                            if (!ResolveKeyLocks(graph, keyInfo, lockInfo))
                            {
                                errorMessage = "Cannot resolve key-locks";
                                return GridFlowExecNodeHandlerResultType.FailRetry;
                            }
                        }
                    }
                }
            }

            var weights = GridFlowExecNodeUtils.CalculateWeights(graph, 10);
            if (debugDraw)
            {
                EmitDebugInfo(graph, weights);
            }

            // Make the links one directional if the difference in the source/dest nodes is too much
            foreach (var link in graph.Links)
            {
                if (!link.state.Directional) continue;
                var source = graph.GetNode(link.Source);
                var dest = graph.GetNode(link.Destination);
                if (source == null || dest == null) continue;
                if (!source.state.Active || !dest.state.Active) continue;

                int weightDiff = (weights[source] + 1) - weights[dest];
                if (weightDiff > oneWayDoorPromotionWeight)
                {
                    link.state.OneWay = true;
                }
            }

            // Remove undirected links
            var links = graph.Links.ToArray();
            foreach (var link in links)
            {
                if (!link.state.Directional)
                {
                    graph.RemoveLink(link);
                }
            }

            AssignRoomTypes(graph, context.Random);

            return GridFlowExecNodeHandlerResultType.Success;
        }

        void AssignRoomTypes(GridFlowAbstractGraph graph, System.Random random)
        {
            foreach (var node in graph.Nodes)
            {
                node.state.RoomType = GetNodeRoomType(graph, node);
            }

            // Make another pass and force assign rooms where a link requires a door
            foreach (var link in graph.Links)
            {
                bool containsLock = link.state.Items.Count(i => i.type == GridFlowGraphItemType.Lock) > 0;
                if (containsLock || link.state.OneWay)
                {
                    // We need atleast one room type that supports doors (rooms and corridors)
                    var nodeA = graph.GetNode(link.Source);
                    var nodeB = graph.GetNode(link.Destination);
                    var containsDoorA = (nodeA.state.RoomType == GridFlowAbstractNodeRoomType.Room || nodeA.state.RoomType == GridFlowAbstractNodeRoomType.Corridor);
                    var containsDoorB = (nodeB.state.RoomType == GridFlowAbstractNodeRoomType.Room || nodeB.state.RoomType == GridFlowAbstractNodeRoomType.Corridor);
                    if (!containsDoorA && !containsDoorB)
                    {
                        // promote one of them to a room
                        var nodeToPromote = (random.NextFloat() < 0.5f) ? nodeA : nodeB;
                        nodeToPromote.state.RoomType = GridFlowAbstractNodeRoomType.Room;
                    }
                }
            }
        }

        GridFlowAbstractNodeRoomType GetNodeRoomType(GridFlowAbstractGraph graph, GridFlowAbstractGraphNode node)
        {
            int numEnemies = node.state.Items.Count(i => i.type == GridFlowGraphItemType.Enemy);
            int numKeys = node.state.Items.Count(i => i.type == GridFlowGraphItemType.Key);
            int numBonus = node.state.Items.Count(i => i.type == GridFlowGraphItemType.Bonus);
            bool hasEntrance = node.state.Items.Count(i => i.type == GridFlowGraphItemType.Entrace) > 0;
            bool hasExit = node.state.Items.Count(i => i.type == GridFlowGraphItemType.Exit) > 0;

            var incoming = graph.GetIncomingLinks(node).ToArray();
            var outgoing = graph.GetOutgoingLinks(node).ToArray();

            if (hasEntrance || hasExit || numKeys > 0 || numBonus > 0)
            {
                return GridFlowAbstractNodeRoomType.Room;
            }

            if (incoming.Length == 1 && outgoing.Length == 1 && numEnemies == 0) 
            {
                // make sure the incoming and outgoing are in the same line
                var incomingNode = graph.GetNode(incoming[0].Source);
                var outgoingNode = graph.GetNode(outgoing[0].Destination);
                var coordIn = incomingNode.state.GridCoord;
                var coordOut = outgoingNode.state.GridCoord;

                var sameLine = (coordIn.x == coordOut.x || coordIn.y == coordOut.y);
                if (sameLine)
                {
                    return GridFlowAbstractNodeRoomType.Corridor;
                }
            }

            return numEnemies <= 3
                ? GridFlowAbstractNodeRoomType.Cave
                : GridFlowAbstractNodeRoomType.Room;
        }


        private void EmitDebugInfo(GridFlowAbstractGraph graph, Dictionary<GridFlowAbstractGraphNode, int> weights)
        {
            foreach (var entry in weights)
            {
                var node = entry.Key;
                var weight = entry.Value;

                var debugItem = new GridFlowItem();
                debugItem.type = GridFlowGraphItemType.Custom;
                debugItem.customInfo.itemType = "debug";
                debugItem.customInfo.text = weight.ToString();
                debugItem.customInfo.backgroundColor = new Color(0, 0, 0.3f);
                node.state.AddItem(debugItem);
            }
        }

        private bool ResolveKeyLocks(GridFlowAbstractGraph graph, ItemInfo keyInfo, ItemInfo lockInfo)
        {
            var keyItem = keyInfo.item;
            var lockItem = lockInfo.item;
            var lockNode = lockInfo.node;
            if (lockNode == null) return false;


            var incomingLinks = (from link in graph.GetIncomingLinks(lockNode)
                                where link.state.Directional
                                select link).ToArray();

            var outgoingLinks = (from link in graph.GetOutgoingLinks(lockNode)
                                 where link.state.Directional
                                 select link).ToArray();

            bool canLockIncoming = true;
            bool canLockOutgoing = true;
            if (incomingLinks.Length == 0)
            {
                canLockIncoming = false;
            }

            if (outgoingLinks.Length == 0)
            {
                canLockOutgoing = false;
            }

            var lockParent = lockInfo.GetParent();
            var keyParent = keyInfo.GetParent();
            if (lockParent == keyParent && lockParent != null)
            {
                canLockIncoming = false;
            }

            if (!canLockIncoming && !canLockOutgoing)
            {
                return false;
            }

            keyItem.referencedItemIds.Remove(lockItem.itemId);
            lockNode.state.Items.Remove(lockItem);

            GridFlowAbstractGraphLink[] linksToLock;
            if (canLockIncoming && canLockOutgoing)
            {
                // We can lock either the incoming or outgoing.  Choose the one that requires less links to be locked
                if (incomingLinks.Length == outgoingLinks.Length)
                {
                    linksToLock = incomingLinks;
                }
                else
                {
                    linksToLock = outgoingLinks.Length < incomingLinks.Length ? outgoingLinks : incomingLinks;
                }
            }
            else
            {
                linksToLock = canLockOutgoing ? outgoingLinks : incomingLinks;
            }
            foreach (var link in linksToLock)
            {
                var linkLock = lockItem.Clone();
                linkLock.itemId = System.Guid.NewGuid();
                link.state.AddItem(linkLock);
                keyItem.referencedItemIds.Add(linkLock.itemId);
            }

            return true;
        }
    }
}
