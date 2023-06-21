using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    public class CodeGenerationUtils
    {
        /// <summary>
        /// Make sure we don't overrun bank boundaries
        /// </summary>
        /// <param name="newCodeOffset"></param>
        /// <param name="numBytes"></param>
        public static bool ensureSpaceInBank(ref int newCodeOffset, int numBytes)
        {
            byte bank1 = (byte)(newCodeOffset >> 16);
            byte bank2 = (byte)((newCodeOffset + numBytes) >> 16);
            if (bank1 != bank2)
            {
                newCodeOffset = (bank2 << 16);
                return true;
            }
            return false;
        }
    }

}
