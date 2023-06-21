using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.MapLoad
{
    /// <summary>
    /// Load Brainlord maps from a plain text file in a format that I made up
    /// </summary>
    public class MapLoader
    {
        private static List<TileProcessor> tileProcessors = new List<TileProcessor>();
        static MapLoader()
        {
            // handlers for various types of tiles
            tileProcessors.Add(new TileDecorations()); // shaping of walls, shadowing of floors, etc
            tileProcessors.Add(new PushableObjectMaker()); // rocks, balls, ice
            tileProcessors.Add(new AutomatedMovingPlatformMaker()); // platforms
            tileProcessors.Add(new EntryPointMaker()); // entry points onto maps
            tileProcessors.Add(new SavePointMaker()); // saves
            tileProcessors.Add(new SwitchMaker()); // switch buttons
            tileProcessors.Add(new DoorMaker()); // doors
            tileProcessors.Add(new MessageBoxMaker()); // message boxes
            tileProcessors.Add(new TrapMaker()); // floor traps
            tileProcessors.Add(new EnemyMaker()); // enemies
            tileProcessors.Add(new ChestMaker()); // chests
            tileProcessors.Add(new StairsMaker()); // stairways
        }

        public static Map LoadMap(/*string resourceName*/Puzzles puzzles, string path, Dictionary<string, Tileset> tilesets, Dictionary<string, Map> parsedMaps)
        {
            // leftover from early on when i was embedding everything
            ////Assembly assemb = Assembly.GetExecutingAssembly();
            ////string[] names = assemb.GetManifestResourceNames();
            Map map = new Map();
            map.mapName = Path.GetFileName(path);
            map.mapPlacementOffsetX = 6;
            map.mapPlacementOffsetY = 2;

            string puzzleDirectory = Path.GetDirectoryName(path);
            parsedMaps[path] = map;
            Dictionary<char, string> dataKey = new Dictionary<char, string>();
            using (Stream stream = new FileStream(path, FileMode.Open))
            using (StreamReader reader = new StreamReader(stream, Encoding.Default))
            {
                try
                {
                    // leftover from early on when i was embedding everything
                    ////Stream stream = assemb.GetManifestResourceStream("BrainLordPuzzleGauntlet.Resources.Maps." + resourceName);
                    string mapSection = "";
                    int mapDataLine = 0;
                    char[,] data = null;
                    while (!reader.EndOfStream)
                    {
                        string line = PropertyFileUtil.stripComments(reader.ReadLine());
                        if (!line.StartsWith("#"))
                        {
                            if (line.StartsWith(":"))
                            {
                                mapSection = line.Substring(1);
                                if (mapSection == "mapdata")
                                {
                                    data = new char[0x40, 0x40];
                                }
                            }
                            else if (mapSection == "properties")
                            {
                                if (line.Contains("="))
                                {
                                    string[] tokens = line.Split(new char[] { '=' });
                                    switch (tokens[0])
                                    {
                                        case "tileset":
                                            if (!tilesets.ContainsKey(tokens[1]))
                                            {
                                                string tilesetError = "Unrecognized tileset: " + tokens[1] + Environment.NewLine + "Valid tilesets are:";
                                                foreach (string key in tilesets.Keys)
                                                {
                                                    tilesetError += Environment.NewLine + "  " + key;
                                                }
                                                throw new MapLoadException(map, tilesetError);
                                            }
                                            map.tileset = tilesets[tokens[1]];
                                            break;
                                        case "music":
                                            map.song = tokens[1];
                                            break;
                                        case "combo":
                                            string combo = tokens[1];
                                            if(combo.Length > 8)
                                            {
                                                map.loadWarnings.Add("Combo can't exceed 8 buttons.");
                                            }
                                            foreach (char c in combo)
                                            {
                                                map.switchCombo.Add(Int32.Parse("" + c));
                                            }
                                            break;
                                        default:
                                            map.properties[tokens[0]] = tokens[1];
                                            break;
                                    }
                                    if (tokens[0].StartsWith("message"))
                                    {
                                        int messageId = Int32.Parse(tokens[0].Replace("message", ""));
                                        map.messages[messageId] = tokens[1];
                                        while (map.messages[messageId].EndsWith(">"))
                                        {
                                            map.messages[messageId] = map.messages[messageId].Replace(">", "");
                                            line = reader.ReadLine();
                                            map.messages[messageId] += line;
                                        }
                                    }
                                    Match m = Regex.Match(tokens[0], "door(\\d)dest$");
                                    if (m.Success)
                                    {
                                        int doorNum = Int32.Parse(m.Groups[1].Value);
                                        string filenameAndEntry = tokens[1];
                                        if (filenameAndEntry != "none")
                                        {
                                            string[] filenameTokens = filenameAndEntry.Split(',');
                                            string filename = filenameTokens[0];
                                            if (!filename.Contains("/") && !filename.Contains("\\"))
                                            {
                                                filename = Path.Combine(puzzleDirectory, filename);
                                            }
                                            int entryNum = Int32.Parse(filenameTokens[1]);
                                            DoorTransition doorTransition = new DoorTransition();
                                            doorTransition.sourceDoorId = doorNum;
                                            doorTransition.destEntryId = entryNum;
                                            if (!parsedMaps.ContainsKey(filename))
                                            {
                                                // don't parse a map twice, so we can make two-way connecting doors if we want
                                                Map connectedMap = LoadMap(puzzles, filename, tilesets, parsedMaps);
                                                map.connections[doorTransition] = connectedMap;
                                            }
                                            else
                                            {
                                                map.connections[doorTransition] = parsedMaps[filename];
                                            }
                                        }
                                    }
                                }
                            }
                            else if (mapSection == "datakey")
                            {
                                if (line.Contains("="))
                                {
                                    int equalsIndex = line.IndexOf('=');
                                    string indicatorChar = line.Substring(0, equalsIndex);
                                    string tileValue = line.Substring(equalsIndex + 1);
                                    if (indicatorChar.Length == 1)
                                    {
                                        dataKey[indicatorChar[0]] = tileValue;
                                    }
                                    else
                                    {
                                        map.loadWarnings.Add("Ignoring multi-character data key: " + indicatorChar);
                                    }
                                }
                            }
                            else if (mapSection == "mapdata")
                            {
                                for (int i = 0; i < line.Length; i++)
                                {
                                    data[i, mapDataLine] = line[i];
                                }
                                mapDataLine++;
                            }
                        }
                    }
                    postProcessMapData(puzzles, data, map, dataKey);
                }
                catch (Exception e)
                {
                    // this is here since sometimes i like to put a breakpoint here
                    throw e;
                }
            }
            return map;
        }

        private static void postProcessMapData(Puzzles puzzles, char[,] parsedData, Map map, Dictionary<char, string> dataKey)
        {
            int fullMapWidth = 0x40;
            int fullMapHeight = 0x40;
            byte filler = map.tileset["Empty"];
            if(map.properties.ContainsKey("emptySpaceFiller"))
            {
                filler = map.tileset[map.properties["emptySpaceFiller"]];
            }
            map.data = new byte[fullMapWidth * fullMapHeight];
            for(int i=0; i < fullMapWidth * fullMapHeight; i++)
            {
                map.data[i] = filler;
            }

            char[,] tileTypeMap = TileDecorations.getTileTypeMap(parsedData, map, dataKey);
            
            for (int y=0; y < fullMapHeight - map.mapPlacementOffsetY; y++)
            {
                for (int x = 0; x < fullMapWidth - map.mapPlacementOffsetX; x++)
                {
                    byte value = filler;
                    if (dataKey.ContainsKey(parsedData[x, y]))
                    {
                        string tileType = dataKey[parsedData[x, y]];
                        if(map.tileset.unsupportedTiles.ContainsKey(tileType))
                        {
                            string loadWarning = "Unsupported tile type for tileset " + map.tileset.name + ": " + tileType + " (replaced with: " + map.tileset.unsupportedTiles[tileType] + ")";
                            if (!map.loadWarnings.Contains(loadWarning))
                            {
                                map.loadWarnings.Add(loadWarning);
                            }
                            tileType = map.tileset.unsupportedTiles[tileType];
                        }

                        bool processed = false;
                        foreach(TileProcessor tileProcessor in tileProcessors)
                        {
                            if(tileProcessor.supportsTile(tileType))
                            {
                                value = tileProcessor.processTile(tileType, puzzles, map, tileTypeMap, x, y);
                                processed = true;
                                break;
                            }
                        }
                        if (!processed)
                        {
                            // just specify hex tile number instead of a name from my tilesets
                            // note there is no consistency of ids between tilesets, so doing this basically locks you to a single tileset
                            if (tileType.StartsWith("@"))
                            {
                                value = Byte.Parse(tileType.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier);
                            }
                        }
                    }
                    map.data[(y + map.mapPlacementOffsetY) * fullMapWidth + x + map.mapPlacementOffsetX] = value;
                }
            }

            // second pass; process anything that needs the whole map w/ processed tiles & objects, or will overwrite the first-pass stuff
            foreach(TileProcessor tileProcessor in tileProcessors)
            {
                tileProcessor.postProcess(puzzles, map, tileTypeMap);
            }

            // assign switches to doors, and post-process values for switches
            foreach (int doorNum in map.doorSwitches.Keys)
            {
                byte switchMask = 0x0F;
                byte switchBit = 0x08;
                byte toggleMask = 0x00;
                for (int i = 0; i < map.switchCombo.Count - 1; i++)
                {
                    toggleMask |= (byte)(1 << i);
                }
                toggleMask = (byte)(~toggleMask);
                int numSwitchesFound = 0;

                foreach (MapObject obj in map.doorSwitches[doorNum])
                {
                    if (!map.doors.ContainsKey(doorNum))
                    {
                        throw new MapLoadException(map, "Entry for door " + doorNum + " not placed!");
                    }
                    if (obj.type == 0x0A)
                    {
                        // value of 0F opens the door; each switch sets a bit, and the last one sets all the remaining ones
                        numSwitchesFound++;
                        obj.p2 = (byte)(map.doors[doorNum].y / 2);
                        obj.p3 = (byte)(map.doors[doorNum].x / 4);
                        if (numSwitchesFound == map.doorSwitches[doorNum].Count)
                        {
                            // last one
                            obj.p1 = (byte)((map.tileset["SwitchPressedForObject"] << 5) + switchMask);
                        }
                        else
                        {
                            obj.p1 = (byte)((map.tileset["SwitchPressedForObject"] << 5) + switchBit);
                            switchMask &= (byte)(~switchBit);
                            switchBit >>= 1;
                        }
                    }
                    else if (obj.type == 0x22)
                    {
                        // value of FF opens the door; each sequential switch sets a bit, and the last one sets all the remaining ones
                        int comboNum = obj.num;
                        int comboSeq = -1;
                        for (int i = 0; i < map.switchCombo.Count; i++)
                        {
                            int combo = map.switchCombo[i];
                            if (comboNum == combo)
                            {
                                comboSeq = i;
                                break;
                            }
                        }
                        obj.p2 = (byte)(map.doors[doorNum].y / 2);
                        obj.p3 = (byte)(map.doors[doorNum].x / 4);

                        if (comboSeq != -1)
                        {
                            if (comboSeq == map.switchCombo.Count - 1)
                            {
                                // last one
                                obj.p1 = (byte)(toggleMask);
                            }
                            else
                            {
                                obj.p1 = (byte)(1 << comboSeq);
                            }
                        }
                        else
                        {
                            obj.p1 = 0;
                        }
                    }
                }
            }

            foreach (int iceId in map.fallingIce.Keys)
            {
                // check if ice really exists
                if (map.iceSwitches.ContainsKey(iceId))
                {
                    map.iceSwitches[iceId].p1 = (byte)map.fallingIce[iceId].num; // 0x00; // 00 is ice, 01 is ball, every other value seems to be ice
                    map.iceSwitches[iceId].p2 = (byte)(map.fallingIce[iceId].y / 2);
                    map.iceSwitches[iceId].p3 = (byte)(map.fallingIce[iceId].x / 4);
                }
                else
                {
                    map.loadWarnings.Add("");
                }
                // okay setting this to 4 makes it a hole and is kind of hilarious .. this byte impacts what the button changes to
                // but it's probably tileset-dependent
                //map.iceSwitches[iceId].p4 = 0x00; 
            }
        }
    }
}
