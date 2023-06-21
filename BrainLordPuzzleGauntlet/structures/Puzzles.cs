using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.structures
{
    /// <summary>
    /// Structure with all the shit from the text file
    /// </summary>
    public class Puzzles
    {
        public string firstPuzzleFilePath;
        public int firstPuzzleDoorId;
        public bool startWithCape = true;
        public bool startWithFireSword = true;
        public int startingHealth = 127;
        public int startingAtk = 99;
        public int startingDef = 99;
        public bool practiceMode = false;
        // also some "global" map processing state that we pass around
        public int saveNum = 1;
        public int chestNum = 1;
        public Dictionary<string, string> otherProperties = new Dictionary<string, string>();

        public bool booleanPropertyEnabled(string propertyName, bool defaultValue)
        {
            if (otherProperties.ContainsKey(propertyName))
            {
                string propVal = otherProperties[propertyName].ToLower();
                return propVal == "yes" || propVal == "1" || propVal == "true";
            }
            return defaultValue;
        }

        public int intPropertyValue(string propertyName, int defaultValue)
        {
            if (otherProperties.ContainsKey(propertyName))
            {
                int parsed;
                if (Int32.TryParse(otherProperties[propertyName], out parsed))
                {
                    return parsed;
                }
                return defaultValue;
            }
            return defaultValue;
        }

    }
}
