using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainLordPuzzleGauntlet.structures
{
    /// <summary>
    /// Info associated with one whole generated brainlord map
    /// </summary>
    public class Map
    {
        // filename of map minus the path
        public string mapName;
        // tile data
        public byte[] data;
        // where to place the loaded data; x
        public int mapPlacementOffsetX = 6;
        // where to place the loaded data; y
        public int mapPlacementOffsetY = 2;
        // tileset
        public Tileset tileset;
        // door id -> list of switches that are involved in opening it
        public Dictionary<int, List<MapObject>> doorSwitches = new Dictionary<int, List<MapObject>>();
        // non-switch objects, including doors themselves, balls, and anything else
        public List<MapObject> otherObjects = new List<MapObject>();
        // door id -> x/y source position of door
        public Dictionary<int, MapDoor> doors = new Dictionary<int, MapDoor>();
        // messagebox id -> x/y position of box
        public Dictionary<int, List<MapMessageBox>> messageBoxes = new Dictionary<int, List<MapMessageBox>>();
        // entry point into map id -> x/y of position
        public Dictionary<int, EntryPos> entries = new Dictionary<int, EntryPos>();
        // messagebox id -> string message to display
        public Dictionary<int, string> messages = new Dictionary<int, string>();
        // name of song to use for this map
        public string song = "Platinum";
        // if we use a numeric combo to open the door, this is it, in switch ids
        public List<int> switchCombo = new List<int>();
        // source door id/destination entry id -> destination map to warp to
        public Dictionary<DoorTransition, Map> connections = new Dictionary<DoorTransition, Map>();
        // ice object id -> obj data
        public Dictionary<int, MapObject> fallingIce = new Dictionary<int, MapObject>();
        // ice object id -> switch used to spawn it
        public Dictionary<int, MapObject> iceSwitches = new Dictionary<int, MapObject>();
        // warnings encountered while loading our map
        public List<string> loadWarnings = new List<string>();
        // map of save num to save loading entry position
        public Dictionary<int, SaveLoadSpot> saveLoadSpots = new Dictionary<int, SaveLoadSpot>();
        // map of chest num to chest pos + item
        public Dictionary<int, Chest> chests = new Dictionary<int, Chest>();
        // other properties
        public Dictionary<string, string> properties = new Dictionary<string, string>();
        // enemies
        public List<Enemy> enemies = new List<Enemy>();
        public bool booleanPropertyEnabled(string propertyName, bool defaultValue)
        {
            if(properties.ContainsKey(propertyName))
            {
                string propVal = properties[propertyName].ToLower();
                return propVal == "yes" || propVal == "1" || propVal == "true";
            }
            return defaultValue;
        }

        public int intPropertyValue(string propertyName, int defaultValue)
        {
            if (properties.ContainsKey(propertyName))
            {
                int parsed;
                if(Int32.TryParse(properties[propertyName], out parsed))
                {
                    return parsed;
                }
                return defaultValue;
            }
            return defaultValue;
        }
    }
}
