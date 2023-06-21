using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    /// <summary>
    /// Loader for my text file with global settings for a gauntlet hack and a link to the map file/door num to start with.
    /// </summary>
    public class PuzzlesLoader
    {
        public static Puzzles LoadPuzzles(string filePath)
        {
            Puzzles puzzles = new Puzzles();
            using (Stream stream = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    StreamReader reader = new StreamReader(stream, Encoding.Default);
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!line.StartsWith("#"))
                        {
                            if (line.Contains("="))
                            {
                                string[] tokens = line.Split(new char[] { '=' });
                                string value = tokens[1];
                                while (value.EndsWith(">"))
                                {
                                    value = value.Replace(">", "");
                                    line = reader.ReadLine();
                                    value += line;
                                }

                                switch (tokens[0])
                                {
                                    case "start":
                                        string[] startTokens = value.Trim().Split(new char[] { '|' });
                                        puzzles.firstPuzzleFilePath = startTokens[0];
                                        puzzles.firstPuzzleDoorId = Int32.Parse(startTokens[1]);
                                        break;
                                    case "startWithCape":
                                        puzzles.startWithCape = value.Trim() == "yes";
                                        break;
                                    case "startWithFireSword":
                                        puzzles.startWithFireSword = value.Trim() == "yes";
                                        break;
                                    case "startingHealth":
                                        puzzles.startingHealth = Int32.Parse(value.Trim());
                                        break;
                                    case "startingAtk":
                                        puzzles.startingAtk = Int32.Parse(value.Trim());
                                        break;
                                    case "startingDef":
                                        puzzles.startingDef = Int32.Parse(value.Trim());
                                        break;
                                    case "practiceMode":
                                        puzzles.practiceMode = value.Trim() == "yes";
                                        break;
                                    default:
                                        puzzles.otherProperties[tokens[0]] = value;
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return puzzles;
        }
    }
}
