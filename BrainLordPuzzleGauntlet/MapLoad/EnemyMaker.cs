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
    /// Creates enemy objects
    /// </summary>
    public class EnemyMaker : TileProcessor
    {
        private static string regex = RegexUtil.makeRegexForNumberedLiteral("Enemy", 1);

        public bool supportsTile(string tileType)
        {
            return Regex.Match(tileType, regex).Success;
        }

        public byte processTile(string tileType, Puzzles puzzles, Map map, char[,] tileTypeMap, int x, int y)
        {
            int floorUnderObject = -1;
            string objectType = tileType;
            // allow specification of floor underneath with ":"
            TileDecorations.processObjectOverTile(tileType, map, tileTypeMap, x, y, out floorUnderObject, out objectType);
            // match to extract id
            Match m = Regex.Match(tileType, regex);

            byte value = floorUnderObject == -1 ? TileDecorations.chooseTile(map, "Floor", tileTypeMap, x, y) : (byte)floorUnderObject;
            // 0x00: nothing
            // 0x01: electric brain thing
            // 0x02: mouse
            // 0x03: tower wall eyeball; they work anywhere
            // 0x04: desert tentacles; they seem to iverride the spawn limit and spawn forever
            // 0x05: skull
            // 0x06: blue fat bats from tower path
            // 0x07: large bouncing orange fire sphere; unkillable
            // 0x08: scorpion
            // 0x09: the platinum sucky balls
            // 0x0a: nothing?
            // 0x0b: mice again
            // 0x0c: toronto bugs
            // 0x0d: the desert tornados that suck you around; they keep respawning
            // 0x0e: horizontal-facing ruins laser bots
            // 0x0f: nothing?
            // 0x10: nothing?
            // 0x11: nothing?
            // 0x12: nothing?
            // 0x13: nothing?
            // 0x14: nothing?
            // 0x15: large pink skeleton
            // 0x16: teleporting ruins wizard
            // 0x17: chubby green dude with whips
            // 0x18: bow and arrow guy from tower
            // 0x19: large turquoise knight
            // 0x1a: this crashed the fucking emulator do not use
            // 0x1b: the bats from outside droog
            // 0x1c: vertical-facing ruins laser bots
            // 0x1d: lux robot from ruins
            // 0x1e: mimic box
            // 0x1f: knife guy from the toronto path
            // 0x20: pink bald guy with knife, i forgot where these live
            // 0x21: platinum guy with sword
            // 0x22: 0x17 but yellow and always walks like he's on ice lol
            // 0x23: small gray skeleton
            // 0x24: ball and chain guy
            // 0x25: ufo things from the big rooms in ruins
            // 0x26: don't attack the stone statue
            // 0x27: the droog heads that shoot lava balls
            // 0x28: the droog heads that explode and make that awful stretching sound
            // 0x29: platinum statue archer
            // 0x2a: platinum statue knife cyclops demon thing
            // 0x2b: platinum statue ball and chain guy
            // 0x2c: nothing?
            // 0x2d: green scorpion
            // 0x2e: orange version of 0x17
            // 0x2f: green clothes archer guy .. was he from droog outside?
            // 0x30: gray version of 0x19 .. is this guy used?
            // 0x31: orange small skeleton
            // 0x32: green ball and chain guy
            // 0x33: probably the cockroach boss if the next one is ruins boss; no graphics
            // 0x34: i think this is the ruins boss? lags and i get sucked around with ball rolling sounds; he doesn't show up though, possible he's at a fixed location?
            // 0x35: the beginning of ramus fight including cutscene; fight doesn't seem to work, and fucks up my player palette
            // 0x36: probably the end guy; nothing seems to show up
            // 0x37: permanent black screen enemy

            int enemyNumber = Int32.Parse(m.Groups[1].Value);
            Enemy enemy = new Enemy();
            enemy.x = (byte)((x + map.mapPlacementOffsetX) * 2); // why are these * 2 but objects are * 4?
            enemy.y = (byte)((y + map.mapPlacementOffsetY) * 2);
            enemy.type = (byte)enemyNumber;
            //enemy.b3 = 0x12; // this is its item drop id, i think .. we can add this later
            byte enemyB4 = 0x00;
            if(!map.booleanPropertyEnabled("infinite respawns", false))
            {
                enemyB4 |= 0x20;
            }
            enemyB4 |= (byte)(map.intPropertyValue("enemy spawn limit", 0) & 0x0F);

            enemy.b4 = enemyB4;  // this number is a max that can be on-screen; others will not spawn
            map.enemies.Add(enemy);
            return value;
        }

        public void postProcess(Puzzles puzzles, Map map, char[,] tileTypeMap)
        {
            // nothing
        }
    }
}
