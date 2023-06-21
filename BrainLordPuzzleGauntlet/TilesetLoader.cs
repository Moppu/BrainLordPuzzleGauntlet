using BrainLordPuzzleGauntlet.structures;
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
    /// Load tileset data from txt files i keep in here as embedded resources.
    /// </summary>
    public class TilesetLoader
    {
        public static Tileset LoadResource(string tilesetName, string resourceName)
        {
            // $C8/C080 C2 20       REP #$20                A:007C X:0400 Y:0020 P:envmxdizC
            Assembly assemb = Assembly.GetExecutingAssembly();
            string[] names = assemb.GetManifestResourceNames();

            Stream stream = assemb.GetManifestResourceStream("BrainLordPuzzleGauntlet.Resources.Tileset." + resourceName);
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            Tileset tileset = new Tileset();
            tileset.name = tilesetName;

            while (!reader.EndOfStream)
            {
                string line = PropertyFileUtil.stripComments(reader.ReadLine());
                if (!line.Trim().StartsWith("#") && line.Contains("="))
                {
                    string[] tokens = line.Split(new char[] { '=' });
                    if (tokens[0] == "TilesetStartOffset")
                    {
                        // vanilla map to pull tileset, palette, etc data from
                        tileset.sourceMapStart = Int32.Parse(tokens[1], System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (tokens[0] == "TilesetStartOffsetAlt1")
                    {
                        // vanilla map to pull tileset, palette, etc data from for alternate settings (usually phantom floor)
                        tileset.sourceMapStartAlt1 = Int32.Parse(tokens[1], System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (tokens[0] == "TilesetStartOffsetAlt2")
                    {
                        // vanilla map to pull tileset, palette, etc data from for alternate settings (usually phantom floor)
                        tileset.sourceMapStartAlt2 = Int32.Parse(tokens[1], System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (tokens[0] == "UnlockedDoorTileOffset")
                    {
                        // vanilla map to pull tileset, palette, etc data from for alternate settings (usually phantom floor)
                        tileset.unlockedDoorTileOffset = Int32.Parse(tokens[1], System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else
                    {
                        // process everything else as a byte
                        tileset.tileValues[tokens[0]] = Byte.Parse(tokens[1], System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                }
                else if(line.Contains(">"))
                {
                    // declare tile type not supported in this tileset, and what we should use instead for best compatibility
                    string[] tokens = line.Split(new char[] { '>' });
                    tileset.unsupportedTiles[tokens[0]] = tokens[1];
                }
            }

            return tileset;
        }
    }
}
