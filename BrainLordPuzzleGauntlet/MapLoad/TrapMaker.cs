using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.MapLoad
{
    /// <summary>
    /// Make floor traps
    /// </summary>
    public class TrapMaker : TileProcessor
    {
        public bool supportsTile(string objTypeName)
        {
            return objTypeName == "Trap";
        }

        public byte processTile(string objType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            byte value = map.tileset["Trap"];
            MapObject mapObj = new MapObject();
            mapObj.x = (byte)((x + map.mapPlacementOffsetX) * 4);
            mapObj.y = (byte)((y + map.mapPlacementOffsetY) * 2);
            mapObj.type = 0x0C;
            mapObj.p1 = map.tileset["TrapForObject"];
            mapObj.p2 = 0;
            mapObj.p3 = 0;
            mapObj.p4 = 0;

            map.otherObjects.Add(mapObj);
            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
