using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using WoWFormatLib.FileReaders;
using Generators.ADT_Alpha;

namespace ADTread
{
    public partial class Form1 : Form
    {
        //-----------------------------------------------------------------------------------------------------------------
        //PUBLIC STUFF:
        //-----------------------------------------------------------------------------------------------------------------

        public List<Bitmap> AlphaLayers = new List<Bitmap>();
        public List<String> AlphaLayersNames = new List<String>();

        //-----------------------------------------------------------------------------------------------------------------
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //textBox1.Text = "D:\\mpqediten32\\7zipan_424\\Work\\World\\maps\\Azeroth\\Azeroth_30_49.adt";
            textBox1.Text = "D:\\mpqediten32\\Work\\World\\maps\\COTWarOfTheAncients\\cotwaroftheancients_40_23.adt";
            label1.Text = "rootADT";
            label2.Text = "WDTFile";
            label3.Text = "objADT";
            label4.Text = "texADT";
            groupBox1.Text = "Alphamap [#]";
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            button1.Text = "Load";
            this.Text = "ADT Alpha Read";
            button2.Enabled = false;
            button2.Text = "Export";
            textBox2.Text = "D:\\export";
            checkBox1.Text = "Unified export";
            //checkBox1.Checked = true;
            button3.Text = "Preview";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------------------------------------------------------------------------
            //Get all files from the getgo
            //-----------------------------------------------------------------------------------------------------------------
            var ADTfile = textBox1.Text;
            var WDTfile = textBox1.Text.Substring(0, textBox1.Text.Length - 10) + ".wdt";
            var ADTobj = textBox1.Text.Replace(".adt", "_obj0.adt");
            var ADTtex = textBox1.Text.Replace(".adt", "_tex0.adt");
            //-----------------------------------------------------------------------------------------------------------------

            //-----------------------------------------------------------------------------------------------------------------
            //Check if files exist and update names in the interface:
            //-----------------------------------------------------------------------------------------------------------------
            label1.Text = ADTfile;
            if (File.Exists(ADTfile))
            {
                label1.BackColor = Color.LightGreen;
            }
            else
            {
                label1.BackColor = Color.Pink;
            }
            label2.Text = WDTfile;
            if (File.Exists(WDTfile))
            {
                label2.BackColor = Color.LightGreen;
            }
            else
            {
                label2.BackColor = Color.Pink;
            }
            label3.Text = ADTobj;
            if (File.Exists(ADTobj))
            {
                label3.BackColor = Color.LightGreen;
            }
            else
            {
                label3.BackColor = Color.Pink;
            }
            label4.Text = ADTtex;
            if (File.Exists(ADTtex))
            {
                label4.BackColor = Color.LightGreen;
            }
            else
            {
                label4.BackColor = Color.Pink;
            }

            //Clear listbox first
            listBox1.Items.Clear();
            //Clear arrays
            AlphaLayers.Clear();
            AlphaLayersNames.Clear();
            //Clear the picturebox
            pictureBox1.Image = null;
            //Reset groupbox name
            groupBox1.Text = "Alphamap [#]";
            //-----------------------------------------------------------------------------------------------------------------

            //-----------------------------------------------------------------------------------------------------------------
            //File operations:
            //-----------------------------------------------------------------------------------------------------------------
            if (File.Exists(ADTfile) && File.Exists(WDTfile) && File.Exists(ADTobj) && File.Exists(ADTtex))
            {
                //Read the ADT file:
                ADTReader reader = new ADTReader();
                reader.LoadADT(ADTfile, WDTfile, ADTobj, ADTtex);
                
                //Add in the listbox all the textures (+path) used by the adt file:
                listBox1.Items.AddRange(reader.adtfile.textures.filenames);

                //Generate the alphamaps:
                ADT_Alpha AlphaMapsGenerator = new ADT_Alpha();
                //AlphaMapsGenerator.GenerateAlphaMaps(reader.adtfile);
                AlphaMapsGenerator.GenerateAlphaMaps(reader.adtfile, checkBox1.Checked);

                //Assign layers and names
                AlphaLayers = AlphaMapsGenerator.AlphaLayers;
                AlphaLayersNames = AlphaMapsGenerator.AlphaLayersNames;

                //Enable the export button if the generation was successful
                if (AlphaLayers != null)
                {
                    button2.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("One or more files are missing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button2.Enabled = false;
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (checkBox1.Checked == true)
            //{ 
            //Update selected layer:
            pictureBox1.Image = AlphaLayers[listBox1.SelectedIndex];
            groupBox1.Text = "Alphamap [" + listBox1.SelectedIndex.ToString() + "]";
            //}
            //else
            //{
            //    groupBox1.Text = "Alphamap [PREVIEW DISABLED IN THIS MODE]";
            //}

        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            //Get the mapname
            string mapname = textBox1.Text;
            mapname = mapname.Substring(mapname.LastIndexOf("\\", mapname.Length - 2) + 1);
            mapname = mapname.Substring(0,mapname.Length-4);

            for (int m = 0; m < AlphaLayers.ToArray().Length; m++)
                {
                try
                {
                    if (checkBox1.Checked == true)
                    { 
                    AlphaLayers[m].Save(textBox2.Text + "\\" + mapname + "_alpha_" + AlphaLayersNames[m] + ".png");
                    }
                    else
                    {
                    AlphaLayers[m].Save(textBox2.Text + "\\" + mapname +"_"+ AlphaLayersNames[m] + ".png");
                    }
                }
                catch
                {
                    MessageBox.Show("Could not export the alpha maps", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(AlphaLayersNames.ToArray());

        }
    }
}

