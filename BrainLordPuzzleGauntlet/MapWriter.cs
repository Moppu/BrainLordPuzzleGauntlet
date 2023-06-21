using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    /// <summary>
    /// Util to write Brainlord maps using their run-length encoding (RLE) compression
    /// </summary>
    public class MapWriter
    {
        public static int writeMap(byte[] rom, Map map, ref int workingOffset)
        {
            // $C8/C080 C2 20       REP #$20                A:007C X:0400 Y:0020 P:envmxdizC
            // RLE compression, [tile value] [num]
            // tile value is 9 bits and its MSB (which we largely don't use, because tile IDs we use don't go that high) is in the LSB of num
            // so all in all it's sets of 16 bits where:
            // TTTTTTTTnnnnnnnT
            // T = Tile ID
            // n = number of tiles - 1
            int dataPos = 0;
            List<byte> compressedMapData = new List<byte>();
            while (dataPos < map.data.Length)
            {
                byte nextTile = map.data[dataPos];
                int numOfThisTile = 1;
                int dataCheckPos = dataPos + 1;
                while(numOfThisTile < 0x7F && dataCheckPos < map.data.Length && map.data[dataCheckPos] == nextTile)
                {
                    numOfThisTile++;
                    dataCheckPos++;
                }

                numOfThisTile--;
                numOfThisTile *= 2;

                compressedMapData.Add(nextTile);
                compressedMapData.Add((byte)numOfThisTile);
                dataPos = dataCheckPos;
            }

            // don't allow data to flow into another (0x10000 byte) bank, otherwise the loading does not work.
            if ((workingOffset >> 16) != ((workingOffset + compressedMapData.Count) >> 16))
            {
                workingOffset += (0x10000 - (workingOffset & 0xFFFF));
            }
            int startPos = workingOffset;
            // now write the data
            foreach(byte b in compressedMapData)
            {
                rom[workingOffset++] = b;
            }
            // and tell the caller where we stuck it so it can point to it from other structures
            return startPos;
        }
    }
}
