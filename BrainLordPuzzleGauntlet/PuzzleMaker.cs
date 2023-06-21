using BrainLordPuzzleGauntlet.MapLoad;
using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    /// <summary>
    /// Main entrypoint for generating a "puzzle gauntlet" rom from a text file linking in map data.
    /// </summary>
    public class PuzzleMaker
    {
        static Dictionary<string, byte> songs = new Dictionary<string, byte>();
        static Dictionary<string, Tileset> allTilesets = new Dictionary<string, Tileset>();

        static PuzzleMaker()
        {
            // droog or others may be added later; i think these are the only four that have buttons
            Tileset platinumTileset = TilesetLoader.LoadResource("Platinum", "Platinum.txt");
            allTilesets["Platinum"] = platinumTileset;
            Tileset iceCastleTileset = TilesetLoader.LoadResource("Ice Castle", "IceCastle.txt");
            allTilesets["Ice Castle"] = iceCastleTileset;
            Tileset towerTileset = TilesetLoader.LoadResource("Tower", "Tower.txt");
            allTilesets["Tower"] = towerTileset;
            Tileset ruinsTileset = TilesetLoader.LoadResource("Ruins", "Ruins.txt");
            allTilesets["Ruins"] = ruinsTileset;
            songs["Tower Path"] = 0;
            songs["Tower"] = 1;
            songs["Ruins Path"] = 2;
            songs["Shop"] = 3;
            songs["Boss 1"] = 4;
            songs["Toronto"] = 5;
            songs["Ruins"] = 6;
            songs["Platinum"] = 7;
            songs["Arcs"] = 8;
            songs["Ice Castle"] = 9;
            songs["Inn"] = 10;
            songs["Toronto Path"] = 11;
            songs["Desert"] = 12;
            songs["Arena"] = 13;
            songs["Arena Win"] = 14;
            songs["Arena Loss"] = 15;
            songs["Intro Cutscene"] = 16;
            songs["Title"] = 17;
            songs["Droog"] = 18;
            songs["Droog Path"] = 19;
            songs["Platinum Path"] = 20;
            songs["Boss 2"] = 21;
            songs["Boss Win"] = 22;
            songs["Death"] = 23;
            songs["Toronto Cutscenes"] = 24;
            songs["Dragon"] = 25;
            songs["Credits"] = 29;
            songs["Ending Cutscene"] = 30;
        }

        public static Dictionary<string, Map> process(string puzzleFile, byte[] rom)
        {
            int workingOffset = 0x180000;

            Dictionary<string, Map> parsedMaps = new Dictionary<string, Map>();
            Puzzles puzzles = PuzzlesLoader.LoadPuzzles(puzzleFile);
            string firstFileName = puzzles.firstPuzzleFilePath;
            string puzzleDirectory = Path.GetDirectoryName(firstFileName);
            if (puzzleDirectory.Length == 0)
            {
                puzzleDirectory = Path.GetDirectoryName(puzzleFile);
                firstFileName = Path.Combine(puzzleDirectory, firstFileName);
            }
            Map testMap = MapLoader.LoadMap(puzzles, firstFileName, allTilesets, parsedMaps);
            if(!songs.ContainsKey(testMap.song))
            {
                string songError = "Unrecognized song: " + testMap.song + Environment.NewLine + "Valid songs are:";
                foreach(string key in songs.Keys)
                {
                    songError += Environment.NewLine + "  " + key;
                }
                throw new MapLoadException(testMap, songError);
            }

            // set up chests list
            int chestValuesOffset = CodeChanges.setUpNewChestBlock(rom, ref workingOffset);

            // set up save points
            int saveDoorsOffset = CodeChanges.setUpNewSaveBlock(rom, ref workingOffset, puzzles.saveNum);

            // fix issue where keys are locked to their original tilesets
            CodeChanges.lockedDoorTileFix(rom, ref workingOffset, allTilesets);

            // set key name/descriptions
            CodeChanges.processKeyNames(rom, ref workingOffset, puzzles);

            // push stuff faster as a QOL; off by default
            if(puzzles.booleanPropertyEnabled("fasterPushing", false))
            {
                CodeChanges.fasterObjectPushing(rom);
            }

            foreach(Tileset tileset in allTilesets.Values)
            {
                if(tileset.sourceMapStartAlt2 != 0 && tileset.sourceMapStartAlt1 != 0)
                {
                    // make a tileset setting (for ruins) that has both phantom floor and darkness enabled
                    // let's hope this works because vanilla doesn't use it
                    tileset.sourceMapStartAlt12 = workingOffset;
                    for(int i=0; i < 0x34; i++)
                    {
                        byte normalTilesetValue = rom[tileset.sourceMapStart + i];
                        byte darkTilesetValue = rom[tileset.sourceMapStartAlt1 + i];
                        byte phantomTilesetValue = rom[tileset.sourceMapStartAlt2 + i];
                        if(darkTilesetValue != normalTilesetValue)
                        {
                            rom[workingOffset++] = darkTilesetValue;
                        }
                        else if(phantomTilesetValue != normalTilesetValue)
                        {
                            rom[workingOffset++] = phantomTilesetValue;
                        }
                        else
                        {
                            rom[workingOffset++] = normalTilesetValue;
                        }
                    }
                }
            }

            Dictionary<Map, Dictionary<int, int>> mapEntries = new Dictionary<Map, Dictionary<int, int>>();
            writeMap(puzzles, testMap, rom, ref workingOffset, parsedMaps, mapEntries, saveDoorsOffset, chestValuesOffset);

            Dictionary<int, int> doorOffsets = mapEntries[testMap];

            // now make the starting door change to this one
            CodeChanges.setStartingDoor(rom, doorOffsets[puzzles.firstPuzzleDoorId]);

            // save point 0 is normally arcs and it gets added automatically to the warp gate list; use the entry door for it here
            CodeChanges.setWarpPoint(rom, ref workingOffset, 0, "Entry", doorOffsets[puzzles.firstPuzzleDoorId]);

            rom[0xAB85] = (byte)puzzles.saveNum;

            if (puzzles.startWithFireSword)
            {
                rom[0x16F48] = 0x12; // start with fire sword instead of copper sword
            }
            if (puzzles.startWithCape)
            {
                rom[0x16F4A] = 0x07; // start with cape instead of leather armor
            }

            rom[0x16F45] = (byte)puzzles.startingHealth; // health
            rom[0x16F46] = (byte)puzzles.startingAtk; // atk
            rom[0x16F47] = (byte)puzzles.startingDef; // def

            return parsedMaps;
        }

        private static void writeMap(Puzzles puzzles, Map testMap, byte[] rom, ref int workingOffset, Dictionary<string, Map> parsedMaps, Dictionary<Map, Dictionary<int, int>> mapEntries, int saveDoorsOffset, int chestValuesOffset)
        {
            int mapDataOffset = MapWriter.writeMap(rom, testMap, ref workingOffset) + 0xC00000;
            int mapSettingsOffset = workingOffset + 0xC00000;
            rom[workingOffset++] = (byte)(mapDataOffset);
            rom[workingOffset++] = (byte)(mapDataOffset >> 8);
            rom[workingOffset++] = (byte)(mapDataOffset >> 16);
            Tileset tileset = testMap.tileset;
            // the rest of these are just like palette/tileset/etc settings
            // also in here is whether or not we do phantom floors for the map, so we swap these out here for that
            // this also defines whether the waterfall tiles in ice castle animate, but for now we just always have that enabled, because why not
            int sourceMapOffset = tileset.sourceMapStart;
            bool alt1 = testMap.booleanPropertyEnabled("dark", false) || testMap.booleanPropertyEnabled("alternating traps", false);
            bool alt2 = testMap.booleanPropertyEnabled("phantom floor", false);
            if (alt2 && !alt1)
            {
                if(tileset.sourceMapStartAlt2 != 0)
                {
                    sourceMapOffset = tileset.sourceMapStartAlt2;
                }
                else
                {
                    testMap.loadWarnings.Add("Map " + testMap.mapName + " tileset " + testMap.tileset.name + " does not support phantom floors");
                }
            }
            if (!alt2 && alt1)
            {
                if (tileset.sourceMapStartAlt1 != 0)
                {
                    sourceMapOffset = tileset.sourceMapStartAlt1;
                }
                else
                {
                    testMap.loadWarnings.Add("Map " + testMap.mapName + " tileset " + testMap.tileset.name + " does not support dark rooms");
                }
            }
            if (alt2 && alt1)
            {
                if (tileset.sourceMapStartAlt12 != 0)
                {
                    sourceMapOffset = tileset.sourceMapStartAlt12;
                }
                else
                {
                    testMap.loadWarnings.Add("Map " + testMap.mapName + " tileset " + testMap.tileset.name + " does not support phantom floor dark rooms");
                }
            }

            for (int orig = sourceMapOffset; orig < sourceMapOffset + 0x34; orig++)
            {
                rom[workingOffset++] = rom[orig];
            }

            Dictionary<int, int> messageOffsets = new Dictionary<int, int>();
            foreach (int messageId in testMap.messages.Keys)
            {
                messageOffsets[messageId] = workingOffset + 0xC00000;
                foreach (char c in testMap.messages[messageId])
                {
                    rom[workingOffset++] = TextConversion.convertChar(c);
                }
                rom[workingOffset++] = 0xF7; // end textbox
            }


            // try to fit doors, messageboxes, and ending block in-bank
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, (testMap.doors.Count + testMap.messageBoxes.Count + 1) * 7);
            int doorwaysOffset = workingOffset + 0xC00000;
            // doors
            foreach (int doorId in testMap.doors.Keys)
            {
                MapDoor door = testMap.doors[doorId];
                rom[workingOffset++] = (byte)door.x;
                rom[workingOffset++] = (byte)door.y;
                // 0 for local door
                rom[workingOffset++] = (byte)(door.doorType);
                rom[workingOffset++] = (byte)(door.key != -1 ? door.key : door.doorAnimation);
                // the dependent maps haven't been created yet, so we don't have an offset to put here until later
                rom[workingOffset++] = 0;
                rom[workingOffset++] = 0;
                rom[workingOffset++] = 0;
            }

            // messageboxes
            foreach (int messageId in testMap.messageBoxes.Keys)
            {
                foreach (MapMessageBox box in testMap.messageBoxes[messageId])
                {
                    CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, 7);
                    int messageOffset = messageOffsets[messageId];
                    rom[workingOffset++] = box.x;
                    rom[workingOffset++] = box.y;
                    rom[workingOffset++] = 0x02;
                    rom[workingOffset++] = 0x0F;
                    rom[workingOffset++] = (byte)(messageOffset);
                    rom[workingOffset++] = (byte)(messageOffset >> 8);
                    rom[workingOffset++] = (byte)(messageOffset >> 16);
                }
            }
            // doorway block end
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0x00;
            rom[workingOffset++] = 0x00;
            rom[workingOffset++] = 0x00;

            // for now, no enemies
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, (testMap.enemies.Count + 1) * 6);
            int enemiesOffset = workingOffset + 0xC00000;
            foreach(Enemy enemy in testMap.enemies)
            {
                rom[workingOffset++] = enemy.x;
                rom[workingOffset++] = enemy.y;
                rom[workingOffset++] = enemy.type;
                rom[workingOffset++] = enemy.b3;
                rom[workingOffset++] = enemy.b4;
                rom[workingOffset++] = enemy.b5;
            }
            // empty enemy block
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;

            // Filter objects based on map data, like brainlord does for tiles, so we don't see rocks and stuff
            // outside the boundaries of the room we're in right now

            // This means we have to make a different set of these for each entrypoint (also the saveload spots)
            // then below we'll plug the right ones in
            Dictionary<int, int> entryPointObjectOffsets = new Dictionary<int, int>();
            Dictionary<int, int> saveEntryPointObjectOffsets = new Dictionary<int, int>();
            foreach (int entryId in testMap.entries.Keys)
            {
                entryPointObjectOffsets[entryId] = workingOffset + 0xC00000;
                PostProcessing.makeObjectList(testMap, rom, ref workingOffset, testMap.entries[entryId]);
            }
            foreach (int saveId in testMap.saveLoadSpots.Keys)
            {
                saveEntryPointObjectOffsets[saveId] = workingOffset + 0xC00000;
                PostProcessing.makeObjectList(testMap, rom, ref workingOffset, testMap.saveLoadSpots[saveId].entryPos);
            }

            Dictionary<int, int> entryDoorOffsets = new Dictionary<int, int>();
            foreach (int entryId in testMap.entries.Keys)
            {
                // unsure why the offsets but this seems to work
                int startXPixel = testMap.entries[entryId].x * 32 - 0x80;
                int startYPixel = testMap.entries[entryId].y * 32 - 0x80;
                // make sure the 23 bytes for the door all fit in-bank
                CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, 23);
                int doorOffset = workingOffset + 0xC00000;
                entryDoorOffsets[entryId] = doorOffset;
                // make a door to it
                rom[workingOffset++] = testMap.entries[entryId].dir; // my direction after entering 0=up, 1=right
                rom[workingOffset++] = (byte)(startXPixel); // 0xC0; // x pos lsb
                rom[workingOffset++] = (byte)(startXPixel >> 8); // 0x00; // x pos msb
                rom[workingOffset++] = (byte)(startYPixel);// 0x08; // y pos lsb
                rom[workingOffset++] = (byte)(startYPixel >> 8);// 0x00; // y pos msb
                // offset to itself; i think this is where "return" goes
                rom[workingOffset++] = (byte)(doorOffset);
                rom[workingOffset++] = (byte)(doorOffset >> 8);
                rom[workingOffset++] = (byte)(doorOffset >> 16);
                // map data offset
                rom[workingOffset++] = (byte)(mapSettingsOffset);
                rom[workingOffset++] = (byte)(mapSettingsOffset >> 8);
                rom[workingOffset++] = (byte)(mapSettingsOffset >> 16);
                // enemy data offset
                rom[workingOffset++] = (byte)(enemiesOffset);
                rom[workingOffset++] = (byte)(enemiesOffset >> 8);
                rom[workingOffset++] = (byte)(enemiesOffset >> 16);
                // door data offset
                rom[workingOffset++] = (byte)(doorwaysOffset);
                rom[workingOffset++] = (byte)(doorwaysOffset >> 8);
                rom[workingOffset++] = (byte)(doorwaysOffset >> 16);
                // object data offset
                int objectOffset = entryPointObjectOffsets[entryId];
                rom[workingOffset++] = (byte)(objectOffset);
                rom[workingOffset++] = (byte)(objectOffset >> 8);
                rom[workingOffset++] = (byte)(objectOffset >> 16);

                rom[workingOffset++] = 0x08; // unknown
                rom[workingOffset++] = songs[testMap.song]; // music
                rom[workingOffset++] = 0x07; // unknown
            }

            mapEntries[testMap] = entryDoorOffsets;

            // create linked maps so we know where to link to from our doors, which we created above with placeholder offsets
            Dictionary<int, int> transitionOffsets = new Dictionary<int, int>();
            foreach (DoorTransition doorTransition in testMap.connections.Keys)
            {
                if (!mapEntries.ContainsKey(testMap.connections[doorTransition]))
                {
                    writeMap(puzzles, testMap.connections[doorTransition], rom, ref workingOffset, parsedMaps, mapEntries, saveDoorsOffset, chestValuesOffset);
                }

                Dictionary<int, int> entryOffsets = mapEntries[testMap.connections[doorTransition]];
                if (entryOffsets.ContainsKey(doorTransition.destEntryId))
                {
                    transitionOffsets[doorTransition.sourceDoorId] = entryOffsets[doorTransition.destEntryId];
                }
                else
                {
                    throw new MapLoadException(testMap, "Undefined entry id: " + doorTransition.destEntryId + " to " + testMap.connections[doorTransition].mapName);
                }
            }

            // now post-process our doors since we know where they should connect.
            int doorwaysPostProcessOffset = doorwaysOffset - 0xC00000;
            // doors
            int doorNum = 0;
            foreach (int doorId in testMap.doors.Keys)
            {
                MapDoor door = testMap.doors[doorId];
                if (transitionOffsets.ContainsKey(doorId))
                {
                    rom[doorwaysPostProcessOffset + doorNum * 7 + 4] = (byte)(transitionOffsets[doorId]);
                    rom[doorwaysPostProcessOffset + doorNum * 7 + 5] = (byte)(transitionOffsets[doorId] >> 8);
                    rom[doorwaysPostProcessOffset + doorNum * 7 + 6] = (byte)(transitionOffsets[doorId] >> 16);
                }
                else
                {
                    // we get here for "none" destinations
                    if (!door.noTransition)
                    {
                        throw new MapLoadException(testMap, "Transition not found for door: " + doorId);
                    }
                }
                doorNum++;
            }

            // write save respawn locations / warp gate destinations
            foreach (int saveNum in testMap.saveLoadSpots.Keys)
            {
                int offset = saveDoorsOffset + saveNum * 23;
                // make sure we can fit the whole save door
                int thisDoorOffset = offset + 0xC00000;
                CodeChanges.setWarpPoint(rom, ref workingOffset, saveNum, testMap.saveLoadSpots[saveNum].name, thisDoorOffset);
                // make a door to it
                int startXPixel = testMap.saveLoadSpots[saveNum].entryPos.x * 32 - 0x80;
                int startYPixel = testMap.saveLoadSpots[saveNum].entryPos.y * 32 - 0x80;
                rom[offset++] = 0x00; // my direction after entering 0=up, 1=right
                rom[offset++] = (byte)(startXPixel); // 0xC0; // x pos lsb
                rom[offset++] = (byte)(startXPixel >> 8); // 0x00; // x pos msb
                rom[offset++] = (byte)(startYPixel);// 0x08; // y pos lsb
                rom[offset++] = (byte)(startYPixel >> 8);// 0x00; // y pos msb

                // offset to itself; i think this is where "return" goes
                rom[offset++] = (byte)(thisDoorOffset);
                rom[offset++] = (byte)(thisDoorOffset >> 8);
                rom[offset++] = (byte)(thisDoorOffset >> 16);
                // map data offset
                rom[offset++] = (byte)(mapSettingsOffset);
                rom[offset++] = (byte)(mapSettingsOffset >> 8);
                rom[offset++] = (byte)(mapSettingsOffset >> 16);
                // enemy data offset
                rom[offset++] = (byte)(enemiesOffset);
                rom[offset++] = (byte)(enemiesOffset >> 8);
                rom[offset++] = (byte)(enemiesOffset >> 16);
                // door data offset
                rom[offset++] = (byte)(doorwaysOffset);
                rom[offset++] = (byte)(doorwaysOffset >> 8);
                rom[offset++] = (byte)(doorwaysOffset >> 16);
                // object data offset
                int objectOffset = saveEntryPointObjectOffsets[saveNum];
                rom[offset++] = (byte)(objectOffset);
                rom[offset++] = (byte)(objectOffset >> 8);
                rom[offset++] = (byte)(objectOffset >> 16);

                rom[offset++] = 0x08; // unknown
                rom[offset++] = songs[testMap.song]; // music
                rom[offset++] = 0x07; // unknown

                offset = saveDoorsOffset + puzzles.saveNum * 23 + saveNum * 6;
                rom[offset++] = (byte)(thisDoorOffset);
                rom[offset++] = (byte)(thisDoorOffset >> 8);
                rom[offset++] = (byte)(thisDoorOffset >> 16);

                // ? pointer to 0x12037d?
                rom[offset++] = 0x7D;
                rom[offset++] = 0x03;
                rom[offset++] = 0xD2;
            }

            foreach(int chestNum in testMap.chests.Keys)
            {
                rom[chestValuesOffset + chestNum * 2] = (byte)testMap.chests[chestNum].item;
                rom[chestValuesOffset + chestNum * 2 + 1] = (byte)(testMap.chests[chestNum].item >> 8);
            }
        }
    }
}
