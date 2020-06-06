using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect.Graphs;

namespace DungeonArchitect.Grammar
{
    public interface IGrammarGraphBuildScript
    {
        void Generate(IGrammarGraphBuilder grammarBuilder);
    }
}
