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
    /// Make the messageboxes that show up on walls
    /// </summary>
    public class MessageBoxMaker : TileProcessor
    {
        private static string regex = RegexUtil.makeRegexForNumberedLiteral("Message Box", 1);

        public bool supportsTile(string tileType)
        {
            return Regex.Match(tileType, regex).Success;
        }

        public byte processTile(string tileType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            Match m = Regex.Match(tileType, regex);

            int messageBoxNumber = Int32.Parse(m.Groups[1].Value);
            // messagebox plaque
            byte value = map.tileset["MessageBox"];
            // "doorway" entry for these
            MapMessageBox mapMessageBox = new MapMessageBox();
            mapMessageBox.x = (byte)((x + map.mapPlacementOffsetX) * 4);
            mapMessageBox.y = (byte)((y + map.mapPlacementOffsetY) * 2);
            if (!map.messageBoxes.ContainsKey(messageBoxNumber))
            {
                map.messageBoxes[messageBoxNumber] = new List<MapMessageBox>();
            }
            map.messageBoxes[messageBoxNumber].Add(mapMessageBox);
            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
