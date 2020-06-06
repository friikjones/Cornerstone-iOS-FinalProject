﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Grammar
{
    public class GrammarNodeType : ScriptableObject
    {
        [SerializeField]
        public string nodeName;

        [SerializeField]
        public string description;

        [SerializeField]
        public Color nodeColor = new Color(0.2f, 0.3f, 0.3f);

        [SerializeField]
        [HideInInspector]
        public bool wildcard = false;
    }
}
