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
    /// Make floor switches for assorted functions
    /// </summary>
    public class SwitchMaker : TileProcessor
    {
        private static Dictionary<string, SwitchInfo> objectTypes = new Dictionary<string, SwitchInfo>();
        private class SwitchInfo
        {
            public byte objValue;
            public int doorGroup;
            public int switchGroup;
            public int iceGroup;
            public SwitchInfo(byte objValue, int doorGroup, int switchGroup, int iceGroup)
            {
                this.objValue = objValue;
                this.doorGroup = doorGroup;
                this.switchGroup = switchGroup;
                this.iceGroup = iceGroup;
            }
        }

        static SwitchMaker()
        {
            objectTypes[RegexUtil.makeRegexForNumberedLiteral("Weighted Switch", 1)] = new SwitchInfo(0x0A, 1, -1, -1);
            objectTypes[RegexUtil.makeRegexForNumberedLiteral("Falling Object Switch", 1)] = new SwitchInfo(0x13, -1, -1, 1);
            objectTypes[RegexUtil.makeRegexForNumberedLiteral("Toggle Switch", 2)] = new SwitchInfo(0x22, 1, 2, -1);
            objectTypes["Light Switch"] = new SwitchInfo(0x0D, -1, -1, -1);
        }

        private bool match(string objTypeName)
        {
            foreach(String regex in objectTypes.Keys)
            {
                if(Regex.Match(objTypeName, regex).Success)
                {
                    return true;
                }
            }
            return false;
        }
        public bool supportsTile(string objTypeName)
        {
            if(match(objTypeName))
            {
                return true;
            }
            return false;
        }

        public byte processTile(string objType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            SwitchInfo objSettings = null;

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

            byte value = map.tileset["Switch2"]; // round switch on ruins maps
            MapObject mapObj = new MapObject();
            mapObj.x = (byte)((x + map.mapPlacementOffsetX) * 4);
            mapObj.y = (byte)((y + map.mapPlacementOffsetY) * 2);
            mapObj.type = objSettings.objValue;
            mapObj.p1 = 0;
            mapObj.p2 = 0; // tbd ice pos y/2
            mapObj.p3 = 0; // tbd ice pos x/4
            mapObj.p4 = map.tileset["SwitchPressedForObject"];

            if (objSettings.iceGroup != -1)
            {
                // pair this to the ice object later
                map.iceSwitches[Int32.Parse(m.Groups[objSettings.iceGroup].Value)] = mapObj;
                map.otherObjects.Add(mapObj);
            }
            if (objSettings.doorGroup != -1)
            {
                // pair this to the appropriate door
                int doorNumber = Int32.Parse(m.Groups[objSettings.doorGroup].Value);
                if (!map.doorSwitches.ContainsKey(doorNumber))
                {
                    map.doorSwitches[doorNumber] = new List<MapObject>();
                }
                map.doorSwitches[doorNumber].Add(mapObj);
            }
            if(objSettings.switchGroup != -1)
            {
                // note the combo order for this switch
                mapObj.num = Int32.Parse(m.Groups[objSettings.switchGroup].Value);
            }
            if(mapObj.type == 0x0D)
            {
                // light switch
                value = map.tileset["Switch"]; // square switch on ruins maps
                map.otherObjects.Add(mapObj);
            }
            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
