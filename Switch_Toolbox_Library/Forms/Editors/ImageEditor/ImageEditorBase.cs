﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Activities.Statements;
using System.Threading;
using System.Windows.Forms;

namespace Switch_Toolbox.Library.Forms
{
    public partial class ImageEditorBase : UserControl
    {
        private Thread Thread;

        int CurMipDisplayLevel = 0;
        int CurArrayDisplayLevel = 0;
        uint TotalMipCount = 1;
        uint TotalArrayCount = 1;
        public STGenericTexture ActiveTexture;

        public bool PropertyShowTop = false;

        //Instead of disabling/enabling channels, this will only show the selected channel
        public bool UseChannelToggle = false; 

        private bool hasBeenEdited = false;
        public bool HasBeenEdited
        {
            set
            {
                hasBeenEdited = value;

                if (ActiveTexture != null)
                    ActiveTexture.IsEdited = value;

                saveBtn.Enabled = hasBeenEdited;

                if (saveBtn.Enabled == false)
                    saveBtn.BackgroundImage = GetGrayscale(Properties.Resources.Save);
                else
                    saveBtn.BackgroundImage = Properties.Resources.Save;

                saveBtn.NotifyDefault(false);
            }
            get
            {
                return hasBeenEdited;
            }
        }

        public bool HasRedChannel = true;
        public bool HasBlueChannel = true;
        public bool HasGreenChannel = true;
        public bool HasAlphaChannel = true;

        ImagePropertiesEditor propertiesEditor;

        public bool ShowChannelEditor = true;

        public void AddFileContextEvent(string Name, EventHandler eventHandler, bool Checked = false)
        {
            if (Name == "Save")
            {
                fileToolStripMenuItem.DropDownItems.Insert(0, new STToolStripItem(Name, eventHandler));
            }
            else
            {
                fileToolStripMenuItem.DropDownItems.Add(new STToolStripItem(Name, eventHandler) { Checked = Checked });
            }
        }

        public Image GetGrayscale(Image img)
        {
            // also fix the problem of indexed pixel format (if any)
            Bitmap imge = new Bitmap(img);

            //create graphics
            Graphics g = Graphics.FromImage(imge);
            //draw disabled image
            System.Windows.Forms.ControlPaint.DrawImageDisabled(g, img, 0, 0, Color.Transparent);

            //retrun result
            return imge;
        }

        public UserControl GetActiveTabEditor(Type type)
        {
            return propertiesEditor.GetActiveTabControl(type);
        }
        public void AddCustomControl(UserControl control, Type type)
        {
            propertiesEditor.AddTabPage(control, type);
        }
        public ImageEditorBase()
        {
            InitializeComponent();

            saveBtn.Enabled = HasBeenEdited;

            imageToolStripMenuItem.Enabled = false;
            adjustmentsToolStripMenuItem.Enabled = false;

            foreach (var type in Enum.GetValues(typeof(Runtime.PictureBoxBG)).Cast<Runtime.PictureBoxBG>())
                imageBGComboBox.Items.Add(type);

            imageBGComboBox.SelectedItem = Runtime.pictureBoxStyle;
            UpdateBackgroundImage();

            propertyGridToolStripMenuItem.Checked = Runtime.ImageEditor.ShowPropertiesPanel;

            propertiesEditor = new ImagePropertiesEditor();
            propertiesEditor.Dock = DockStyle.Fill;

            if (!propertyGridToolStripMenuItem.Checked)
            {
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.Panel2.Hide();
            }
            else
            {
                splitContainer1.Panel2Collapsed = false;
                splitContainer1.Panel2.Show();
            }

            if (ShowChannelEditor)
            {
                LoadChannelEditor(null);
            }

            if (PropertyShowTop)
            {
                splitContainer1.Panel1.Controls.Add(propertiesEditor);
                splitContainer1.Panel2.Controls.Add(stPanel1);

                splitContainer1.Orientation = Orientation.Horizontal;
            }
            else
            {
                splitContainer1.Panel2.Controls.Add(propertiesEditor);
                splitContainer1.Panel1.Controls.Add(stPanel1);

                splitContainer1.Orientation = Orientation.Vertical;
            }

            OnDataAcquiredEvent += new DataAcquired(ThreadReportsDataAquiredEvent);
        }

        private void UpdateBackgroundImage()
        {
            switch (Runtime.pictureBoxStyle)
            {
                case Runtime.PictureBoxBG.Black:
                    pictureBoxCustom1.BackColor = Color.Black;
                    pictureBoxCustom1.BackgroundImage = null;
                    break;
                case Runtime.PictureBoxBG.White:
                    pictureBoxCustom1.BackColor = Color.White;
                    pictureBoxCustom1.BackgroundImage = null;
                    break;
                case Runtime.PictureBoxBG.Checkerboard:
                    pictureBoxCustom1.BackColor = Color.Transparent;
                    pictureBoxCustom1.BackgroundImage = pictureBoxCustom1.GetCheckerBackground();
                    break;
            }
        }

        public void LoadProperties(object prop) => propertiesEditor.LoadProperties(prop);
       

        public void LoadImage(STGenericTexture texture)
        {
            ResetChannelEditor();

            HasBeenEdited = false;

            if (texture.CanEdit)
            {
                imageToolStripMenuItem.Enabled = true;
                adjustmentsToolStripMenuItem.Enabled = true;
            }

            ActiveTexture = texture;
            CurMipDisplayLevel = 0;
            CurArrayDisplayLevel = 0;
            hasBeenEdited = false;

            UpdateMipDisplay();
        }
        private Bitmap SetChannel(Bitmap image, int ChannelIndex)
        {
            if (ChannelIndex == 0)
                return image;
            else if (ChannelIndex == 1)
                return BitmapExtension.ShowChannel(image, STChannelType.Red);
            else if (ChannelIndex == 2)
                return BitmapExtension.ShowChannel(image, STChannelType.Green);
            else if (ChannelIndex == 3)
                return BitmapExtension.ShowChannel(image, STChannelType.Blue);
            else
                return BitmapExtension.ShowChannel(image, STChannelType.Alpha);
        }
        private bool isFinished;
        private bool IsFinished
        {
            get
            {
                return isFinished;
            }
            set
            {
                isFinished = value;

                if (isFinished == true && CurrentChannelIndex == 0)
                {
                    LoadChannelEditor(pictureBoxCustom1.Image);
                }
            }
        }

        private void SetPicture(Image img)
        {
            if (pictureBoxCustom1.InvokeRequired)
            {
                pictureBoxCustom1.Invoke(new MethodInvoker(
                delegate ()
                {
                    pictureBoxCustom1.Image = img;
                }));
            }
            else
            {
                pictureBoxCustom1.Image = img;
            }
        }

        public delegate void DataAcquired(object sender);

        public event DataAcquired OnDataAcquiredEvent;

        private bool DecodeProcessFinished = false; //Used to determine when the decode process is done

        private void UpdatePictureBox(int ChannelIndex = 0)
        {
            DecodeProcessFinished = false;

            PushImage(Properties.Resources.LoadingImage);

            var image = ActiveTexture.GetBitmap(CurArrayDisplayLevel, CurMipDisplayLevel);

            if (image != null)
            {
                if (ChannelIndex == 1)
                    BitmapExtension.SetChannel(image, STChannelType.Red, STChannelType.Red, STChannelType.Red, STChannelType.One);
                else if (ChannelIndex == 2)
                    BitmapExtension.SetChannel(image, STChannelType.Green, STChannelType.Green, STChannelType.Green, STChannelType.One);
                else if (ChannelIndex == 3)
                    BitmapExtension.SetChannel(image, STChannelType.Blue, STChannelType.Blue, STChannelType.Blue, STChannelType.One);
                else if (ChannelIndex == 4)
                    BitmapExtension.SetChannel(image, STChannelType.Alpha, STChannelType.Alpha, STChannelType.Alpha, STChannelType.One);
                else
                {
                    if (!toggleAlphaChk.Checked)
                    {
                        BitmapExtension.SetChannel(image, STChannelType.Red, STChannelType.Green, STChannelType.Blue, STChannelType.One);
                    }
                }

                DecodeProcessFinished = true;

                // BitmapExtension.SetChannels(image, HasRedChannel, HasBlueChannel, HasGreenChannel, HasAlphaChannel);
                PushImage(image);
            }

        }

        private void PushImage(Image image)
        {
            if (pictureBoxCustom1.InvokeRequired)
            {
                pictureBoxCustom1.Invoke(this.OnDataAcquiredEvent,
                new object[] { image });
            }
            else
            {
                pictureBoxCustom1.Image = image;
                pictureBoxCustom1.Refresh();

                if (DecodeProcessFinished)
                    IsFinished = true;
            }
        }

        private void ThreadReportsDataAquiredEvent(object sender)
        {
            pictureBoxCustom1.Image = (Image)sender;
            pictureBoxCustom1.Refresh();

            if (DecodeProcessFinished)
                IsFinished = true;
        }

        public int CurrentChannelIndex;

        bool IsCancelled = false;
        public void UpdateMipDisplay()
        {
            if (HasBeenEdited)
            {
                var message = MessageBox.Show("This texture has been edited! Would you like to apply the changes made?", "Texture Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (message == DialogResult.Yes)
                    ApplyEdit();

                HasBeenEdited = false;
            }

            IsFinished = false;

            if (ActiveTexture.ArrayCount == 0)
                ActiveTexture.ArrayCount = 1;
            if (ActiveTexture.MipCount == 0)
                ActiveTexture.MipCount = 1;

            TotalMipCount = ActiveTexture.MipCount - 1;
            TotalArrayCount = ActiveTexture.ArrayCount - 1;


            CurrentChannelIndex = 0;

            if (propertiesEditor != null && propertiesEditor.channelListView.SelectedIndices.Count > 0)
                CurrentChannelIndex = propertiesEditor.channelListView.SelectedIndices[0];

            if (this.Thread != null && this.Thread.IsAlive && Thread.ThreadState.ToString() != "AbortRequested")
            {
                IsCancelled = true;
            }

            Thread = new Thread((ThreadStart)(() =>
            {
                UpdatePictureBox(CurrentChannelIndex);
            }));
            Thread.Start();


            arrayLevelCounterLabel.Text = $"Array Level: {CurArrayDisplayLevel} / {TotalArrayCount}";
            mipLevelCounterLabel.Text = $"Mip Level: {CurMipDisplayLevel} / {TotalMipCount}";


            if (CurMipDisplayLevel != TotalMipCount)
                BtnMipsRight.Enabled = true;
            else
                BtnMipsRight.Enabled = false;

            if (CurMipDisplayLevel != 0)
                BtmMipsLeft.Enabled = true;
            else
                BtmMipsLeft.Enabled = false;

            if (CurArrayDisplayLevel != TotalArrayCount)
                btnRightArray.Enabled = true;
            else
                btnRightArray.Enabled = false;

            if (CurArrayDisplayLevel != 0)
                btnLeftArray.Enabled = true;
            else
                btnLeftArray.Enabled = false;
        }
        private void imageBGComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Runtime.pictureBoxStyle = (Runtime.PictureBoxBG)imageBGComboBox.SelectedItem;
            UpdateBackgroundImage();
        }

        private void BtmMipsLeft_Click(object sender, EventArgs e)
        {
            if (CurMipDisplayLevel != 0)
                CurMipDisplayLevel -= 1;

            UpdateMipDisplay();
        }

        private void BtnMipsRight_Click(object sender, EventArgs e)
        {
            if (CurMipDisplayLevel != TotalMipCount)
                CurMipDisplayLevel += 1;

            UpdateMipDisplay();
        }

        private void btnLeftArray_Click(object sender, EventArgs e)
        {
            if (CurArrayDisplayLevel != 0)
                CurArrayDisplayLevel -= 1;

            UpdateMipDisplay();
        }

        private void btnRightArray_Click(object sender, EventArgs e)
        {
            if (CurArrayDisplayLevel != TotalArrayCount)
                CurArrayDisplayLevel += 1;

            UpdateMipDisplay();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveTexture.ExportImage();
        }

        private void propertyGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!propertyGridToolStripMenuItem.Checked)
            {
                propertyGridToolStripMenuItem.Checked = false;
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.Panel2.Hide();
            }
            else
            {
                propertyGridToolStripMenuItem.Checked = true;
                splitContainer1.Panel2Collapsed = false;
                splitContainer1.Panel2.Show();
            }
            Runtime.ImageEditor.ShowPropertiesPanel = propertyGridToolStripMenuItem.Checked;
        }

        private void stPropertyGrid1_Load(object sender, EventArgs e)
        {
        }

        private void btnRedCompSel_Click(object sender, EventArgs e)
        {
            if (HasRedChannel)
                HasRedChannel = false;
            else
                HasRedChannel = true;

            UpdateMipDisplay();
        }

        private void btnGreenCompSel_Click(object sender, EventArgs e)
        {
            if (HasGreenChannel)
                HasGreenChannel = false;
            else
                HasGreenChannel = true;

            UpdateMipDisplay();
        }

        private void btnBlueCompSel_Click(object sender, EventArgs e)
        {
            if (HasBlueChannel)
                HasBlueChannel = false;
            else
                HasBlueChannel = true;

            UpdateMipDisplay();
        }

        private void btnAlphaCompSel_Click(object sender, EventArgs e)
        {
            if (HasAlphaChannel)
                HasAlphaChannel = false;
            else
                HasAlphaChannel = true;

            UpdateMipDisplay();
        }

        private void channelViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadChannelEditor(pictureBoxCustom1.Image);
        }

        public delegate void UpdateChannelViewer();
        private void LoadChannelEditor(Image bitmap)
        {
            if (bitmap == null)
                return;

            propertiesEditor.LoadImage(new Bitmap(bitmap), this);
        }

        private void ResetChannelEditor()
        {
            propertiesEditor.ResetChannels();
        }

        private void InvokeMethod()
        {
        }

        private void fliVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.RotateNoneFlipY);
        }

        private void flipHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.RotateNoneFlipX);
        }

        private void rotate90ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.Rotate90FlipNone);
        }

        private void rotate90CounterClockwiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.Rotate90FlipX);
        }

        private void rToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.Rotate180FlipNone);
        }

        private void RotateImage(RotateFlipType rotation)
        {
            Image Image = pictureBoxCustom1.Image;
            if (Image != null)
            {
                Image.RotateFlip(rotation);
                UpdateEdit(Image);
            }
        }

        private void UpdateEdit(Image image)
        {
            ActiveTexture.EditedImages = new EditedBitmap[ActiveTexture.ArrayCount];
            ActiveTexture.EditedImages[CurArrayDisplayLevel] = new EditedBitmap()
            {
                bitmap = new Bitmap(image),
                ArrayLevel = CurArrayDisplayLevel
            };

            pictureBoxCustom1.Image = image;

            TotalMipCount = ActiveTexture.MipCount - 1;
            TotalArrayCount = ActiveTexture.ArrayCount - 1;

            arrayLevelCounterLabel.Text = $"Array Level: {CurArrayDisplayLevel} / {TotalArrayCount}";
            mipLevelCounterLabel.Text = $"Mip Level: {CurMipDisplayLevel} / {TotalMipCount}";


            propertiesEditor.UpdateProperties();

            HasBeenEdited = true;
        }

        private void ApplyEdit()
        {
            STProgressBar progressBar = new STProgressBar();
            progressBar.Task = "Applying Edits";
            progressBar.Value = 0;
            progressBar.StartPosition = FormStartPosition.CenterScreen;
            progressBar.Show();
            progressBar.Refresh();


            Image Image = pictureBoxCustom1.Image;
            if (Image != null)
            {
                ActiveTexture.Width = (uint)Image.Width;
                ActiveTexture.Height = (uint)Image.Height;

                ActiveTexture.SetImageData(new Bitmap(Image), CurArrayDisplayLevel);

                CurMipDisplayLevel = 0;
                HasBeenEdited = false;
            }

            ActiveTexture.EditedImages[CurArrayDisplayLevel].bitmap.Dispose();
            ActiveTexture.EditedImages[CurArrayDisplayLevel] = null;

            progressBar.Value = 100;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            ApplyEdit();
        }

        private void ChannelBtn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.Text == "R")
            {
                if (HasRedChannel) {
                    HasRedChannel = false;
                    redChannelBtn.BackColor = FormThemes.BaseTheme.DisabledItemColor;
                }
                else {
                    HasRedChannel = true;
                    redChannelBtn.BackColor = Color.FromArgb(192, 0, 0);
                }
            }
            else if (btn.Text == "B")
            {
                if (HasBlueChannel)
                {
                    HasBlueChannel = false;
                    blueChannelBtn.BackColor = FormThemes.BaseTheme.DisabledItemColor;
                }
                else
                {
                    HasBlueChannel = true;
                    blueChannelBtn.BackColor = Color.FromArgb(0, 0, 192);
                }
            }
            else if (btn.Text == "G")
            {
                if (HasGreenChannel)
                {
                    HasGreenChannel = false;
                    greenChannelBtn.BackColor = FormThemes.BaseTheme.DisabledItemColor;
                }
                else
                {
                    HasGreenChannel = true;
                    greenChannelBtn.BackColor = Color.FromArgb(0, 192, 0);
                }
            }
            else if (btn.Text == "A")
            {
                if (HasAlphaChannel)
                {
                    HasAlphaChannel = false;
                    alphaChannelBtn.BackColor = FormThemes.BaseTheme.DisabledItemColor;
                }
                else
                {
                    HasAlphaChannel = true;
                    alphaChannelBtn.BackColor = Color.Silver;
                }
            }

            UpdateMipDisplay();
        }

        private void resizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageResizeDialog resizeEditor = new ImageResizeDialog();
            resizeEditor.LoadImage(pictureBoxCustom1.Image);
            if (resizeEditor.ShowDialog() == DialogResult.OK)
            {
                UpdateEdit(resizeEditor.newImage);
            }
        }

        private void reEncodeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ImageReEncodeDialog encodingEditor = new ImageReEncodeDialog();
            encodingEditor.LoadImage(pictureBoxCustom1.Image, ActiveTexture);
            if (encodingEditor.ShowDialog() == DialogResult.OK)
            {
                ActiveTexture.Format = encodingEditor.Format;
                ActiveTexture.MipCount = (uint)encodingEditor.MipCount;

                UpdateEdit(encodingEditor.newImage);
            }
        }

        private void hueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HueSaturationAdjuster hsvEditor = new HueSaturationAdjuster();
            hsvEditor.LoadBitmap(pictureBoxCustom1);

            if (hsvEditor.ShowDialog() == DialogResult.OK)
            {
            }
        }

        private void toggleAlphaChk_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePictureBox();
        }
    }
}
