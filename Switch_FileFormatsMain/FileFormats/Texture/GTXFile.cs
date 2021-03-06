﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Switch_Toolbox.Library.IO;
using Switch_Toolbox.Library;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using Switch_Toolbox.Library.Forms;
using Bfres.Structs;

namespace FirstPlugin
{
    public enum BlockType : uint
    {
        Invalid = 0x00,
        EndOfFile = 0x01,
        AlignData = 0x02,
        VertexShaderHeader = 0x03,
        VertexShaderProgram = 0x05,
        PixelShaderHeader = 0x06,
        PixelShaderProgram = 0x07,
        GeometryShaderHeader = 0x08,
        GeometryShaderProgram = 0x09,
        GeometryShaderProgram2 = 0x10,
        ImageInfo = 0x11,
        ImageData = 0x12,
        MipData = 0x13,
        ComputeShaderHeader = 0x14,
        ComputeShader = 0x15,
        UserBlock = 0x16,
    }

    public class GTXFile : TreeNodeFile, IFileFormat
    {
        public bool CanSave { get; set; }
        public string[] Description { get; set; } = new string[] { "GTX" };
        public string[] Extension { get; set; } = new string[] { "*.gtx" };
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public IFileInfo IFileInfo { get; set; }

        public bool Identify(System.IO.Stream stream)
        {
            using (var reader = new Switch_Toolbox.Library.IO.FileReader(stream, true))
            {
                return reader.CheckSignature(4, "Gfx2");
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
        private GTXHeader header;

        public List<byte[]> data = new List<byte[]>();
        public List<byte[]> mipMaps = new List<byte[]>();
        public List<TextureData> textures = new List<TextureData>();

        public List<GTXDataBlock> blocks = new List<GTXDataBlock>();

        public void Load(System.IO.Stream stream)
        {
            CanSave = true;
            Text = FileName;

            ReadGx2(new FileReader(stream));

            ContextMenu = new ContextMenu();
            MenuItem save = new MenuItem("Save");
            ContextMenu.MenuItems.Add(save);
            save.Click += Save;
        }
        private void Save(object sender, EventArgs args)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "gtx";
            sfd.Filter = "Supported Formats|*.gtx;";
            sfd.FileName = FileName;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                STFileSaver.SaveFileFormat(this, sfd.FileName);
            }
        }
        public void Unload()
        {
        }
        public byte[] Save()
        {
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            using (FileWriter writer = new FileWriter(mem))
            {
                writer.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                header.Write(writer);

                uint surfBlockType;
                uint dataBlockType;
                uint mipBlockType;

                if (header.MajorVersion == 6)
                {
                    surfBlockType = 0x0A;
                    dataBlockType = 0x0B;
                    mipBlockType = 0x0C;
                }
                else if (header.MajorVersion == 7)
                {
                    surfBlockType = 0x0B;
                    dataBlockType = 0x0C;
                    mipBlockType = 0x0D;
                }
                else
                    throw new Exception($"Unsupported GTX version {header.MajorVersion}");

                writer.Seek(header.HeaderSize, System.IO.SeekOrigin.Begin);
                foreach (var tex in textures)
                {
                    tex.surface.Write(writer);
                }

                foreach (var block in blocks)
                {
                    if ((uint)block.BlockType == surfBlockType)
                    {
                        block.Write(writer);
                    }
                    else
                    {
                        block.Write(writer);
                    }
                }
            }
            return mem.ToArray();
        }
        private void ReadGx2(FileReader reader)
        {
            reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;

            header = new GTXHeader();
            header.Read(reader);

            Console.WriteLine("header size " + header.HeaderSize);

            uint surfBlockType;
            uint dataBlockType;
            uint mipBlockType;

            if (header.MajorVersion == 6)
            {
                surfBlockType = 0x0A;
                dataBlockType = 0x0B;
                mipBlockType = 0x0C;
            }
            else if (header.MajorVersion == 7)
            {
                surfBlockType = 0x0B;
                dataBlockType = 0x0C;
                mipBlockType = 0x0D;
            }
            else
                throw new Exception($"Unsupported GTX version {header.MajorVersion}");

            if (header.GpuVersion != 2)
                throw new Exception($"Unsupported GPU version {header.GpuVersion}");

            reader.Position = header.HeaderSize;

            bool blockB = false;
            bool blockC = false;

            uint ImageInfo = 0;
            uint images = 0;

            while (reader.Position < reader.BaseStream.Length)
            {
                GTXDataBlock block = new GTXDataBlock();
                block.Read(reader);
                blocks.Add(block);

                //Here we use "if" instead of "case" statements as types vary between versions
                if ((uint)block.BlockType == surfBlockType)
                {
                    ImageInfo += 1;
                    blockB = true;

                    var surface = new SurfaceInfoParse();
                    surface.Read(new FileReader(block.data));

                    if (surface.numMips > 14)
                        throw new Exception($"Invalid number of mip maps {surface.numMips}!");

                    TextureData textureData = new TextureData();
                    textureData.surface = surface;
                    textureData.MipCount = surface.numMips;
                    textureData.ArrayCount = surface.numArray;
                    textureData.Text = "Texture" + ImageInfo;
                    Nodes.Add(textureData);
                    textures.Add(textureData);
                }
                else if ((uint)block.BlockType == dataBlockType)
                {
                    images += 1;
                    blockC = true;

                    data.Add(block.data);
                }
                else if ((uint)block.BlockType == mipBlockType)
                {
                    mipMaps.Add(block.data);
                }
            }
            if (textures.Count != data.Count)
                throw new Exception($"Bad size! {textures.Count} {data.Count}");

            int curTex = 0;
            int curMip = 0;
            foreach (var node in Nodes)
            {
                TextureData tex = (TextureData)node;

                tex.surface.data = data[curTex];
                tex.surface.bpp = GTX.surfaceGetBitsPerPixel(tex.surface.format) >> 3;
                tex.Format = FTEX.ConvertFromGx2Format((Syroot.NintenTools.Bfres.GX2.GX2SurfaceFormat)tex.surface.format);
                tex.Width = tex.surface.width;
                tex.Height = tex.surface.height;

                if (tex.surface.numMips > 1)
                    tex.surface.mipData = mipMaps[curMip++];
                else
                    tex.surface.mipData = new byte[0];

                if (tex.surface.mipData == null)
                    tex.surface.numMips = 1;

                curTex++;
            }
            reader.Close();
            reader.Dispose();
        }
        public class GTXHeader
        {
            readonly string Magic = "Gfx2";
            public uint HeaderSize;
            public uint MajorVersion;
            public uint MinorVersion;
            public uint GpuVersion;
            public uint AlignMode;

            public void Read(FileReader reader)
            {
                string Signature = reader.ReadString(4, Encoding.ASCII);
                if (Signature != Magic)
                    throw new Exception($"Invalid signature {Signature}! Expected Gfx2.");

                HeaderSize = reader.ReadUInt32();
                MajorVersion = reader.ReadUInt32();
                MinorVersion = reader.ReadUInt32();
                GpuVersion = reader.ReadUInt32();
                AlignMode = reader.ReadUInt32();
            }
            public void Write(FileWriter writer)
            {
                writer.WriteSignature(Magic);
                writer.Write(HeaderSize);
                writer.Write(MajorVersion);
                writer.Write(MinorVersion);
                writer.Write(GpuVersion);
                writer.Write(AlignMode);
            }
        }
        public class GTXDataBlock
        {
            readonly string Magic = "BLK{";
            public uint HeaderSize;
            public uint MajorVersion;
            public uint MinorVersion;
            public BlockType BlockType;
            public uint Identifier;
            public uint index;
            public uint DataSize;
            public byte[] data;

            public void Read(FileReader reader)
            {
                long blockStart = reader.Position;

                string Signature = reader.ReadString(4, Encoding.ASCII);
                if (Signature != Magic)
                    throw new Exception($"Invalid signature {Signature}! Expected BLK.");

                HeaderSize = reader.ReadUInt32();
                MajorVersion = reader.ReadUInt32(); //Must be 0x01 for 6.x.x
                MinorVersion = reader.ReadUInt32(); //Must be 0x00 for 6.x.x
                BlockType = reader.ReadEnum<BlockType>(false);
                DataSize = reader.ReadUInt32();
                Identifier = reader.ReadUInt32();
                index = reader.ReadUInt32();

                reader.Seek(blockStart + HeaderSize, System.IO.SeekOrigin.Begin);
                data = reader.ReadBytes((int)DataSize);
            }
            public void Write(FileWriter writer)
            {
                long blockStart = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(HeaderSize);
                writer.Write(MajorVersion);
                writer.Write(MinorVersion);
                writer.Write(BlockType, false);
                writer.Write(DataSize);
                writer.Write(Identifier);
                writer.Write(index);
                writer.Seek(blockStart + HeaderSize, System.IO.SeekOrigin.Begin);

                writer.Write(data);
            }
        }
        public class TextureData : STGenericTexture
        {
            public override TEX_FORMAT[] SupportedFormats
            {
                get
                {
                    return new TEX_FORMAT[]
                    {
                        TEX_FORMAT.BC1_UNORM,
                        TEX_FORMAT.BC1_UNORM_SRGB,
                        TEX_FORMAT.BC2_UNORM,
                        TEX_FORMAT.BC2_UNORM_SRGB,
                        TEX_FORMAT.BC3_UNORM,
                        TEX_FORMAT.BC3_UNORM_SRGB,
                        TEX_FORMAT.BC4_UNORM,
                        TEX_FORMAT.BC4_SNORM,
                        TEX_FORMAT.BC5_UNORM,
                        TEX_FORMAT.BC5_SNORM,
                        TEX_FORMAT.B5G5R5A1_UNORM,
                        TEX_FORMAT.B5G6R5_UNORM,
                        TEX_FORMAT.B8G8R8A8_UNORM_SRGB,
                        TEX_FORMAT.B8G8R8A8_UNORM,
                        TEX_FORMAT.R10G10B10A2_UNORM,
                        TEX_FORMAT.R16_UNORM,
                        TEX_FORMAT.B4G4R4A4_UNORM,
                        TEX_FORMAT.B5_G5_R5_A1_UNORM,
                        TEX_FORMAT.R8G8B8A8_UNORM_SRGB,
                        TEX_FORMAT.R8G8B8A8_UNORM,
                        TEX_FORMAT.R8_UNORM,
                        TEX_FORMAT.R8G8_UNORM,
                        TEX_FORMAT.R32G8X24_FLOAT,
                    };
                }
            }

            public override bool CanEdit { get; set; } = false;

            public SurfaceInfoParse surface;

            public TextureData()
            {
                ImageKey = "Texture";
                SelectedImageKey = "Texture";

                ContextMenu = new ContextMenu();
                MenuItem export = new MenuItem("Export");
                ContextMenu.MenuItems.Add(export);
                export.Click += Export;
                MenuItem replace = new MenuItem("Replace");
                ContextMenu.MenuItems.Add(replace);
                replace.Click += Replace;
                MenuItem remove = new MenuItem("Remove");
                ContextMenu.MenuItems.Add(remove);
                remove.Click += Remove;
            }

            public override void SetImageData(Bitmap bitmap, int ArrayLevel)
            {
                throw new NotImplementedException("Cannot set image data! Operation not implemented!");
            }

            public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0)
            {
                Console.WriteLine("");
                Console.WriteLine("// ----- GX2Surface Info ----- ");
                Console.WriteLine("  dim             = " + surface.dim);
                Console.WriteLine("  width           = " + surface.width);
                Console.WriteLine("  height          = " + surface.height);
                Console.WriteLine("  depth           = " + surface.depth);
                Console.WriteLine("  numMips         = " + surface.numMips);
                Console.WriteLine("  format          = " + surface.format);
                Console.WriteLine("  aa              = " + surface.aa);
                Console.WriteLine("  use             = " + surface.use);
                Console.WriteLine("  imageSize       = " + surface.imageSize);
                Console.WriteLine("  mipSize         = " + surface.mipSize);
                Console.WriteLine("  tileMode        = " + surface.tileMode);
                Console.WriteLine("  swizzle         = " + surface.swizzle);
                Console.WriteLine("  alignment       = " + surface.alignment);
                Console.WriteLine("  pitch           = " + surface.pitch);
                Console.WriteLine("  bits per pixel  = " + (surface.bpp << 3));
                Console.WriteLine("  bytes per pixel = " + surface.bpp);
                Console.WriteLine("  data size       = " + surface.data.Length);
                Console.WriteLine("  mip size        = " + surface.mipData.Length);
                Console.WriteLine("  realSize        = " + surface.imageSize);

                var surfaces = GTX.Decode(surface);

                return surfaces[ArrayLevel][MipLevel];
            }
            private void Remove(object sender, EventArgs args)
            {
                ((GTXFile)Parent).Nodes.Remove(this);
            }
            private void Export(object sender, EventArgs args)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = Text;
                sfd.DefaultExt = "bftex";
                sfd.Filter = "Supported Formats|*.bftex;*.dds; *.png;*.tga;*.jpg;*.tiff|" +
                             "Binary Texture |*.bftex|" +
                             "Microsoft DDS |*.dds|" +
                             "Portable Network Graphics |*.png|" +
                             "Joint Photographic Experts Group |*.jpg|" +
                             "Bitmap Image |*.bmp|" +
                             "Tagged Image File Format |*.tiff|" +
                             "All files(*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Export(sfd.FileName);
                }
            }
            public void Export(string FileName, bool ExportSurfaceLevel = false,
     bool ExportMipMapLevel = false, int SurfaceLevel = 0, int MipLevel = 0)
            {
                string ext = System.IO.Path.GetExtension(FileName);
                ext = ext.ToLower();

                switch (ext)
                {
                    case ".dds":
                        SaveDDS(FileName);
                        break;
                    default:
                        SaveBitMap(FileName);
                        break;
                }
            }
            private void Replace(object sender, EventArgs args)
            {
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
                    Replace(ofd.FileName);
                }
            }
            public void Replace(string FileName)
            {

            }
            public override void OnClick(TreeView treeView)
            {
                UpdateEditor();
            }

            public void UpdateEditor()
            {
                ImageEditorBase editor = (ImageEditorBase)LibraryGUI.Instance.GetActiveContent(typeof(ImageEditorBase));
                if (editor == null)
                {
                    editor = new ImageEditorBase();
                    editor.Dock = DockStyle.Fill;

                    LibraryGUI.Instance.LoadEditor(editor);
                }
                editor.Text = Text;

                UserDataEditor userDataEditor = new UserDataEditor();
                userDataEditor.Name = "User Data";
                editor.LoadImage(this);
                editor.AddCustomControl(userDataEditor, typeof(UserDataEditor));
            }
        }
        public class SurfaceInfoParse : GTX.GX2Surface
        {

            public void Read(FileReader reader)
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;

                dim = reader.ReadUInt32();
                width = reader.ReadUInt32();
                height = reader.ReadUInt32();
                depth = reader.ReadUInt32();
                numMips = reader.ReadUInt32();
                format = reader.ReadUInt32();
                aa = reader.ReadUInt32();
                use = reader.ReadUInt32();
                imageSize = reader.ReadUInt32();
                imagePtr = reader.ReadUInt32();
                mipSize = reader.ReadUInt32();
                mipPtr = reader.ReadUInt32();
                tileMode = reader.ReadUInt32();
                swizzle = reader.ReadUInt32();
                alignment = reader.ReadUInt32();
                pitch = reader.ReadUInt32();
                mipOffset = reader.ReadUInt32s(13);
                firstMip = reader.ReadUInt32();
                imageCount = reader.ReadUInt32();
                firstSlice = reader.ReadUInt32();
                numSlices = reader.ReadUInt32();
                compSel = reader.ReadBytes(4);
                texRegs = reader.ReadUInt32s(5);
            }
            public void Write(FileWriter writer)
            {
                writer.Write(dim);
                writer.Write(width);
                writer.Write(height);
                writer.Write(depth);
                writer.Write(numMips);
                writer.Write(format);
                writer.Write(aa);
                writer.Write(use);
                writer.Write(imageSize);
                writer.Write(imagePtr);
                writer.Write(mipSize);
                writer.Write(mipPtr);
                writer.Write(tileMode);
                writer.Write(swizzle);
                writer.Write(alignment);
                writer.Write(pitch);
                writer.Write(mipOffset);
                writer.Write(firstMip);
                writer.Write(imageCount);
                writer.Write(firstSlice);
                writer.Write(numSlices);
                writer.Write(compSel);
                writer.Write(texRegs);
            }
        }
    }


}