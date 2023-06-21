using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.MapLoad
{
    /// <summary>
    /// Regex utils for map processing
    /// </summary>
    public class RegexUtil
    {
        public static string makeRegexForNumberedLiteral(string name, int numGroups)
        {
            return makeOpenEndedRegexForNumberedLiteral(name, numGroups) + "$";
        }

        public static string makeOpenEndedRegexForNumberedLiteral(string name, int numGroups)
        {
            string regex = name;
            // number is a group so we can extract it later
            for (int i = 0; i < numGroups; i++)
            {
                regex += " (\\d\\d?)";
            }
            return regex;
        }
    }
}
