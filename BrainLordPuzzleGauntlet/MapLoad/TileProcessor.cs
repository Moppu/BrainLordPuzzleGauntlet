using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.MapLoad
{
    /// <summary>
    /// Processor for map tile type into game tile & objects
    /// </summary>
    public interface TileProcessor
    {
        bool supportsTile(string tileType);
        byte processTile(string objType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y);
        void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap);
    }
}
