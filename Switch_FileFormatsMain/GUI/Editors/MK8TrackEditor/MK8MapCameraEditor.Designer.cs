﻿namespace FirstPlugin.Turbo
{
    partial class MK8MapCameraEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MK8MapCameraEditor));
            this.stPropertyGrid1 = new Switch_Toolbox.Library.Forms.STPropertyGrid();
            this.pictureBoxCustom1 = new Switch_Toolbox.Library.Forms.PictureBoxCustom();
            this.leBtnRadio = new System.Windows.Forms.RadioButton();
            this.beBtnRadio = new System.Windows.Forms.RadioButton();
            this.contentContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustom1)).BeginInit();
            this.SuspendLayout();
            // 
            // contentContainer
            // 
            this.contentContainer.Controls.Add(this.beBtnRadio);
            this.contentContainer.Controls.Add(this.leBtnRadio);
            this.contentContainer.Controls.Add(this.pictureBoxCustom1);
            this.contentContainer.Controls.Add(this.stPropertyGrid1);
            this.contentContainer.Size = new System.Drawing.Size(570, 397);
            this.contentContainer.Controls.SetChildIndex(this.stPropertyGrid1, 0);
            this.contentContainer.Controls.SetChildIndex(this.pictureBoxCustom1, 0);
            this.contentContainer.Controls.SetChildIndex(this.leBtnRadio, 0);
            this.contentContainer.Controls.SetChildIndex(this.beBtnRadio, 0);
            // 
            // stPropertyGrid1
            // 
            this.stPropertyGrid1.AutoScroll = true;
            this.stPropertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
            this.stPropertyGrid1.Location = new System.Drawing.Point(346, 25);
            this.stPropertyGrid1.Name = "stPropertyGrid1";
            this.stPropertyGrid1.Size = new System.Drawing.Size(224, 372);
            this.stPropertyGrid1.TabIndex = 11;
            // 
            // pictureBoxCustom1
            // 
            this.pictureBoxCustom1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxCustom1.BackColor = System.Drawing.Color.Empty;
            this.pictureBoxCustom1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxCustom1.BackgroundImage")));
            this.pictureBoxCustom1.Location = new System.Drawing.Point(3, 56);
            this.pictureBoxCustom1.Name = "pictureBoxCustom1";
            this.pictureBoxCustom1.Size = new System.Drawing.Size(337, 341);
            this.pictureBoxCustom1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxCustom1.TabIndex = 12;
            this.pictureBoxCustom1.TabStop = false;
            // 
            // leBtnRadio
            // 
            this.leBtnRadio.AutoSize = true;
            this.leBtnRadio.Location = new System.Drawing.Point(249, 32);
            this.leBtnRadio.Name = "leBtnRadio";
            this.leBtnRadio.Size = new System.Drawing.Size(83, 17);
            this.leBtnRadio.TabIndex = 13;
            this.leBtnRadio.TabStop = true;
            this.leBtnRadio.Text = "Little Endian";
            this.leBtnRadio.UseVisualStyleBackColor = true;
            // 
            // beBtnRadio
            // 
            this.beBtnRadio.AutoSize = true;
            this.beBtnRadio.Location = new System.Drawing.Point(167, 32);
            this.beBtnRadio.Name = "beBtnRadio";
            this.beBtnRadio.Size = new System.Drawing.Size(76, 17);
            this.beBtnRadio.TabIndex = 14;
            this.beBtnRadio.TabStop = true;
            this.beBtnRadio.Text = "Big Endian";
            this.beBtnRadio.UseVisualStyleBackColor = true;
            this.beBtnRadio.CheckedChanged += new System.EventHandler(this.beBtnRadio_CheckedChanged);
            // 
            // MK8MapCameraEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 402);
            this.Name = "MK8MapCameraEditor";
            this.contentContainer.ResumeLayout(false);
            this.contentContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustom1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Switch_Toolbox.Library.Forms.STPropertyGrid stPropertyGrid1;
        private Switch_Toolbox.Library.Forms.PictureBoxCustom pictureBoxCustom1;
        private System.Windows.Forms.RadioButton beBtnRadio;
        private System.Windows.Forms.RadioButton leBtnRadio;
    }
}
