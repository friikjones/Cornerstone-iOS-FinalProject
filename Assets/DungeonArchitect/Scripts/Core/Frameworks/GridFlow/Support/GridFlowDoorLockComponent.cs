using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowDoorLockComponent : MonoBehaviour
    {
        public string lockId;
        public string[] validKeyIds = new string[0];
        public GridFlowDoorKeyComponent[] validKeyRefs = new GridFlowDoorKeyComponent[0];
    }
}
