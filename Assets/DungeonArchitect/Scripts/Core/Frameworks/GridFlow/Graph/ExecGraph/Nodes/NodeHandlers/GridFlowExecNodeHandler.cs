using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers
{
    public enum GridFlowExecNodeHandlerResultType
    {
        Success,
        FailRetry,
        FailHalt
    }

    public abstract class GridFlowExecNodeHandler : ScriptableObject
    {
        //public GridFlowExecNodeState ExecutionState { get; set; }
        public abstract GridFlowExecNodeHandlerResultType Execute(GridFlowExecutionContext context, GridFlowExecRuleGraphNode node, out GridFlowExecNodeState ExecutionState, ref string errorMessage);
        public string description = "";
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class GridFlowExecNodeInfoAttribute : System.Attribute
    {
        public string Title { get; private set; }
        public string MenuPrefix { get; private set; }
        public float Weight { get; private set; }

        public GridFlowExecNodeInfoAttribute(string title)
            : this(title, "", 0)
        {
        }

        public GridFlowExecNodeInfoAttribute(string title, string menuPrefix)
            : this(title, menuPrefix, 0)
        {
        }

        public GridFlowExecNodeInfoAttribute(string title, string menuPrefix, float weight)
        {
            this.Title = title;
            this.MenuPrefix = menuPrefix;
            this.Weight = weight;
        }

        public static GridFlowExecNodeInfoAttribute GetHandlerAttribute(System.Type type)
        {
            if (type == null) return null;
            return type.GetCustomAttributes(typeof(GridFlowExecNodeInfoAttribute), true).FirstOrDefault() as GridFlowExecNodeInfoAttribute;
        }
    }

    public class GridFlowExecNodeStates
    {
        public void Clear()
        {
            stateByNodeId.Clear();
        }

        public void Register(string nodeId, GridFlowExecNodeState state)
        {
            if (state != null)
            {
                stateByNodeId[nodeId] = state;
            }
        }

        public GridFlowExecNodeState Get(string nodeId)
        {
            if (stateByNodeId.ContainsKey(nodeId))
            {
                return stateByNodeId[nodeId];
            }
            return null;
        }

        private Dictionary<string, GridFlowExecNodeState> stateByNodeId = new Dictionary<string, GridFlowExecNodeState>();
    }

    public abstract class GridFlowExecNodeState
    {
        public GridFlowAbstractGraph AbstractGraph { get; protected set; }
        public GridFlowTilemap Tilemap { get; protected set; }
    }

    public class GridFlowExecNodeState_AbstractGraph : GridFlowExecNodeState
    {
        public GridFlowExecNodeState_AbstractGraph(GridFlowAbstractGraph graph)
        {
            this.AbstractGraph = graph;
        }
    }

    public class GridFlowExecNodeState_Tilemap : GridFlowExecNodeState
    {
        public GridFlowExecNodeState_Tilemap(GridFlowTilemap tilemap, GridFlowAbstractGraph abstractGraph)
        {
            this.Tilemap = tilemap;
            this.AbstractGraph = abstractGraph;
        }
    }
}
