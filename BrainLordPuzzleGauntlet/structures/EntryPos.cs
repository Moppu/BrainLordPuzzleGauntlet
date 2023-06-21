using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.structures
{
    /// <summary>
    /// Entry spot on map from a door on another map
    /// </summary>
    public class EntryPos
    {
        public byte x;
        public byte y;
        public byte dir; // 00 up, 02 down
    }
}
