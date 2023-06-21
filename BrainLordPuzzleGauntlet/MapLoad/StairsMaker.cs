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
    public class StairsMaker : TileProcessor
    {
        List<string> doorTypeRegexes = new string[] {
            RegexUtil.makeRegexForNumberedLiteral("Up Stairs", 1),
            RegexUtil.makeRegexForNumberedLiteral("Down Stairs", 1),
        }.ToList();

        public bool supportsTile(string objTypeName)
        {
            foreach(string regex in doorTypeRegexes)
            {
                if(Regex.Match(objTypeName, regex).Success)
                {
                    return true;
                }
            }
            return false;
        }

        public byte processTile(string objType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            byte value = objType.Contains("Down") ? map.tileset["DownStairsFromTop"] : map.tileset["UpStairsPlatform"];

            Match m = null;
            foreach (String regex in doorTypeRegexes)
            {
                m = Regex.Match(objType, regex);
                if (m.Success)
                {
                    break;
                }
            }

            int doorNumber = Int32.Parse(m.Groups[1].Value);

            int doorX = x + map.mapPlacementOffsetX;
            int doorY = y + map.mapPlacementOffsetY;
            byte doorXValue = (byte)(doorX * 4);
            byte doorYValue = (byte)(doorY * 2);
            MapDoor door = new MapDoor();
            door.x = doorXValue;
            door.y = doorYValue;
            map.doors[doorNumber] = door;
            // no opening animation plus the stairs sound
            door.doorAnimation = 1;
            door.doorType = 3;

            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // post-process to add stairs leading to "up" stairways, and check for orientation of "down" stairways
            for(int y=0; y < 0x40 - 1; y++)
            {
                for (int x = 0; x < 0x40; x++)
                {
                    int sourceX = x - map.mapPlacementOffsetX;
                    int sourceY = y - map.mapPlacementOffsetY;
                    bool doorLoc = false;
                    // make sure we're changing an actual stairway door and not just another random tile that happens to be
                    // the same value
                    foreach(MapDoor door in map.doors.Values)
                    {
                        if(door.x == x * 4 && door.y == y * 2)
                        {
                            doorLoc = true;
                        }
                    }

                    if (doorLoc)
                    {
                        if (map.data[y * 0x40 + x] == map.tileset["UpStairsPlatform"])
                        {
                            if (TileDecorations.isFloor(tileTypeMap, sourceX, sourceY + 1))
                            {
                                map.data[(y + 1) * 0x40 + x] = map.tileset["UpStairs"];
                            }
                        }

                        if (map.data[y * 0x40 + x] == map.tileset["DownStairsFromTop"])
                        {
                            if (TileDecorations.isWall(tileTypeMap, sourceX, sourceY - 1))
                            {
                                map.data[y * 0x40 + x] = map.tileset["DownStairsFromBottom"];
                            }
                        }
                    }
                }
            }
        }
    }
}
