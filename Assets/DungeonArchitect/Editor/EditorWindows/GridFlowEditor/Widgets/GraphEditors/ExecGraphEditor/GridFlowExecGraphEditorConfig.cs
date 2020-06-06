using UnityEngine;
using DungeonArchitect.Builders.GridFlow;

namespace DungeonArchitect.Editors.GridFlow
{
    public class GridFlowExecGraphEditorConfig : ScriptableObject
    {
        public bool randomizeSeed = true;
        public int seed = 0;
        public GridFlowDungeonBuilder dungeonObject;
    }

}
