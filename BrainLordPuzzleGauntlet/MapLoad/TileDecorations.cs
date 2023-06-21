using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.MapLoad
{
    /// <summary>
    /// Utilities to shape walls, add shadows, and all that kind of extra shit without having to do it manually in the map file.
    /// </summary>
    public class TileDecorations : TileProcessor
    {
        private static char FLOOR_TYPE = ' ';
        private static char WALL_TYPE = 'W';
        private static char DOOR_TYPE = 'D';
        private static char HOLE_TYPE = 'H';
        private static char STAIRS_TYPE = 'S';

        static int[] wallTypesForShadowing = new int[] { WALL_TYPE, DOOR_TYPE };
        static int[] floorTypesForShadowing = new int[] { HOLE_TYPE, FLOOR_TYPE };

        static Dictionary<string, List<TileShapePattern>> allPatterns = new Dictionary<string, List<TileShapePattern>>();

        private class TileShapePattern
        {
            public string[] pattern;
            public string tileReplacement;
            public TileShapePattern(string[] pattern, string tileReplacement)
            {
                this.pattern = pattern;
                this.tileReplacement = tileReplacement;
            }
        }
        static TileDecorations()
        {
            // basic types
            Dictionary<string, string> singleTileTypes = new Dictionary<string, string>();
            singleTileTypes["Icy Floor"] = "IcyFloor";
            singleTileTypes["Empty"] = "Empty";
            singleTileTypes["Definitely Not Phantom Floor"] = "DefinitelyNotPhantomFloor";
            singleTileTypes["Cracked Floor"] = "CrackedFloor";
            singleTileTypes["Conveyor Left"] = "ConveyorLeft";
            singleTileTypes["Conveyor Right"] = "ConveyorRight";
            singleTileTypes["Conveyor Up"] = "ConveyorUp";
            singleTileTypes["Conveyor Down"] = "ConveyorDown";
            singleTileTypes["Wall Decoration 1"] = "WallDecoration1";
            singleTileTypes["Pillar Left"] = "PillarLeft";
            singleTileTypes["Pillar Right"] = "PillarRight";
            singleTileTypes["Floor Crest"] = "FloorCrest";
            singleTileTypes["Electric Floor"] = "ElectricFloor";
            singleTileTypes["Unshadowed Floor"] = "NormalFloor";
            singleTileTypes["Alternating Trap 1"] = "AlternatingTrap1";
            singleTileTypes["Alternating Trap 2"] = "AlternatingTrap2";
            singleTileTypes["Alternating Trap 3"] = "AlternatingTrap3";
            singleTileTypes["Alternating Trap 4"] = "AlternatingTrap4";

            foreach (string key in singleTileTypes.Keys)
            {
                allPatterns[key] = new List<TileShapePattern>();
                allPatterns[key].Add(new TileShapePattern(new string[] { "." }, singleTileTypes[key]));
            }
            // patterns to shape the walls properly on platinum and ruins tilesets
            // W matches only wall/door
            // w matches anything but wall and door
            // X matches out of bounds
            // anything else (we use * here) matches anything
            // these get chosen in order, so we do the more complex/specific ones first
            List<TileShapePattern> wallPatterns = new List<TileShapePattern>();
            List<TileShapePattern> floorShadowPatterns = new List<TileShapePattern>();
            List<TileShapePattern> holePatterns = new List<TileShapePattern>();
            List<TileShapePattern> phantomFloorPatterns = new List<TileShapePattern>();
            List<TileShapePattern> doorPatterns = new List<TileShapePattern>();
            List<TileShapePattern> iceBlockPatterns = new List<TileShapePattern>();

            // door socket
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                    "D",
                }, "DoorSocket"
                ));

            // outer wall shape
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*W*",
                    "W.W",
                    "OWO",
                    "*W*",
                }, "OuterWallIntersectionFourWay"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*w*",
                    "W.W",
                    "WWW",
                    "*O*",
                }, "OuterWallTop"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*W*",
                    "W.W",
                    "WWW",
                    "*w*",
                }, "OuterWallBottom"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*W*",
                    "*.W",
                    "*WO",
                    "*O*",
                }, "OuterWallLeft"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*W*",
                    "W.*",
                    "OW*",
                    "*O*",
                }, "OuterWallRight"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*w*",
                    "W.W",
                    "*W*",
                    "*w*",
                }, "OuterWallHorizontal"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "ww*",
                    "w.W",
                    "*WO",
                    "*Ww",
                }, "OuterWallTopLeft"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "ww*",
                    "w.W",
                    "*WO",
                    "*W*",
                }, "OuterWallTopLeftThick"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "ww*",
                    "W.W",
                    "DWO",
                    "*W*",
                }, "OuterWallTopLeftThick"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*ww",
                    "W.w",
                    "OW*",
                    "wW*",
                }, "OuterWallTopRight"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*ww",
                    "W.w",
                    "OW*",
                    "*W*",
                }, "OuterWallTopRightThick"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*ww",
                    "W.W",
                    "OWD",
                    "*W*",
                }, "OuterWallTopRightThick"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*Ww",
                    "*.W",
                    "wWW",
                    "*w*",
                }, "OuterWallBottomLeft"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*W*",
                    "*.W",
                    "*WW",
                    "*w*",
                }, "OuterWallBottomLeftThick"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "wW*",
                    "W.*",
                    "WWw",
                    "*w*",
                }, "OuterWallBottomRight"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*W*",
                    "W.*",
                    "WW*",
                    "*w*",
                }, "OuterWallBottomRightThick"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*W*",
                    "*.*",
                    "*W*",
                    "*W*",
                }, "OuterWallVertical"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*W*",
                    "*.*",
                    "wWw",
                    "*w*",
                }, "OuterWallBottomEnd"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*w*",
                    "w.w",
                    "*W*",
                    "*W*",
                }, "OuterWallTopEnd"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*w*",
                    "w.W",
                    "*WW",
                }, "OuterWallLeftEnd"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "*w*",
                    "W.w",
                    "WW*",
                }, "OuterWallRightEnd"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    "",
                    ".",
                    "W",
                }, "OuterWallNoConnections"
                ));

            // inner wall
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                    "X",
                }, "InnerWallNormalNonObscured"
                ));
            wallPatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                }, "InnerWallNormal"
                ));

            allPatterns["Wall"] = wallPatterns;

            // floor shadows
            floorShadowPatterns.Add(new TileShapePattern(
                new string[] {
                    "*O",
                    "O.",
                }, "NormalFloorTopLeftShadow"
                ));
            floorShadowPatterns.Add(new TileShapePattern(
                new string[] {
                    "*o",
                    "O.",
                }, "NormalFloorLeftShadow"
                ));
            floorShadowPatterns.Add(new TileShapePattern(
                new string[] {
                    "*O",
                    "o.",
                }, "NormalFloorTopShadow"
                ));
            floorShadowPatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                }, "NormalFloor"
                ));

            allPatterns["Floor"] = floorShadowPatterns;

            // hole
            holePatterns.Add(new TileShapePattern(
                new string[] {
                    "h",
                    ".",
                }, "HoleBelowFloor"
                ));
            holePatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                }, "Hole"
                ));

            allPatterns["Hole"] = holePatterns;

            // phantom floor
            phantomFloorPatterns.Add(new TileShapePattern(
                new string[] {
                    "h",
                    ".",
                }, "PhantomFloorBelowFloor"
                ));
            phantomFloorPatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                }, "PhantomFloor"
                ));

            allPatterns["Phantom Floor"] = phantomFloorPatterns;

            // door
            //doorPatterns.Add(new TileShapePattern(
                //new string[] {
                    //"O",
                    //".",
                //}, "NormalDoor"
                //));
            doorPatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                    "O",
                }, "DoorSocket"
                ));
            doorPatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                }, "NormalDoor"
                ));

            allPatterns["Door"] = doorPatterns;


            iceBlockPatterns.Add(new TileShapePattern(
                new string[] {
                    "O.",
                }, "IceBlockLeftShadow"
                ));

            iceBlockPatterns.Add(new TileShapePattern(
                new string[] {
                    ".",
                }, "IceBlock"
                ));

            allPatterns["Ice Block"] = iceBlockPatterns;

        }

        public static void processObjectOverTile(string tileType, Map map, char[,] tileTypeMap, int x, int y, out int floorType, out string objType)
        {
            floorType = -1;
            objType = tileType;
            if (tileType.Contains(":"))
            {
                string[] tokens = tileType.Split(new char[] { ':' });
                floorType = TileDecorations.chooseTile(map, tokens[0], tileTypeMap, x, y);
                objType = tokens[1];
            }
        }

        public bool supportsTile(string tileType)
        {
            return allPatterns.ContainsKey(tileType);
        }

        public static char[,] getTileTypeMap(char[,] parsedData, Map map, Dictionary<char, string> dataKey)
        {
            // generate a simplified version of the loaded map that only indicates basic tile types: floor, wall, door, hole.
            // this is used to determine how to shape tiles around other types of tiles
            //string[] floorTypeNames = new string[] { "Floor", "IcyFloor", "Ball", "Weighted Switch", "Toggle Switch", "Rock", "Entry", "Cracked Floor", "Falling Ice", "Falling Object Switch", "Save Point", "Controllable Platform", "Conveyor" };
            string[] floorTypeNames = new string[] { "Floor", "IcyFloor", "Switch", "Definitely Not Phantom Floor", "Entry" };
            string[] wallTypeNames = new string[] { "Wall", "Message Box", "Pillar", "Up Stairs" };
            string[] doorTypeNames = new string[] { "Weighted Door", "Toggle Door", "Unlocked Door", "Locked Door" };
            string[] holeTypeNames = new string[] { "Hole", "Controllable Platform Over Hole", "Moving Platform", "Phantom Floor" };
            string[] stairsTypeNames = new string[] { "Down Stairs" };

            Dictionary<string[], char> simplifiedTypes = new Dictionary<string[], char>();
            simplifiedTypes[floorTypeNames] = FLOOR_TYPE;
            simplifiedTypes[wallTypeNames] = WALL_TYPE;
            simplifiedTypes[doorTypeNames] = DOOR_TYPE;
            simplifiedTypes[holeTypeNames] = HOLE_TYPE;
            simplifiedTypes[stairsTypeNames] = STAIRS_TYPE;
            char[,] tileTypeMap = new char[parsedData.GetLength(0), parsedData.GetLength(1)];
            for (int y = 0; y < parsedData.GetLength(1); y++)
            {
                for (int x = 0; x < parsedData.GetLength(0); x++)
                {
                    tileTypeMap[x, y] = parsedData[x, y];
                    if (dataKey.ContainsKey(tileTypeMap[x, y]))
                    {
                        string tileType = dataKey[tileTypeMap[x, y]];
                        if (map.tileset.unsupportedTiles.ContainsKey(tileType))
                        {
                            tileType = map.tileset.unsupportedTiles[tileType];
                        }
                        // close match
                        foreach (string[] types in simplifiedTypes.Keys)
                        {
                            foreach (string typeName in types)
                            {
                                if (tileType.Contains(typeName))
                                {
                                    tileTypeMap[x, y] = simplifiedTypes[types];
                                }
                            }
                        }
                        // exact match override
                        foreach (string[] types in simplifiedTypes.Keys)
                        {
                            foreach(string typeName in types)
                            {
                                if(tileType == typeName)
                                {
                                    tileTypeMap[x, y] = simplifiedTypes[types];
                                }
                            }
                        }
                    }
                }
            }
            return tileTypeMap;
        }

        public static byte chooseDoorTile(Map map, char[,] tileTypeMap, int x, int y)
        {
            // place normal doors if wall above, or door sockets if not
            byte value = map.tileset["DoorSocket"];
            if (TileDecorations.checkAdjacent(tileTypeMap, x, y, 0, -1) == WALL_TYPE || TileDecorations.checkAdjacent(tileTypeMap, x, y, 0, -1) == DOOR_TYPE)
            {
                value = map.tileset["NormalDoor"];
            }
            return value;
        }

        private static byte chooseTile(List<TileShapePattern> patterns, Map map, char[,] tileTypeMap, int x, int y)
        {
            byte value = map.tileset["Empty"];
            foreach (TileShapePattern tilePattern in patterns)
            {
                if (matchTilePattern(map, tilePattern, tileTypeMap, x, y))
                {
                    value = map.tileset[tilePattern.tileReplacement];
                    break;
                }
            }
            return value;
        }

        private static bool matchTilePattern(Map map, TileShapePattern tileSwap, char[,] tileTypeMap, int x, int y)
        {
            // check if tile at given position matches the tile pattern
            int centerX = -1;
            int centerY = -1;
            // find position of tile to change
            for (int _y=0; _y < tileSwap.pattern.Length; _y++)
            {
                for (int _x = 0; _x < tileSwap.pattern[_y].Length; _x++)
                {
                    if(tileSwap.pattern[_y][_x] == '.')
                    {
                        centerX = _x;
                        centerY = _y;
                    }
                }
            }

            // check pattern around it for a match
            for (int _y = 0; _y < tileSwap.pattern.Length; _y++)
            {
                for (int _x = 0; _x < tileSwap.pattern[_y].Length; _x++)
                {
                    char tileType = tileSwap.pattern[_y][_x];
                    int xOffset =  _x - centerX;
                    int yOffset =  _y - centerY;
                    switch(tileType)
                    {
                        case 'W':
                            // match wall only
                            if (checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) != WALL_TYPE)
                            {
                                return false;
                            }
                            break;
                        case 'D':
                            // match door only
                            if (checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) != DOOR_TYPE)
                            {
                                return false;
                            }
                            break;
                        case 'O':
                            // match wall or door
                            if (checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) != WALL_TYPE && checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) != DOOR_TYPE)
                            {
                                return false;
                            }
                            break;
                        case 'w':
                            // match non-wall
                            if (checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) == WALL_TYPE)
                            {
                                return false;
                            }
                            break;
                        case 'd':
                            // match non-door
                            if (checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) == DOOR_TYPE)
                            {
                                return false;
                            }
                            break;
                        case 'o':
                            // match non-wall and non-door
                            if (checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) == WALL_TYPE || checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) == DOOR_TYPE)
                            {
                                return false;
                            }
                            break;
                        case 'h':
                            // match non-hole
                            if (checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) == HOLE_TYPE)
                            {
                                return false;
                            }
                            break;
                        case 'X':
                            // if we're not using empty space, skip this
                            if(map.properties.ContainsKey("emptySpaceFiller") && map.properties["emptySpaceFiller"] != "Empty")
                            {
                                return false;
                            }
                            // match empty space (out of bounds area)
                            if (checkAdjacent(tileTypeMap, x, y, xOffset, yOffset) != -1)
                            {
                                return false;
                            }
                            break;
                    }
                }
            }
            return true;
        }

        public byte processTile(string tileType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            return chooseTile(allPatterns[tileType], map, tileTypeMap, x, y);
        }

        public static byte chooseTile(Map map, string tileType, char[,] tileTypeMap, int x, int y)
        {
            return chooseTile(allPatterns[tileType], map, tileTypeMap, x, y);
        }

        public static bool isDoor(char[,] tileTypeMap, int x, int y)
        {
            return checkAdjacent(tileTypeMap, x, y, 0, 0) == DOOR_TYPE;
        }

        public static bool isFloor(char[,] tileTypeMap, int x, int y)
        {
            return checkAdjacent(tileTypeMap, x, y, 0, 0) == FLOOR_TYPE;
        }

        public static bool isWall(char[,] tileTypeMap, int x, int y)
        {
            return checkAdjacent(tileTypeMap, x, y, 0, 0) == WALL_TYPE;
        }

        private static int checkAdjacent(char[,] parsedData, int currentX, int currentY, int xOffset, int yOffset)
        {
            // return the tile at the given position, or -1 if out of map bounds
            int returnVal = -1;
            int xDim = parsedData.GetLength(0);
            int yDim = parsedData.GetLength(1);
            int newX = currentX + xOffset;
            int newY = currentY + yOffset;
            if (newX >= 0 && newX < xDim && newY >= 0 && newY < yDim)
            {
                returnVal = parsedData[newX, newY];
            }
            if (returnVal == 0)
            {
                // empty space - process the same as edge of map
                returnVal = -1;
            }
            return returnVal;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
