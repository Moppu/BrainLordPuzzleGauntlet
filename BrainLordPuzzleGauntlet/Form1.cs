using BrainLordPuzzleGauntlet.structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrainLordPuzzleGauntlet
{
    /// <summary>
    /// As in every single C# project I've ever made, the main form is called Form1.
    /// </summary>
    public partial class Form1 : Form
    {
        ToolTip t = new ToolTip();
        string VERSION_NUMBER = "0.2";
        public Form1()
        {
            InitializeComponent();

            t.AutoPopDelay = 32767;

            string buildDateIndicator = "Build date: ";
            try
            {
                string assemblyVersion = "" + typeof(Form1).Assembly.GetName().Version;
                string[] versionTokens = assemblyVersion.Split(new char[] { '.' });
                string days = versionTokens[2];
                string minutes = versionTokens[3];
                DateTime date = new DateTime(2000, 1, 1)     // baseline is 01/01/2000
                .AddDays(Int32.Parse(days))             // build is number of days after baseline
                .AddSeconds(Int32.Parse(minutes) * 2);    // revision is half the number of seconds into the day
                buildDateIndicator += "" + date;
            }
            catch (Exception e)
            {
                // shrug i guess
                buildDateIndicator += "???";
            }

            // process config file if it exists
            t.SetToolTip(pictureBox1,
                "Version " + VERSION_NUMBER +
                "\n" + buildDateIndicator +
                "\nBy Mop!" +
                "\numokumok@gmail.com" +
                "\ntwitch.tv/moppleton");

            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string specificFolder = Path.Combine(appDataFolder, "BLPG");
            string configFile = Path.Combine(specificFolder, "config.ini");
            try
            {
                Dictionary<string, string> configProperties = PropertyFileUtil.readFile(configFile);
                if(configProperties.ContainsKey("inRom"))
                {
                    textBox1.Text = configProperties["inRom"];
                }
                if (configProperties.ContainsKey("outRom"))
                {
                    textBox2.Text = configProperties["outRom"];
                }
                if (configProperties.ContainsKey("puzzleFile"))
                {
                    textBox4.Text = configProperties["puzzleFile"];
                }
            }
            catch (Exception e)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox3.Clear();
            string inPath = textBox1.Text;
            string outPath = textBox2.Text;
            if(inPath.EndsWith(".zip"))
            {
                textBox3.Text += "Doesn't support zip files.  Unzip your ROM!";
                return;
            }
            if(inPath == outPath)
            {
                textBox3.Text += "Trying to overwrite your vanilla ROM.  Choose a different output!";
                return;
            }

            byte[] origRom = null;
            try
            {
                origRom = File.ReadAllBytes(inPath);
            }
            catch(Exception ee)
            {
                textBox3.Text += "Unable to read input ROM: " + ee.Message;
                return;
            }

            if (origRom.Length >= 2 && origRom[0] == 'P' && origRom[1] == 'K')
            {
                textBox3.Text += "Zipped ROMs are not supported.";
                return;
            }

            // support header that's on some roms, by ignoring it
            int headerSize = 0;
            if (origRom.Length == 0x180200)
            {
                headerSize = 0x200;
            }
            else if(origRom.Length != 0x180000)
            {
                textBox3.Text += "File size doesn't match expected.  Please use 12 Megabit (1.5 megabyte) US Brainlord ROM.";
                return;
            }

            // for now allocate 32 megabit and just assume we won't exceed it
            byte[] newRom = new byte[0x400000];
            for (int i = 0; i < 0x180000; i++)
            {
                // copy original rom into new space
                newRom[i] = origRom[i + headerSize];
            }

            // from https://sneslab.net/wiki/SNES_ROM_Header
            // check language and checksum
            // brainlord appears to only have version 0, so i'm not checking it here
            if (newRom[0xFFD9] != 1)
            {
                textBox3.Text += "Please only use the US version of the game.";
                return;
            }

            if (newRom[0xFFDE] != 0x6A || newRom[0xFFDF] != 0xA1)
            {
                textBox3.Text += "Warning: unexpected checksum. ROM may not work!";
            }

            textBox3.Text += "Generating..." + Environment.NewLine;
            try
            {
                // call out to PuzzleMaker to actually make the rom
                Dictionary<string, Map> parsedMaps = PuzzleMaker.process(textBox4.Text, newRom);
                // check for warnings and show them
                foreach (string mapName in parsedMaps.Keys)
                {
                    Map parsedMap = parsedMaps[mapName];
                    if (parsedMap.loadWarnings.Count > 0)
                    {
                        textBox3.Text += "Warnings for " + mapName + ":" + Environment.NewLine;
                        foreach (string loadWarning in parsedMap.loadWarnings)
                        {
                            textBox3.Text += "  " + loadWarning + Environment.NewLine;
                        }
                    }
                }
                textBox3.Text += "Success!";
            }
            catch(MapLoadException ee)
            {
                // an error i'm actually checking for
                textBox3.Text += "Failed with error for " + ee.map.mapName + ":" + Environment.NewLine;
                textBox3.Text += ee.Message;
                return;
            }
            catch (Exception ee)
            {
                // runtime exception, probably something i'm doing wrong or not checking properly
                textBox3.Text += "Failed with generic exception! Reason:" + Environment.NewLine;
                textBox3.Text += ee.Message + Environment.NewLine;
                textBox3.Text += ee.StackTrace + Environment.NewLine;
                if (ee.InnerException != null)
                {
                    textBox3.Text += ee.InnerException.Message + Environment.NewLine;
                    textBox3.Text += ee.InnerException.StackTrace + Environment.NewLine;
                }
                textBox3.Text += "If you're seeing this, it's probably a condition I should be checking." + Environment.NewLine;
                return;
            }

            // write generated rom
            try
            {
                File.WriteAllBytes(outPath, newRom);
            }
            catch(Exception ee)
            {
                textBox3.Text += "Failed to write file! Reason:" + Environment.NewLine;
                textBox3.Text += ee.Message;
                return;
            }

            // write config after successful ROM generation
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string specificFolder = Path.Combine(appDataFolder, "BLPG");
            string configFile = Path.Combine(specificFolder, "config.ini");
            try
            {
                Directory.CreateDirectory(specificFolder);
            }
            catch(Exception ee)
            {
                textBox3.Text += "ROM generated, but failed to write config file. Reason:" + Environment.NewLine;
                textBox3.Text += ee.Message;
                return;
            }
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties["inRom"] = textBox1.Text;
            properties["outRom"] = textBox2.Text;
            properties["puzzleFile"] = textBox4.Text;
            try
            {
                PropertyFileUtil.writePropertyFile(configFile, properties);
            }
            catch(Exception ee)
            {
                textBox3.Text += "ROM generated, but failed to write config file. Reason:" + Environment.NewLine;
                textBox3.Text += ee.Message;
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // open vanilla rom callback
            string ownPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            string path = openFileDialog(ownPath + "\\Roms");
            if (path != null)
            {
                textBox1.Text = path;
            }
        }

        private string openFileDialog(string initialPath)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.InitialDirectory = initialPath;
            of.Multiselect = false;
            DialogResult dr = of.ShowDialog();
            if (dr == DialogResult.OK)
            {
                return of.FileName;
            }
            return null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // open target rom callback
            string ownPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            SaveFileDialog sf = new SaveFileDialog();
            sf.InitialDirectory = ownPath + "\\Roms";
            sf.Filter = "Rom File (*.smc)|*.smc";
            DialogResult d = sf.ShowDialog();
            if (d == DialogResult.OK)
            {
                textBox2.Text = sf.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // open puzzle settings callback
            string ownPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            string path = openFileDialog(ownPath + "\\Puzzles");
            if (path != null)
            {
                textBox4.Text = path;
            }
        }
    }
}
