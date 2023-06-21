using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.MapLoad
{
    /// <summary>
    /// Make pushable objects: rocks, balls, ice, etc.
    /// </summary>
    public class PushableObjectMaker : TileProcessor
    {
        //private static Dictionary<string, byte> objectTypes = new Dictionary<string, byte>();
        private class PushableObject
        {
            public byte objectType;
            public byte objectP1;
            public int switchP1;
            public PushableObject(byte objectType, byte objectP1, int switchP1)
            {
                this.objectType = objectType;
                this.objectP1 = objectP1;
                this.switchP1 = switchP1;
            }
        }
        private static Dictionary<string, PushableObject> objectTypes = new Dictionary<string, PushableObject>();

        static PushableObjectMaker()
        {
            // 0x01 is a chest .. array of chest items is at C5:5576, platinum2 chest is at 556AE
            // 0x0C is a trap?  needs to be configured
            // 0x0D is a light switch
            // 0x0E is a fountain, but needs a message/event offset
            // 0x0A and 0x0B seem to be the same .. summonable ice
            // 0x06 is npc, p1 is dir, p2 is type
            // 0x17 is that tower chest that makes floors when you shoot it, the pattern is seemingly fixed around the object
            // 0x18 uh idk it just plays a sound
            // 0x1c the spiky balls from ice castle, p1 is the movement pattern
            // 0x1d just a solid invisible object?
            // 0x1e the randomly moving platforms from that one ruins room
            // 0x20 the gray platforms except they only move when you step on them
            // 0x32 is a swirly effect on layer 2
            // 0x33 is the warp gate from toronto to desert
            // 0x35 the droog dragon
            // 0x37 cutscene thing? makes me invisible and i cannot move
            // 0x39 the dragon again but he dies this time
            // 0x3a uhh this makes everything all wacky
            // 0x3b shaking effect
            // 0x3c window opening effect from credits
            // 0x3d the "fin" from the credits
            // 0x3e game freezes with black screen
            objectTypes["(Pushable )?Rock$"] = new PushableObject(0x07, 0, -1);
            objectTypes["Red Ball$"] = new PushableObject(0x10, 0, -1);
            objectTypes["(Pushable )?Bouncing Ball$"] = new PushableObject(0x11, 255, -1); // other p1 values make it stop moving after a bit
            objectTypes["(Pushable )?Ball$"] = new PushableObject(0x08, 0, -1);
            objectTypes["(Pushable )?Tower Wall$"] = new PushableObject(0x07, 1, -1);
            objectTypes["(Pushable )?Ruins Wall$"] = new PushableObject(0x07, 2, -1);
            objectTypes["(Pushable )?Droog Statue$"] = new PushableObject(0x07, 3, -1);
            objectTypes["(Pushable )?Sliding Ice$"] = new PushableObject(0x14, 0, -1); // ball-like
            objectTypes["(Pushable )?Bouncing Ice$"] = new PushableObject(0x15, 0, -1); // blue ball-like
            // ice patterns
            // 0x10 freeze game
            // 0x11 go straight left
            // 0x12 go straight down
            // 0x13 go down, then left
            // 0x14 go straight right
            // 0x15 randomly move left or right
            // 0x16 go down, then right
            // 0x17 go down, then left/right randomly
            // 0x18 go straight up
            // 0x19 go left two, then up?
            // 0x1a go up and down randomly
            // 0x1b go randomly left/up/down
            // 0x1c go up, then right?
            // 0x1d up/left/right randomly
            // 0x1e right/up/down randomly
            // 0x1f completely random
            // 0x2x, 0x3x, etc seem to do the same but with greater delays
            objectTypes["(Pushable )?Moving Ice$"] = new PushableObject(0x12, 0x1F, -1);
            objectTypes["(Pushable )?Ice$"] = new PushableObject(0x12, 0, -1); // single block push .. p1 is 0x14, 0x11 for ice castle auto movement ones?
            objectTypes["(Controllable )?Platform$"] = new PushableObject(0x1F, 0, -1);
            objectTypes["Falling Ice (\\d\\d?)$"] = new PushableObject(0x0B, 0, 0);
            objectTypes["Falling Ball (\\d\\d?)$"] = new PushableObject(0x0B, 0, 1);
        }

        private bool match(string objTypeName)
        {
            foreach(String regex in objectTypes.Keys)
            {
                if(Regex.Match(objTypeName, regex).Success)
                {
                    return true;
                }
            }
            return false;
        }
        public bool supportsTile(string objTypeName)
        {
            if(match(objTypeName))
            {
                return true;
            }
            if(objTypeName.Contains(":"))
            {
                string[] tokens = objTypeName.Split(new char[] { ':' });
                return match(tokens[1]);
            }
            return false;
        }

        public byte processTile(string tileType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            int floorUnderObject = -1;
            string objectType = tileType;
            // allow specification of floor underneath with ":"
            TileDecorations.processObjectOverTile(tileType, map, tileTypeMap, x, y, out floorUnderObject, out objectType);

            int iceNumber = -1;
            PushableObject objValue = null;

            foreach (String regex in objectTypes.Keys)
            {
                Match m = Regex.Match(objectType, regex);
                if (m.Success)
                {
                    if (objectType.Contains("Falling "))
                    {
                        iceNumber = Int32.Parse(m.Groups[1].Value);
                    }
                    objValue = objectTypes[regex];
                    break;
                }
            }

            byte value = floorUnderObject == -1 ? TileDecorations.chooseTile(map, "Floor", tileTypeMap, x, y) : (byte)floorUnderObject;
            MapObject mapObj = new MapObject();
            mapObj.x = (byte)((x + map.mapPlacementOffsetX) * 4);
            mapObj.y = (byte)((y + map.mapPlacementOffsetY) * 2);
            mapObj.type = objValue.objectType;
            mapObj.p1 = objValue.objectP1;
            mapObj.p2 = 0;
            mapObj.p3 = 0;
            mapObj.p4 = 0;
            map.otherObjects.Add(mapObj);
            if (iceNumber >= 0)
            {
                map.fallingIce[iceNumber] = mapObj;
                mapObj.num = objValue.switchP1;
            }
            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
