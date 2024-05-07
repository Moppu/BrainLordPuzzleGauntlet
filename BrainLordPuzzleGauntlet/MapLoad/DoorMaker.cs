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
    /// Create switch-opened, or normal unlocked doors
    /// <para>
    ///   supports objects:
    ///     <para>"Weighted Door (n)" - Door requiring several "Weighted Switch" to be pressed simultaneously to open</para>
    ///     <para>  (n) = A unique numeric door ID</para>
    ///     <para>"Toggle Door (n)" - Door requiring several "Toggle Switch" to be pressed in the correct order to open</para>
    ///     <para>  (n) = A unique numeric door ID</para>
    ///     <para>"Locked Door (n)>(k)" - Door you have to have key (k) to open</para>
    ///     <para>  (n) = A unique numeric door ID</para>
    ///     <para>  (k) = The numeric key ID</para>
    ///     <para>"Unlocked Door (n)" - Door you can enter any time</para>
    ///     <para>  (n) = A unique numeric door ID</para>
    ///     <para>"Floor Warper (n)" - A spot on the floor you step on to transition maps</para>
    ///     <para>  (n) = A unique numeric door ID</para>
    /// </para>
    /// </summary>
    public class DoorMaker : TileProcessor
    {
        private static Dictionary<string, DoorInfo> objectTypes = new Dictionary<string, DoorInfo>();

        private class DoorInfo
        {
            public int objValue;
            public DoorInfo(int objValue)
            {
                this.objValue = objValue;
            }
        }

        static DoorMaker()
        {
            objectTypes[RegexUtil.makeRegexForNumberedLiteral("Weighted Door", 1)] = new DoorInfo(0x0B);
            objectTypes[RegexUtil.makeRegexForNumberedLiteral("Toggle Door", 1)] = new DoorInfo(0x23);
            objectTypes[RegexUtil.makeOpenEndedRegexForNumberedLiteral("Locked Door", 1) + ">\\d\\d?$"] = new DoorInfo(0x05);
            objectTypes[RegexUtil.makeRegexForNumberedLiteral("Unlocked Door", 1)] = new DoorInfo(-1);
            objectTypes[RegexUtil.makeRegexForNumberedLiteral("Floor Warper", 1)] = new DoorInfo(-1);
        }

        private bool match(string objTypeName)
        {
            foreach (String regex in objectTypes.Keys)
            {
                if (Regex.Match(objTypeName, regex).Success)
                {
                    return true;
                }
            }
            return false;
        }
        public bool supportsTile(string objTypeName)
        {
            if (match(objTypeName))
            {
                return true;
            }
            return false;
        }

        public byte processTile(string objType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            DoorInfo objSettings = null;
            Match m = null;
            foreach (String regex in objectTypes.Keys)
            {
                m = Regex.Match(objType, regex);
                if (m.Success)
                {
                    objSettings = objectTypes[regex];
                    break;
                }
            }

            int doorNumber = Int32.Parse(m.Groups[1].Value);
            bool foundTransition = false;

            foreach (DoorTransition dt in map.connections.Keys)
            {
                if (dt.sourceDoorId == doorNumber)
                {
                    foundTransition = true;
                }
            }
            byte value = puzzles.practiceMode ? TileDecorations.chooseTile(map, "Door", tileTypeMap, x, y) : map.tileset["ShutteredDoor"];
            if(objSettings.objValue == -1)
            {
                if (objType.Contains("Warper"))
                {
                    value = map.tileset["FloorWarper"];
                }
                else
                {
                    // no object = normal, unlocked door
                    value = TileDecorations.chooseTile(map, "Door", tileTypeMap, x, y);
                }
            }
            int doorX = x + map.mapPlacementOffsetX;
            int doorY = y + map.mapPlacementOffsetY;
            byte doorXValue = (byte)(doorX * 4);
            byte doorYValue = (byte)(doorY * 2);
            MapDoor door = new MapDoor();
            door.x = doorXValue;
            door.y = doorYValue;
            map.doors[doorNumber] = door;
            door.doorAnimation = map.tileset["DoorOpenValue"];

            if (objType.Contains("Locked"))
            {
                value = map.tileset["LockedDoor"];
                // pull key num from name
                if(objType.Contains(">"))
                {
                    string[] objTokens = objType.Split(new char[] { '>' });
                    door.key = Int32.Parse(objTokens[1]);
                }
                else
                {
                    throw new MapLoadException(map, "Locked door must specify a key with Locked Door>KeyNum");
                }
            }

            door.doorType = 4;
            if (objSettings.objValue != -1)
            {
                // object that the switches trigger to use a different tile, by x/y
                MapObject mapObj = new MapObject();
                mapObj.x = doorXValue;
                mapObj.y = doorYValue;
                mapObj.type = (byte)objSettings.objValue;
                if (objType.Contains("Locked"))
                {
                    // key num .. without this, the door re-closes
                    mapObj.p1 = (byte)door.key;
                    door.doorType = 5;
                }
                else
                {
                    mapObj.p1 = map.tileset["DoorOpenForObject"];
                    if (door.noTransition)
                    {
                        door.doorType = 0;
                    }
                    // no door opening animation when walking through
                    if (!puzzles.practiceMode)
                    {
                        door.doorAnimation = 0;
                        door.doorType = 3;
                    }
                }
                map.otherObjects.Add(mapObj);
                door.noTransition = !foundTransition;
            }
            else
            {
                // no door opening animation when walking through
                if (value == map.tileset["DoorSocket"] || objType.Contains("Warper"))
                {
                    door.doorAnimation = 0;
                    door.doorType = 3;
                }
            }

            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
