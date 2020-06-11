using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers;

namespace DungeonArchitect.Editors.GridFlow
{
    public class GridFlowExecRuleNodeRenderer : GridFlowExecNodeRendererBase
    {
        protected override string GetDescText(GraphNode node)
        {
            var handler = GetHandler(node);
            return (handler != null) ? handler.description : "";
        }

        protected override string GetCaption(GraphNode node)
        {
            var handler = GetHandler(node);
            var menuAttribute = handler != null ? GridFlowExecNodeInfoAttribute.GetHandlerAttribute(handler.GetType()) : null;
            return (menuAttribute != null) ? menuAttribute.Title : "[INVALID]";
        }

        protected override Color GetPinColor(GraphNode node)
        {
            var handler = GetHandler(node);
            return (handler != null) ? new Color(0.3f, 0.3f, 0.5f) : Color.red;
        }

        protected override Color GetBodyColor(GraphNode node)
        {
            var handler = GetHandler(node);
            return (handler != null) ? new Color(0.1f, 0.1f, 0.1f, 1) : Color.red;
        }
    }
}
