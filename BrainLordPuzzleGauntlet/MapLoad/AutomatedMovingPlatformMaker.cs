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
    /// Make those platforms that move back and forth automatically, or the ones you step on to move them
    /// </summary>
    public class AutomatedMovingPlatformMaker : TileProcessor
    {
        private static string regex = RegexUtil.makeRegexForNumberedLiteral("^(Automated )?Moving Platform", 1);

        public bool supportsTile(string tileType)
        {
            return Regex.Match(tileType, regex).Success;
        }

        public byte processTile(string tileType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            int floorUnderObject = -1;
            bool automatedPlatform = tileType.Contains("Automated");
            string objectType = tileType;
            // allow specification of floor underneath with ":"
            TileDecorations.processObjectOverTile(tileType, map, tileTypeMap, x, y, out floorUnderObject, out objectType);

            Match m = Regex.Match(tileType, regex);

            byte value = floorUnderObject == -1 ? TileDecorations.chooseTile(map, "Hole", tileTypeMap, x, y) : (byte)floorUnderObject;
            int entryNumber = Int32.Parse(m.Groups[2].Value);

            MapObject mapObj = new MapObject();
            mapObj.x = (byte)((x + map.mapPlacementOffsetX) * 4);
            mapObj.y = (byte)((y + map.mapPlacementOffsetY) * 2);
            mapObj.type = (byte)(automatedPlatform ? 0x09 : 0x20);
            mapObj.p1 = (byte)entryNumber;
            // ^ these seem to be different for the two types
            mapObj.p2 = 0x00;
            mapObj.p3 = 0x00;
            mapObj.p4 = 0x00;
            // p2/3/4 seem to do nothing
            map.otherObjects.Add(mapObj);

            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
