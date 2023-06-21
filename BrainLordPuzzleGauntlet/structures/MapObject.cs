using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.structures
{
    /// <summary>
    /// Brainlord's structure of an object on a map
    /// </summary>
    public class MapObject
    {
        // rom object structure
        public byte x;
        public byte y;
        public byte type;
        public byte p1;
        public byte p2;
        public byte p3;
        public byte p4;
        // this is extra info for the combo number on toggle switches
        public int num;
    }
}
