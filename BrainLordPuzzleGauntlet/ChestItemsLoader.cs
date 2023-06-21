using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    /// <summary>
    /// Load the list of items paired with their IDs
    /// </summary>
    public class ChestItemsLoader
    {
        public static Dictionary<string, string> LoadResource()
        {
            Assembly assemb = Assembly.GetExecutingAssembly();
            string[] names = assemb.GetManifestResourceNames();

            Stream stream = assemb.GetManifestResourceStream("BrainLordPuzzleGauntlet.Resources.Misc.ChestItems.txt");
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            Dictionary<string, string> itemRegexToIdPatterns = new Dictionary<string, string>();

            while(!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                line = line.Trim();
                if(!line.StartsWith("#"))
                {
                    if(line.Contains("="))
                    {
                        string[] tokens = line.Split(new char[] { '=' });
                        string itemNameRegex = tokens[0];
                        string itemValuePattern = tokens[1];
                        itemRegexToIdPatterns[itemNameRegex] = itemValuePattern;
                    }
                }
            }

            return itemRegexToIdPatterns;
        }
    }
}
