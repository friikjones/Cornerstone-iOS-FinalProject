using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec;
using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;

namespace DungeonArchitect.Builders.GridFlow
{
    [System.Serializable]
    public class DungeonGridFlowAsset : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        public GridFlowExecGraph execGraph;
    }

}
