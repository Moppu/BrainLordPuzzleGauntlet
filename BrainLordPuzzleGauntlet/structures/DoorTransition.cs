using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.structures
{
    /// <summary>
    /// Pairing of door to entry point on another map
    /// </summary>
    public class DoorTransition
    {
        public int sourceDoorId;
        public int destEntryId;
    }
}
