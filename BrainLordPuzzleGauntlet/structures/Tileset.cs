using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.structures
{
    /// <summary>
    /// Tileset structure
    /// </summary>
    public class Tileset
    {
        public string name;
        public Dictionary<string, byte> tileValues = new Dictionary<string, byte>();
        public Dictionary<string, string> unsupportedTiles = new Dictionary<string, string>();
        public int sourceMapStart;
        public int sourceMapStartAlt2;
        public int sourceMapStartAlt1;
        public int sourceMapStartAlt12;
        public int unlockedDoorTileOffset;

        public byte this[string key]
        {
            get { return tileValues[key]; }
        }
    }
}
