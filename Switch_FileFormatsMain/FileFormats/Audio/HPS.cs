﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Switch_Toolbox;
using System.Windows.Forms;
using Switch_Toolbox.Library;
using Switch_Toolbox.Library.Forms;
using VGAudio.Formats;
using VGAudio.Containers.NintendoWare;

namespace FirstPlugin
{
    public class HPS : VGAdudioFile, IEditor<AudioPlayer>, IFileFormat
    {
        public bool CanSave { get; set; }
        public string[] Description { get; set; } = new string[] { "hps" };
        public string[] Extension { get; set; } = new string[] { "*.HPS" };
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public IFileInfo IFileInfo { get; set; }

        public bool Identify(System.IO.Stream stream)
        {
            using (var reader = new Switch_Toolbox.Library.IO.FileReader(stream, true))
            {
                return reader.CheckSignature(8, " HALPST\0");
            }
        }

        public Type[] Types
        {
            get
            {
                List<Type> types = new List<Type>();
                return types.ToArray();
            }
        }

        public AudioPlayer OpenForm()
        {
            AudioPlayer form = new AudioPlayer();
            form.Text = FileName;
            form.Dock = DockStyle.Fill;
            form.LoadFile(audioData, this);
            return form;
        }

        public void Load(System.IO.Stream stream)
        {
            CanSave = true;
            LoadAudio(stream, this);
        }
        public void Unload()
        {

        }
        public byte[] Save()
        {
            return SaveAudio();
        }
    }
}
