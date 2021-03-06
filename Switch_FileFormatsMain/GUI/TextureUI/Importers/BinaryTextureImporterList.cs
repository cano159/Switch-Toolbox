﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;
using Switch_Toolbox.Library;

namespace FirstPlugin
{
    public partial class BinaryTextureImporterList : Form
    {
        public bool ForceMipCount = false;

        public uint SelectedMipCount
        {
            set
            {
                if (MipmapNum.Maximum <= value)
                    MipmapNum.Value = value;
            }
            get
            {
                return (uint)MipmapNum.Value;
            }
        }

        public BinaryTextureImporterList()
        {
            InitializeComponent();
            listViewCustom1.FullRowSelect = true;
            listViewCustom1.CanResizeList = true;

            //Add enums
            foreach (SurfaceFormat format in (SurfaceFormat[])Enum.GetValues(typeof(SurfaceFormat)))
            {
                //  if (format != SurfaceFormat.Invalid)
            }

            formatComboBox.Items.Add(SurfaceFormat.A1_B5_G5_R5_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.A4_B4_G4_R4_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.B5_G5_R5_A1_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.B5_G6_R5_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.B8_G8_R8_A8_SRGB);
            formatComboBox.Items.Add(SurfaceFormat.B8_G8_R8_A8_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R10_G10_B10_A2_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R16_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R4_G4_B4_A4_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R4_G4_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R5_G5_B5_A1_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R5_G6_B5_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R8_G8_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R8_UNORM);

            formatComboBox.Items.Add(SurfaceFormat.R8_G8_B8_A8_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.R8_G8_B8_A8_SRGB);
            formatComboBox.Items.Add(SurfaceFormat.BC1_SRGB);
            formatComboBox.Items.Add(SurfaceFormat.BC1_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.BC2_SRGB);
            formatComboBox.Items.Add(SurfaceFormat.BC2_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.BC3_SRGB);
            formatComboBox.Items.Add(SurfaceFormat.BC3_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.BC4_SNORM);
            formatComboBox.Items.Add(SurfaceFormat.BC4_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.BC5_SNORM);
            formatComboBox.Items.Add(SurfaceFormat.BC5_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.BC6_UFLOAT);
            formatComboBox.Items.Add(SurfaceFormat.BC6_FLOAT);
            formatComboBox.Items.Add(SurfaceFormat.BC7_UNORM);
            formatComboBox.Items.Add(SurfaceFormat.BC7_SRGB);

            foreach (SurfaceDim dim in (SurfaceDim[])Enum.GetValues(typeof(SurfaceDim)))
            {
                ImgDimComb.Items.Add(dim);
            }
            tileModeCB.Items.Add("Texture");
            ImgDimComb.SelectedIndex = 1;
            tileModeCB.SelectedIndex = 0;
            formatComboBox.SelectedItem = SurfaceFormat.BC1_SRGB;
        }
        TextureImporterSettings SelectedTexSettings;

        public List<TextureImporterSettings> settings = new List<TextureImporterSettings>();
        public void LoadSetting(TextureImporterSettings setting)
        {
            settings.Add(setting);

            listViewCustom1.Items.Add(setting.TexName).SubItems.Add(setting.Format.ToString());
            listViewCustom1.Items[0].Selected = true;
            listViewCustom1.Select();
        }
        public void LoadSettings(List<TextureImporterSettings> s)
        {
            settings = s;

            foreach (var setting in settings)
            {
                listViewCustom1.Items.Add(setting.TexName).SubItems.Add(setting.Format.ToString());
            }
            listViewCustom1.Items[0].Selected = true;
            listViewCustom1.Select();
        }

        private Thread Thread;
        public void SetupSettings()
        {
            if (SelectedTexSettings.Format == SurfaceFormat.Invalid)
                return;


            WidthLabel.Text = $"Width {SelectedTexSettings.TexWidth}";
            HeightLabel.Text = $"Height {SelectedTexSettings.TexHeight}";


            if (formatComboBox.SelectedItem is SurfaceFormat)
            {
                SelectedTexSettings.Format = (SurfaceFormat)formatComboBox.SelectedItem;
                listViewCustom1.SelectedItems[0].SubItems[1].Text = SelectedTexSettings.Format.ToString();
            }
            Bitmap bitmap = Switch_Toolbox.Library.Imaging.GetLoadingImage();


            Thread = new Thread((ThreadStart)(() =>
            {
                ToggleOkButton(false);

                pictureBox1.Image = bitmap;
                SelectedTexSettings.Compress();

                ToggleOkButton(true);

                if (SelectedTexSettings.Format == SurfaceFormat.BC5_SNORM)
                {
                    bitmap = DDSCompressor.DecompressBC5(SelectedTexSettings.DataBlockOutput[0],
                (int)SelectedTexSettings.TexWidth, (int)SelectedTexSettings.TexHeight, true);
                }
                else
                {
                    bitmap = TextureData.DecodeBlockGetBitmap(SelectedTexSettings.DataBlockOutput[0],
                    SelectedTexSettings.TexWidth, SelectedTexSettings.TexHeight, TextureData.ConvertFormat(SelectedTexSettings.Format));
                }

                if (pictureBox1.InvokeRequired)
                {
                    pictureBox1.Invoke((MethodInvoker)delegate {
                        pictureBox1.Image = bitmap;
                        pictureBox1.Refresh();
                    });
                }
            }));
            Thread.Start();

        }

        private void ToggleOkButton(bool Enable)
        {
            if (button1.InvokeRequired)
            {
                button1.Invoke((MethodInvoker)delegate {
                    button1.Enabled = Enable;
                });
            }
            else
                button1.Enabled = Enable;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void formatComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (formatComboBox.SelectedIndex > -1 && SelectedTexSettings != null)
            {
                SetupSettings();
            }
        }

        private void listViewCustom1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCustom1.SelectedItems.Count > 0)
            {
                SelectedTexSettings = settings[listViewCustom1.SelectedIndices[0]];
                formatComboBox.SelectedItem = SelectedTexSettings.Format;

                SetupSettings();

                if (ForceMipCount)
                    MipmapNum.Maximum = SelectedTexSettings.MipCount;
                else
                    MipmapNum.Maximum = SelectedTexSettings.GetTotalMipCount() + 1;

                MipmapNum.Value = SelectedTexSettings.MipCount;
            }
        }

        private void BinaryTextureImporterList_Load(object sender, EventArgs e)
        {
        }

        private void MipmapNum_ValueChanged(object sender, EventArgs e)
        {
            SelectedTexSettings.MipCount = (uint)MipmapNum.Value;
        }
    }
}
