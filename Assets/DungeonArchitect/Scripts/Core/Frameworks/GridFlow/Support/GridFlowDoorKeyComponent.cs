using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowDoorKeyComponent : MonoBehaviour
    {
        public string keyId;
        public string[] validLockIds = new string[0];

        public GridFlowDoorLockComponent[] lockRefs;
    }
}
