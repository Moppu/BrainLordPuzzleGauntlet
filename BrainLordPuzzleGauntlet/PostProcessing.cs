using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet
{
    /// <summary>
    /// utilities for post-processing maps
    /// </summary>
    public class PostProcessing
    {
        private static bool filterObject(Map map, EntryPos entryPoint, byte objectX, byte objectY)
        {
            // determine whether object is in the same "room" as the EntryPoint
            int[] distanceFromEntry = new int[64 * 64];
            List<int> unprocessedPositions = new List<int>();

            // initial pos
            unprocessedPositions.Add(entryPoint.y * 64 + entryPoint.x);
            distanceFromEntry[entryPoint.y * 64 + entryPoint.x] = 1;

            int objX = objectX / 4;
            int objY = objectY / 2;
            int objLoc = objY * 64 + objX;
            // fill in locations we can reach from the entry point
            while (unprocessedPositions.Count > 0)
            {
                int checkPosition = unprocessedPositions[0];
                unprocessedPositions.RemoveAt(0);
                int dist = distanceFromEntry[checkPosition];
                if (checkPosition >= 64)
                {
                    int upLoc = checkPosition - 64;
                    if(processPosition(map, upLoc, distanceFromEntry))
                    {
                        if (upLoc == objLoc)
                        {
                            return true;
                        }
                        distanceFromEntry[upLoc] = dist + 1;
                        unprocessedPositions.Add(upLoc);
                    }
                }
                if (checkPosition < 64 * 64 - 64)
                {
                    int downLoc = checkPosition + 64;
                    if (processPosition(map, downLoc, distanceFromEntry))
                    {
                        if (downLoc == objLoc)
                        {
                            return true;
                        }
                        distanceFromEntry[downLoc] = dist + 1;
                        unprocessedPositions.Add(downLoc);
                    }
                }
                if ((checkPosition % 64) != 0)
                {
                    int leftLoc = checkPosition - 1;
                    if (processPosition(map, leftLoc, distanceFromEntry))
                    {
                        if (leftLoc == objLoc)
                        {
                            return true;
                        }
                        distanceFromEntry[leftLoc] = dist + 1;
                        unprocessedPositions.Add(leftLoc);
                    }
                }
                if ((checkPosition % 64) != 63)
                {
                    int rightLoc = checkPosition + 1;
                    if (processPosition(map, rightLoc, distanceFromEntry))
                    {
                        if (rightLoc == objLoc)
                        {
                            return true;
                        }
                        distanceFromEntry[rightLoc] = dist + 1;
                        unprocessedPositions.Add(rightLoc);
                    }
                }
            }
            return false;
        }

        private static bool processPosition(Map map, int position, int[] distanceFromEntry)
        {
            if (distanceFromEntry[position] == 0)
            {
                bool isWall = false;
                foreach (string tileName in map.tileset.tileValues.Keys)
                {
                    if (tileName.Contains("OuterWall") || tileName == "DoorSocket")
                    {
                        if (map.data[position] == map.tileset.tileValues[tileName])
                        {
                            isWall = true;
                            break;
                        }
                    }
                }
                return !isWall;
            }
            return false;
        }

        public static void makeObjectList(Map map, byte[] rom, ref int workingOffset, EntryPos entryPoint)
        {
            foreach (int chestId in map.chests.Keys)
            {
                if (PostProcessing.filterObject(map, entryPoint, map.chests[chestId].x, map.chests[chestId].y))
                {
                    rom[workingOffset++] = map.chests[chestId].x;
                    rom[workingOffset++] = map.chests[chestId].y;
                    rom[workingOffset++] = 0x01;
                    rom[workingOffset++] = (byte)chestId;
                    rom[workingOffset++] = 0x00;
                    rom[workingOffset++] = 0x00;
                    rom[workingOffset++] = 0x00;
                }
            }

            foreach (MapObject obj in map.otherObjects)
            {
                if (PostProcessing.filterObject(map, entryPoint, obj.x, obj.y))
                {
                    rom[workingOffset++] = obj.x;
                    rom[workingOffset++] = obj.y;
                    rom[workingOffset++] = obj.type;
                    rom[workingOffset++] = obj.p1;
                    rom[workingOffset++] = obj.p2;
                    rom[workingOffset++] = obj.p3;
                    rom[workingOffset++] = obj.p4;
                }
            }

            // this object makes the dark water animate
            // 36/00 makes the ice castle clouds come on
            // 32/00 is phantom floor
            if (map.tileset.name == "Platinum")
            {
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x34; // 34
                rom[workingOffset++] = 0x02; // 02
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
            }
            else if (map.booleanPropertyEnabled("ice castle clouds", false))
            {
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x36;
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
            }

            if (map.tileset.name == "Tower")
            {
                // this makes the background swirly
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x32; // 0x32
                rom[workingOffset++] = 0x00; // 00
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
                rom[workingOffset++] = 0x00;
            }

            // don't filter this list, these things are invisible anyway
            foreach (int switchDoorNum in map.doorSwitches.Keys)
            {
                foreach (MapObject obj in map.doorSwitches[switchDoorNum])
                {
                    rom[workingOffset++] = obj.x;
                    rom[workingOffset++] = obj.y;
                    rom[workingOffset++] = obj.type;
                    rom[workingOffset++] = obj.p1;
                    rom[workingOffset++] = obj.p2;
                    rom[workingOffset++] = obj.p3;
                    rom[workingOffset++] = obj.p4;
                }
            }

            // object block end
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0xFF;
            rom[workingOffset++] = 0x00;
            rom[workingOffset++] = 0x00;
            rom[workingOffset++] = 0x00;

        }
    }
}
