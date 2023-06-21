using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    public class CodeChanges
    {
        /// <summary>
        /// Allocate a new spot in ROM for the rewards granted by each chest.  The chest object structure then refers to them by index.
        /// </summary>
        public static int setUpNewChestBlock(byte[] rom, ref int workingOffset)
        {
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, 190 * 2);
            int chestValuesOffset = workingOffset;
            int chestValuesRomOffset = chestValuesOffset + 0xC00000;
            workingOffset += 190 * 2;

            rom[0x145F8D] = (byte)(chestValuesRomOffset);
            rom[0x145F8E] = (byte)(chestValuesRomOffset >> 8);
            rom[0x145F94] = (byte)(chestValuesRomOffset >> 16);

            rom[0x145FBE] = (byte)(chestValuesRomOffset);
            rom[0x145FBF] = (byte)(chestValuesRomOffset >> 8);
            rom[0x145FC5] = (byte)(chestValuesRomOffset >> 16);

            rom[0x14604F] = (byte)(chestValuesRomOffset);
            rom[0x146050] = (byte)(chestValuesRomOffset >> 8);
            rom[0x146056] = (byte)(chestValuesRomOffset >> 16);

            rom[0x1460D0] = (byte)(chestValuesRomOffset);
            rom[0x1460D1] = (byte)(chestValuesRomOffset >> 8);
            rom[0x1460D7] = (byte)(chestValuesRomOffset >> 16);

            // patch out this check for whether a chest is locked, since it's hardcoded to look in bank C5
            rom[0x145F33] = 0xA9;
            rom[0x145F34] = 0x00;
            rom[0x145F35] = 0x00;
            rom[0x145F36] = 0xEA;
            rom[0x145F37] = 0xEA;
            // load the chest reward ID from RAM instead of ROM in bank C5
            rom[0x145F83] = 0xB5;
            rom[0x145F84] = 0x5C;
            rom[0x145FB4] = 0xB5;
            rom[0x145FB5] = 0x5C;
            rom[0x146045] = 0xB5;
            rom[0x146046] = 0x5C;
            rom[0x1460A2] = 0xB5;
            rom[0x1460A3] = 0x5C;
            rom[0x1460C6] = 0xB5;
            rom[0x1460C7] = 0x5C;

            return chestValuesOffset;
        }

        /// <summary>
        /// Allocate a new spot in ROM for all the save points.  For each one this is a 23-byte door structure plus a 6-byte structure pointing to it (and something else?)
        /// </summary>
        public static int setUpNewSaveBlock(byte[] rom, ref int workingOffset, int numSaves)
        {
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, numSaves * (23 + 6));
            int saveDoorsOffset = workingOffset;
            workingOffset += numSaves * (23 + 6);

            int savePointRefsOffset = saveDoorsOffset + numSaves * 23;
            int saveDoorsRomOffset = savePointRefsOffset + 0xC00000;

            // change where the game loads save doors from
            // $D4/0E93 A9 7B 5B    LDA #$5B7B              A:009C X:0580 Y:009C P:envmxdizc **
            // ..
            // $D4/0E9A A9 C6       LDA #$C6                A:5B7B X:0580 Y:009C P:envMxdizc **
            rom[0x140E94] = (byte)(saveDoorsRomOffset);
            rom[0x140E95] = (byte)(saveDoorsRomOffset >> 8);
            rom[0x140E9B] = (byte)(saveDoorsRomOffset >> 16);

            // also this bank when we write saveram at a savepoint
            // $D4/38DD A9 C5       LDA #$C5                A:1D24 X:1080 Y:0002 P:envMxdizC
            rom[0x1438DE] = (byte)(saveDoorsRomOffset >> 16);

            return saveDoorsOffset;
        }

        /// <summary>
        /// QOL change to reduce the delay when pushing objects.
        /// </summary>
        public static void fasterObjectPushing(byte[] rom)
        {
            // change:
            /*
            $C0/6908 F6 64       INC $64,x  [$00:08E4]   A:0000 X:0880 Y:1100 P:envMxdiZc
            $C0/690A B5 64       LDA $64,x  [$00:08E4]   A:0000 X:0880 Y:1100 P:envMxdizc
            $C0/690C C9 19       CMP #$19                A:0002 X:0880 Y:1100 P:envMxdizc
            ..
            $D4/3AF3 AD E4 08    LDA $08E4  [$00:08E4]   A:03E4 X:1100 Y:0200 P:envMxdizC
            $D4/3AF6 D0 04       BNE $04    [$3AFC]      A:030C X:1100 Y:0200 P:envMxdizC
            $D4/3AFC C9 18       CMP #$18                A:030C X:1100 Y:0200 P:envMxdizC
            08 / 07 is nice for fast pushing
            doesn't work on balls though
            $D4/3E19 AD E4 08    LDA $08E4  [$00:08E4]   A:01E4 X:1100 Y:0200 P:envMxdizC
            $D4/3E1C D0 04       BNE $04    [$3E22]      A:0101 X:1100 Y:0200 P:envMxdizC
            $D4/3E22 C9 18       CMP #$18                A:0101 X:1100 Y:0200 P:envMxdizC
            here's the ball spot ^
            this should be all of them - same block of code copied into each obj callback:
            143AFD
            143E23
            144452
            14508E
            14535D
            14581D
             */
            rom[0x690D] = 0x08;
            rom[0x143AFD] = 0x07;
            rom[0x143E23] = 0x07;
            rom[0x144452] = 0x07;
            rom[0x14508E] = 0x07;
            rom[0x14535D] = 0x07;
            rom[0x14581D] = 0x07;
        }

        /// <summary>
        /// Set the name and location of a warp gate target.
        /// </summary>
        public static void setWarpPoint(byte[] rom, ref int workingOffset, int warpId, string warpName, int doorOffset)
        {
            /*
            $C0/AB19 A9 7B 5B    LDA #$5B7B              A:E7A4 X:0600 Y:0029 P:eNvmxdizc
            $C0/AB1C 85 85       STA $85    [$00:0085]   A:5B7B X:0600 Y:0029 P:envmxdizc
            $C0/AB1E E2 20       SEP #$20                A:5B7B X:0600 Y:0029 P:envmxdizc
            $C0/AB20 A9 C6       LDA #$C6                A:5B7B X:0600 Y:0029 P:envMxdizc
            $C0/AB22 85 87       STA $87    [$00:0087]   A:5BC6 X:0600 Y:0029 P:eNvMxdizc
            ^ 65B7B for debug warp gate offsets list .. destination door structure, name .. 6 bytes apiece
            $C0/A5E4 A9 7B 5B    LDA #$5B7B              A:E7A4 X:0600 Y:000B P:eNvmxdizc
            $C0/A5E7 85 85       STA $85    [$00:0085]   A:5B7B X:0600 Y:000B P:envmxdizc
            $C0/A5E9 E2 20       SEP #$20                A:5B7B X:0600 Y:000B P:envmxdizc
            $C0/A5EB A9 C6       LDA #$C6                A:5B7B X:0600 Y:000B P:envMxdizc
            ^ this is the one for normal warpgates .. it's looking at 7e81A8,x (0x50) to determine whether to include them.. are these event flags? they get set when i save at each savepoint

                $C2/CE2C D0 02       BNE $02    [$CE30]      A:0001 X:0600 Y:000B P:envMxdizc
                if i change this to BRA (0x80) it should just ignore the value and include everything
                or do these actually work, if i add/replace savepoints?
                our first savepoint seems to set these properly .. Arcs is hardcoded to be 0 and to get added to the list automatically

            $C0/AB84 C9 3B 00    CMP #$003B              A:0001 X:0600 Y:0029 P:envmxdizc
            ^ number of debug warpgate spots
             */

            int warpOffset = 0x65B7B + warpId * 6;
            rom[warpOffset] = (byte)doorOffset;
            rom[warpOffset + 1] = (byte)(doorOffset >> 8);
            rom[warpOffset + 2] = (byte)(doorOffset >> 16);
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, warpName.Length + 1);
            int nameOffset = workingOffset + 0xC00000;
            rom[warpOffset + 3] = (byte)nameOffset;
            rom[warpOffset + 4] = (byte)(nameOffset >> 8);
            rom[warpOffset + 5] = (byte)(nameOffset >> 16);
            foreach (char c in warpName)
            {
                rom[workingOffset++] = TextConversion.convertChar(c);
            }
            rom[workingOffset++] = 0xF7; // end string
        }

        /// <summary>
        /// Inject overridden key names and descriptions
        /// </summary>
        public static void processKeyNames(byte[] rom, ref int workingOffset, Puzzles puzzles)
        {
            foreach (string propertyName in puzzles.otherProperties.Keys)
            {
                Match regexMatch = Regex.Match(propertyName, "key(\\d\\d?).(name|desc)");
                if (regexMatch.Success)
                {
                    int keyNum = Int32.Parse(regexMatch.Groups[1].Value);
                    if (propertyName.EndsWith("name"))
                    {
                        CodeChanges.setKeyName(rom, ref workingOffset, keyNum, puzzles.otherProperties[propertyName]);
                    }
                    else if (propertyName.EndsWith("desc"))
                    {
                        CodeChanges.setKeyDescription(rom, ref workingOffset, keyNum, puzzles.otherProperties[propertyName]);
                    }
                }
            }
        }

        /// <summary>
        /// Inject key name
        /// </summary>
        private static void setKeyName(byte[] rom, ref int workingOffset, int keyNum, string name)
        {
            // offsets start at 60210, and point to a list of two offsets for the name and description
            int keyOffset = rom[0x60210 + keyNum * 3] + (rom[0x60210 + keyNum * 3 + 1] << 8) + (rom[0x60210 + keyNum * 3 + 2] << 16) - 0xC00000;
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, name.Length + 1);
            int romWorkingOffset = workingOffset + 0xC00000;
            rom[keyOffset] = (byte)romWorkingOffset;
            rom[keyOffset + 1] = (byte)(romWorkingOffset >> 8);
            rom[keyOffset + 2] = (byte)(romWorkingOffset >> 16);

            foreach (char c in name)
            {
                rom[workingOffset++] = TextConversion.convertChar(c);
            }
            rom[workingOffset++] = 0xF7; // end string
        }

        /// <summary>
        /// Inject key description
        /// </summary>
        private static void setKeyDescription(byte[] rom, ref int workingOffset, int keyNum, string description)
        {
            // offsets start at 60210, and point to a list of two offsets for the name and description
            int keyOffset = rom[0x60210 + keyNum * 3] + (rom[0x60210 + keyNum * 3 + 1] << 8) + (rom[0x60210 + keyNum * 3 + 2] << 16) - 0xC00000;
            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, description.Length + 6);
            int romWorkingOffset = workingOffset + 0xC00000;
            rom[keyOffset + 3] = (byte)romWorkingOffset;
            rom[keyOffset + 4] = (byte)(romWorkingOffset >> 8);
            rom[keyOffset + 5] = (byte)(romWorkingOffset >> 16);

            // vanilla formatting:
            // (text color highlight)(item name)-
            // (description)
            rom[workingOffset++] = 0x82; // highlight color set
            rom[workingOffset++] = 0x8B; // name of item
            rom[workingOffset++] = 0x82; // highlight color unset
            rom[workingOffset++] = 0x68; // -
            rom[workingOffset++] = 0xF9; // newline

            foreach (char c in description)
            {
                rom[workingOffset++] = TextConversion.convertChar(c);
            }
            rom[workingOffset++] = 0xF7; // end string
        }

        /// <summary>
        /// Allow any key to work with any tileset
        /// </summary>
        public static void lockedDoorTileFix(byte[] rom, ref int workingOffset, Dictionary<string, Tileset> allTilesets)
        {
            // in vanilla, the tile shown when a locked door is opened is determined by a table of offsets for each key
            // at C5:5759.  this limits our options for keys, so replace the loading of this with a block of code that
            // loads these same values based on which tileset we currently have loaded in RAM instead of from a fixed
            // table in ROM.  the 24-bit RAM value at 7e00d5 should be the same offset at the ROM position indicated by
            // TilesetStartOffset for the tileset.  note that the values at the offsets for the "alt" tilesets (platinum
            // dark rooms, phantom floor enabled tower/ruins) are the same, so those should work here also

            // replaced block:
            /*
            $C0/A090 A9 59 57    LDA #$5759              A:00A8 X:0600 Y:00A8 P:envmxdizc
            $C0/A093 85 82       STA $82    [$00:0082]   A:5759 X:0600 Y:00A8 P:envmxdizc
            $C0/A095 E2 20       SEP #$20                A:5759 X:0600 Y:00A8 P:envmxdizc
            $C0/A097 A9 C5       LDA #$C5                A:5759 X:0600 Y:00A8 P:envMxdizc
            $C0/A099 85 84       STA $84    [$00:0084]   A:57C5 X:0600 Y:00A8 P:eNvMxdizc
            $C0/A09B C2 20       REP #$20                A:57C5 X:0600 Y:00A8 P:eNvMxdizc
            $C0/A09D B7 82       LDA [$82],y[$C5:5801]   A:57C5 X:0600 Y:00A8 P:eNvmxdizc
            $C0/A09F 85 85       STA $85    [$00:0085]   A:5835 X:0600 Y:00A8 P:envmxdizc
            $C0/A0A1 C8          INY                     A:5835 X:0600 Y:00A8 P:envmxdizc
            $C0/A0A2 C8          INY                     A:5835 X:0600 Y:00A9 P:envmxdizc
            $C0/A0A3 E2 20       SEP #$20                A:5835 X:0600 Y:00AA P:envmxdizc
            $C0/A0A5 B7 82       LDA [$82],y[$C5:5803]   A:5835 X:0600 Y:00AA P:envMxdizc
            $C0/A0A7 85 87       STA $87    [$00:0087]   A:58C5 X:0600 Y:00AA P:eNvMxdizc
            $C0/A0A9 C2 20       REP #$20                A:58C5 X:0600 Y:00AA P:eNvMxdizc
             */
            // new block:
            // (registers are 16 bit here)
            // LDA $D5
            // (for each tileset)
            // CMP #xxxx (16 bit LSBs of TilesetStartOffset)
            // BNE next
            // SEP #20
            // LDA #yy (8 bit MSB/bank of door tile offset for tileset)
            // STA $87
            // REP #20
            // LDA #yyyy (16 bit LSBs of door tile offset for tileset)
            // STA $85
            // RTL
            // next:
            // ... (next loop iter)
            // also load a default at the end
            // (registers should be 16 bit after)

            CodeGenerationUtils.ensureSpaceInBank(ref workingOffset, 300);
            int subrOffset = workingOffset + 0xC00000;
            for(int off = 0xA090; off <= 0xA0AA; off++)
            {
                rom[off] = 0xEA;
            }
            rom[0xA090] = 0x22;
            rom[0xA091] = (byte)subrOffset;
            rom[0xA092] = (byte)(subrOffset >> 8);
            rom[0xA093] = (byte)(subrOffset >> 16);
            // there's also an exact duplicate of it here for when we load a map with a locked door already opened
            for (int off = 0x27C98; off <= 0x27CB2; off++)
            {
                rom[off] = 0xEA;
            }
            rom[0x27C98] = 0x22;
            rom[0x27C99] = (byte)subrOffset;
            rom[0x27C9A] = (byte)(subrOffset >> 8);
            rom[0x27C9B] = (byte)(subrOffset >> 16);

            rom[workingOffset++] = 0xA5;
            rom[workingOffset++] = 0xD5;
            foreach (Tileset tileset in allTilesets.Values)
            {
                int srcMapStart = tileset.sourceMapStart;
                rom[workingOffset++] = 0xC9;
                rom[workingOffset++] = rom[srcMapStart];
                rom[workingOffset++] = rom[srcMapStart + 1];

                rom[workingOffset++] = 0xD0;
                rom[workingOffset++] = 0x0E;

                rom[workingOffset++] = 0xE2;
                rom[workingOffset++] = 0x20;

                rom[workingOffset++] = 0xA9;
                rom[workingOffset++] = (byte)((tileset.unlockedDoorTileOffset >> 16) + 0xc0);

                rom[workingOffset++] = 0x85;
                rom[workingOffset++] = 0x87;

                rom[workingOffset++] = 0xC2;
                rom[workingOffset++] = 0x20;

                rom[workingOffset++] = 0xA9;
                rom[workingOffset++] = (byte)(tileset.unlockedDoorTileOffset);
                rom[workingOffset++] = (byte)(tileset.unlockedDoorTileOffset >> 8);

                rom[workingOffset++] = 0x85;
                rom[workingOffset++] = 0x85;

                rom[workingOffset++] = 0x6B;
            }

            rom[workingOffset++] = 0xE2;
            rom[workingOffset++] = 0x20;

            rom[workingOffset++] = 0xA9;
            rom[workingOffset++] = (byte)((allTilesets["Platinum"].unlockedDoorTileOffset >> 16) + 0xc0);

            rom[workingOffset++] = 0x85;
            rom[workingOffset++] = 0x87;

            rom[workingOffset++] = 0xC2;
            rom[workingOffset++] = 0x20;

            rom[workingOffset++] = 0xA9;
            rom[workingOffset++] = (byte)(allTilesets["Platinum"].unlockedDoorTileOffset);
            rom[workingOffset++] = (byte)(allTilesets["Platinum"].unlockedDoorTileOffset >> 8);

            rom[workingOffset++] = 0x85;
            rom[workingOffset++] = 0x85;

            rom[workingOffset++] = 0x6B;
        }

        /// <summary>
        /// Set the starting door for a new game.
        /// </summary>
        public static void setStartingDoor(byte[] rom, int doorOffset)
        {
            rom[0xD61] = (byte)(doorOffset);
            rom[0xD62] = (byte)(doorOffset >> 8);
            rom[0xD69] = (byte)(doorOffset >> 16);
        }
    }
}
