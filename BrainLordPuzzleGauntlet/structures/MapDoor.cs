using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.structures
{
    /// <summary>
    /// A door on a map
    /// </summary>
    public class MapDoor
    {
        public int x;
        public int y;
        // don't actually go anywhere, this is so you can switch a door to open and let you through for puzzles
        public bool noTransition;
        public byte doorType;
        public byte doorAnimation;
        public int key = -1;
    }
}
