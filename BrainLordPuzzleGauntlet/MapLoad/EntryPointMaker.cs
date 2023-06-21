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
    /// Create entry points into map to be referenced by doors on other maps
    /// <para>
    ///   supports objects:
    ///     <para>"[Floor tile type:]Entry (n)" - An entry point into the map from other maps</para>
    ///     <para>  (n) = A unique numeric entry ID to be referenced by doors on other maps</para>
    ///     <para>[Floor tile type:] is optional and refers to a floor type from TileDecorations. Defults to "Floor" if unspecified.</para>
    /// </para>
    /// </summary>
    public class EntryPointMaker : TileProcessor
    {
        private static string regex = "Entry (\\d\\d?)";

        public bool supportsTile(string tileType)
        {
            return Regex.Match(tileType, regex).Success;
        }

        public byte processTile(string tileType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            int floorUnderObject = -1;
            string objectType = tileType;
            // allow specification of floor underneath with ":"
            TileDecorations.processObjectOverTile(tileType, map, tileTypeMap, x, y, out floorUnderObject, out objectType);
            int entryDir = -1;
            // allow specification of direction with >
            if(objectType.Contains(">"))
            {
                string[] dirTokens = objectType.Split(new char[] { '>' });
                objectType = dirTokens[0];
                string dir = dirTokens[1].ToLower();
                switch (dir)
                {
                    case "up":
                        entryDir = 0;
                        break;
                    case "right":
                        entryDir = 1;
                        break;
                    case "down":
                        entryDir = 2;
                        break;
                    case "left":
                        entryDir = 3;
                        break;
                }
            }
            // match to extract id
            Match m = Regex.Match(tileType, regex);

            byte value = floorUnderObject == -1 ? TileDecorations.chooseTile(map, "Floor", tileTypeMap, x, y) : (byte)floorUnderObject;
            int entryNumber = Int32.Parse(m.Groups[1].Value);
            // player start spot
            EntryPos entry = new EntryPos();
            entry.x = (byte)(x + map.mapPlacementOffsetX);
            entry.y = (byte)(y + map.mapPlacementOffsetY);
            if(entryDir != -1)
            {
                entry.dir = (byte)entryDir;
            }
            else if (TileDecorations.isDoor(tileTypeMap, x, y - 1))
            {
                entry.dir = 0x02;
            }
            map.entries[entryNumber] = entry;
            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
