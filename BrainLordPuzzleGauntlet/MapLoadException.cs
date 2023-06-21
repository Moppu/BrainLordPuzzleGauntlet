using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    public class MapLoadException : Exception
    {
        public Map map;
        public MapLoadException(Map map, String message) : base(message)
        {
            this.map = map;
        }
    }
}
