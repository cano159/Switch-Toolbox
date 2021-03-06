﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Switch_Toolbox.Library.Forms;
using Switch_Toolbox.Library;

namespace FirstPlugin
{
    public partial class EmitterEditor : STUserControl
    {
        public EmitterEditor()
        {
            InitializeComponent();
            stTabControl1.myBackColor = FormThemes.BaseTheme.FormBackColor;
            tabPageColors.BackColor = FormThemes.BaseTheme.TabPageActive;
        }

        private void Reset()
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            TBTexture0.Text = "";
            TBTexture1.Text = "";
            TBTexture2.Text = "";
        }

        private Thread Thread;
        PTCL.Emitter ActiveEmitter;

        public void LoadEmitter(PTCL.Emitter Emitter)
        {
            ActiveEmitter = Emitter;

            Reset();


            for (int i = 0; i < 8; i++)
            {
            }

            color0Index0.BackColor = Emitter.Color0s[0];
            color0Index1.BackColor = Emitter.Color0s[1];
            color0Index2.BackColor = Emitter.Color0s[2];
            color0Index3.BackColor = Emitter.Color0s[3];
            color0Index4.BackColor = Emitter.Color0s[4];
            color0Index5.BackColor = Emitter.Color0s[5];
            color0Index6.BackColor = Emitter.Color0s[6];
            color0Index7.BackColor = Emitter.Color0s[7];


            color1Index0.BackColor = Emitter.Color1s[0];
            color1Index1.BackColor = Emitter.Color1s[1];
            color1Index2.BackColor = Emitter.Color1s[2];
            color1Index3.BackColor = Emitter.Color1s[3];
            color1Index4.BackColor = Emitter.Color1s[4];
            color1Index5.BackColor = Emitter.Color1s[5];
            color1Index6.BackColor = Emitter.Color1s[6];
            color1Index7.BackColor = Emitter.Color1s[7];

            color0TB.Text = Utils.ColorToHex(Emitter.Color0s[0]);
            color0TB2.Text = Utils.ColorToHex(Emitter.Color0s[1]);
            color0TB3.Text = Utils.ColorToHex(Emitter.Color0s[2]);
            color0TB4.Text = Utils.ColorToHex(Emitter.Color0s[3]);
            color0TB5.Text = Utils.ColorToHex(Emitter.Color0s[4]);
            color0TB6.Text = Utils.ColorToHex(Emitter.Color0s[5]);
            color0TB7.Text = Utils.ColorToHex(Emitter.Color0s[6]);
            color0TB8.Text = Utils.ColorToHex(Emitter.Color0s[7]);
            color1TB.Text = Utils.ColorToHex(Emitter.Color1s[0]);
            color1TB2.Text = Utils.ColorToHex(Emitter.Color1s[1]);
            color1TB3.Text = Utils.ColorToHex(Emitter.Color1s[2]);
            color1TB4.Text = Utils.ColorToHex(Emitter.Color1s[3]);
            color1TB5.Text = Utils.ColorToHex(Emitter.Color1s[4]);
            color1TB6.Text = Utils.ColorToHex(Emitter.Color1s[5]);
            color1TB7.Text = Utils.ColorToHex(Emitter.Color1s[6]);
            color1TB8.Text = Utils.ColorToHex(Emitter.Color1s[7]);


            if (Emitter.DrawableTex.Count <= 0)
                return;

            if (Emitter.DrawableTex[0].MipCount > 0)
            {
                var contextMenuStrip1 = new ContextMenuStrip();
                contextMenuStrip1.Items.Add("Replace", null, ReplaceImage0);
                contextMenuStrip1.Items.Add("Export", null, ExportImage0);

                pictureBox1.Image = Imaging.GetLoadingImage();
                pictureBox1.Image = Emitter.DrawableTex[0].GetBitmap();
                pictureBox1.ContextMenuStrip = contextMenuStrip1;

                TBTexture0.Text = Emitter.DrawableTex[0].Text;
            }
            if (Emitter.DrawableTex.Count < 2)
                return;

            if (Emitter.DrawableTex[1].MipCount > 0)
            {
                pictureBox2.Image = Imaging.GetLoadingImage();
                pictureBox2.Image = Emitter.DrawableTex[1].GetBitmap();
                TBTexture1.Text = Emitter.DrawableTex[1].Text;
            }
            if (Emitter.DrawableTex.Count < 3)
                return;

            if (Emitter.DrawableTex[2].MipCount > 0)
            {
                pictureBox3.Image = Imaging.GetLoadingImage();
                pictureBox3.Image = Emitter.DrawableTex[2].GetBitmap();
                TBTexture2.Text = Emitter.DrawableTex[2].Text;
            }


            Thread = new Thread((ThreadStart)(() =>
            {
          
            }));

            try
            {

           /*     Thread Thread2 = new Thread((ThreadStart)(() =>
                {
                    pictureBox2.Image = Imaging.GetLoadingImage();
                    pictureBox2.Image = Emitter.DrawableTex[1].GetBitmap();
                }));
                Thread Thread3 = new Thread((ThreadStart)(() =>
                {
                    pictureBox3.Image = Imaging.GetLoadingImage();
                    pictureBox3.Image = Emitter.DrawableTex[2].GetBitmap();
                }));*/

            }
            catch
            {

            }
        }
        private void ExportImage0(object sender, EventArgs e)
        {
            ActiveEmitter.DrawableTex[0].ExportImage();
        }
        private void ReplaceImage0(object sender, EventArgs e)
        {
            if (ActiveEmitter is PTCL.WiiU.EmitterU)
            {
                var emitter = (PTCL.WiiU.EmitterU)ActiveEmitter;

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Supported Formats|*.dds; *.png;*.tga;*.jpg;*.tiff|" +
                             "Microsoft DDS |*.dds|" +
                             "Portable Network Graphics |*.png|" +
                             "Joint Photographic Experts Group |*.jpg|" +
                             "Bitmap Image |*.bmp|" +
                             "Tagged Image File Format |*.tiff|" +
                             "All files(*.*)|*.*";

                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ((PTCL.WiiU.TextureInfo)emitter.DrawableTex[0]).Replace(ofd.FileName);
                }
            }
        }

        private void color_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                char LastChar = button.Name[button.Name.Length - 1];

                Console.WriteLine(button.Name + " LastChar " + LastChar + " " + ActiveEmitter.Color0Array.Length);
                int index = int.Parse(LastChar.ToString());

                ColorDialog dialog = new ColorDialog();


                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (button.Name.Contains("color0"))
                    {
                        ActiveEmitter.Color0Array[index].R = (float)dialog.Color.R / 255;
                        ActiveEmitter.Color0Array[index].G = (float)dialog.Color.G / 255;
                        ActiveEmitter.Color0Array[index].B = (float)dialog.Color.B / 255;
                        ActiveEmitter.Color0Array[index].A = (float)dialog.Color.A / 255;
                        ActiveEmitter.Color0s[index] = dialog.Color;
                    }
                    else
                    {
                        ActiveEmitter.Color1Array[index].R = (float)dialog.Color.R / 255;
                        ActiveEmitter.Color1Array[index].G = (float)dialog.Color.G / 255;
                        ActiveEmitter.Color1Array[index].B = (float)dialog.Color.B / 255;
                        ActiveEmitter.Color1Array[index].A = (float)dialog.Color.A / 255;
                        ActiveEmitter.Color1s[index] = dialog.Color;
                    }

                    button.BackColor = dialog.Color;
                }
            }
        }
    }
}
