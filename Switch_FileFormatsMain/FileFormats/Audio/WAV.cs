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
    public class WAV : IEditor<AudioPlayer>, IFileFormat
    {
        public bool CanSave { get; set; }
        public string[] Description { get; set; } = new string[] { "Waveform Audio" };
        public string[] Extension { get; set; } = new string[] { "*.wav" };
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public IFileInfo IFileInfo { get; set; }

        public bool Identify(System.IO.Stream stream)
        {
            using (var reader = new Switch_Toolbox.Library.IO.FileReader(stream, true))
            {
                bool IsValidSig = reader.CheckSignature(4, "WAVE"); //RIFF is also used in avi so just use WAVE
                bool IsValidExt = reader.CheckSignature(4, ".wav");

                if (IsValidExt || IsValidSig)
                    return true;
                else
                    return false;
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

            //  form.AddFileContextEvent("Save", Save);
            //   form.AddFileContextEvent("Replace", Replace);
            //   form.LoadProperties(prop);

            return form;
        }

        AudioData audioData;
        public void Load(System.IO.Stream stream)
        {
            CanSave = true;


            audioData = new BCFstmReader().Read(stream);
        }
        public void Unload()
        {

        }
        public byte[] Save()
        {
            var writer = new BCFstmWriter(NwTarget.Ctr);
            writer.Configuration = new BxstmConfiguration()
            {
                Endianness = VGAudio.Utilities.Endianness.LittleEndian,
            };

            return writer.GetFile(audioData, writer.Configuration);
        }
    }
}
