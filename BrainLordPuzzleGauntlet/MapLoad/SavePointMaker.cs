using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.MapLoad
{
    /// <summary>
    /// Make save points
    /// </summary>
    public class SavePointMaker : TileProcessor
    {
        public bool supportsTile(string tileType)
        {
            return tileType.Contains("Save Point=");
        }

        public byte processTile(string objType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            int floorUnderObject = -1;
            string objectType = objType;
            if (objectType.Contains(":"))
            {
                string[] tokens = objectType.Split(new char[] { ':' });
                floorUnderObject = TileDecorations.chooseTile(map, tokens[0], tileTypeMap, x, y);
                objectType = tokens[1];
            }

            string saveName = objectType.Replace("Save Point=", "");
            byte value = floorUnderObject == -1 ? TileDecorations.chooseTile(map, "Floor", tileTypeMap, x, y) : (byte)floorUnderObject;
            MapObject mapObj = new MapObject();
            mapObj.x = (byte)((x + map.mapPlacementOffsetX) * 4);
            mapObj.y = (byte)((y + map.mapPlacementOffsetY) * 2);
            // $C8/C33E B7 88       LDA [$88],y[$D8:1D5F]   A:1DD8 X:0400 Y:0011 P:enVmxdizc
            mapObj.type = 0x31;
            mapObj.p1 = (byte)(puzzles.saveNum); // save num

            /*
            * savepoint doors for vanilla start here (loading saved game)
            * this actually points to a 6 byte structure, the first 3 bytes of which point to the door
            * savepoint 0 seemingly cannot be used? and will change to 1; we just leave its block empty
           $D4/0E93 A9 7B 5B    LDA #$5B7B              A:009C X:0580 Y:009C P:envmxdizc **
           $D4/0E96 85 82       STA $82    [$00:0082]   A:5B7B X:0580 Y:009C P:envmxdizc
           $D4/0E98 E2 20       SEP #$20                A:5B7B X:0580 Y:009C P:envmxdizc
           $D4/0E9A A9 C6       LDA #$C6                A:5B7B X:0580 Y:009C P:envMxdizc **
           $D4/0E9C 85 84       STA $84    [$00:0084]   A:5BC6 X:0580 Y:009C P:eNvMxdizc
           $D4/0E9E C2 20       REP #$20                A:5BC6 X:0580 Y:009C P:eNvMxdizc
           $D4/0EA0 B7 82       LDA [$82],y[$C6:5C17]   A:5BC6 X:0580 Y:009C P:eNvmxdizc
            */

            mapObj.p2 = 0;
            mapObj.p3 = 0;
            mapObj.p4 = 0;
            map.otherObjects.Add(mapObj);
            EntryPos entry = new EntryPos();
            entry.x = (byte)(x + map.mapPlacementOffsetX);
            entry.y = (byte)(y + map.mapPlacementOffsetY + 1);
            SaveLoadSpot saveLoadSpot = new SaveLoadSpot();
            saveLoadSpot.entryPos = entry;
            saveLoadSpot.name = saveName;
            map.saveLoadSpots[puzzles.saveNum] = saveLoadSpot;
            puzzles.saveNum++;
            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
