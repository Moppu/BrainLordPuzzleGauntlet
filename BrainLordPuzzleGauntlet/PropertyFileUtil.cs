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
    /// Parser & writer for .properties/.ini files
    /// </summary>
    public class PropertyFileUtil
    {

        public static Dictionary<string, string> readByteArray(byte[] data)
        {
            try
            {
                StreamReader sr = new StreamReader(new MemoryStream(data), Encoding.Default);
                return readPropertyStream(sr);
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }

        public static Dictionary<string, string> readFile(string filename)
        {
            StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open), Encoding.Default);
            return readPropertyStream(sr);
        }
        
        private static Dictionary<string, string> readPropertyStream(StreamReader sr)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string line = sr.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                if (!line.StartsWith("#"))
                {
                    int equals = line.IndexOf('=');
                    if (equals > 0)
                    {
                        string before = line.Substring(0, equals).Trim();
                        string after = line.Substring(equals + 1).Trim();
                        if (before.Length > 0 && after.Length > 0)
                        {
                            ret[before] = after;
                        }
                    }
                }
                line = sr.ReadLine();
            }
            sr.Close();
            return ret;
        }

        public static void writePropertyFile(string filename, Dictionary<string, string> data)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            foreach (string key in data.Keys)
            {
                sw.WriteLine(key + "=" + data[key]);
            }
            sw.Close();
        }

        public static string stripComments(string line)
        {
            if(line.Contains("#"))
            {
                return line.Substring(0, line.IndexOf("#"));
            }
            return line;
        }
    }

}
