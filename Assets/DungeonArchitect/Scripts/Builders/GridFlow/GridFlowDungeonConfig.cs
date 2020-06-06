using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowDungeonConfig : DungeonConfig
    {
        public DungeonGridFlowAsset flowAsset;
        public Vector3 gridSize = new Vector3(4, 4, 4);

        public override bool HasValidConfig(ref string errorMessage)
        {
            if (flowAsset == null)
            {
                errorMessage = "Flow Asset is not assign in the configuration";
                return false;
            }
            return true;
        }

    }
}
