using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BrainLordPuzzleGauntlet.structures;

namespace BrainLordPuzzleGauntlet.MapLoad
{
    public class ChestMaker : TileProcessor
    {
        static Dictionary<string, string> itemNameRegexToIdPatterns = ChestItemsLoader.LoadResource();

        // chest items list at C5:5576 in ROM; 2 bytes per item; loaded in code:
        /*
$D4/5F83 B7 88       LDA [$88],y[$C5:405D]   A:40C5 X:1100 Y:0003 P:envmxdizC
$D4/5F85 29 FF 00    AND #$00FF              A:029B X:1100 Y:0003 P:envmxdizC
$D4/5F88 0A          ASL A                   A:009B X:1100 Y:0003 P:envmxdizC
$D4/5F89 A8          TAY                     A:0136 X:1100 Y:0003 P:envmxdizc
$D4/5F8A C2 20       REP #$20                A:0136 X:1100 Y:0136 P:envmxdizc
$D4/5F8C A9 76 55    LDA #$5576              A:0136 X:1100 Y:0136 P:envmxdizc
$D4/5F8F 85 82       STA $82    [$00:0082]   A:5576 X:1100 Y:0136 P:envmxdizc
$D4/5F91 E2 20       SEP #$20                A:5576 X:1100 Y:0136 P:envmxdizc
$D4/5F93 A9 C5       LDA #$C5                A:5576 X:1100 Y:0136 P:envMxdizc
         */
        // we should probably figure out how many of these saveram actually supports; the list above appears to have 190 entries, with some identical ones (filler?) at the end
        // 0x556ae (01 05 = reviving mirror) is the island in the big room in platinum2, which is what i'm using for testing here
        // LSB is item ID within category, and MSB is item category
        // categories:
        // 00 consumables
        // 01 weapons
        // 02 hats
        // 03 armor
        // 04 shields
        // 05 accessories (reviving mirror, wind shoes)
        // 06 jades
        // 07 keys
        // 08 spells
        // 09 looks like it just starts dumping the whole rom to the text box so let's assume it ends there

        public bool supportsTile(string tileType)
        {
            // "(FloorType:)Chest=itemType"
            return tileType.Contains("Chest=");
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

            byte value = floorUnderObject == -1 ? TileDecorations.chooseTile(map, "Floor", tileTypeMap, x, y) : (byte)floorUnderObject;
            // make chest from objectType
            string[] chestTokens = objectType.Split(new char[] { '=' });
            bool foundItem = false;
            foreach(string namePattern in itemNameRegexToIdPatterns.Keys)
            {
                Match m = Regex.Match(chestTokens[1].ToLower(), namePattern);
                if (m.Success)
                {
                    string idPattern = itemNameRegexToIdPatterns[namePattern];
                    if(idPattern.Contains("xx"))
                    {
                        string groupVal = m.Groups[1].Value;
                        idPattern = idPattern.Replace("xx", Int32.Parse(groupVal).ToString("X").PadLeft(2, '0'));
                    }
                    ushort itemValue = ushort.Parse(idPattern, System.Globalization.NumberStyles.AllowHexSpecifier);
                    // add chest to map
                    foundItem = true;
                    Chest chest = new Chest();
                    chest.x = (byte)((x + map.mapPlacementOffsetX) * 4);
                    chest.y = (byte)((y + map.mapPlacementOffsetY) * 2);
                    chest.item = itemValue;
                    map.chests[puzzles.chestNum++] = chest;
                    break;
                }
            }
            if(!foundItem)
            {
                throw new MapLoadException(map, "Couldn't identify item: " + chestTokens[1] + " for chest");
            }
            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
