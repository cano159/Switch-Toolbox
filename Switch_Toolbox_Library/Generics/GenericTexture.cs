﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Switch_Toolbox.Library.IO;
using OpenTK.Graphics.OpenGL;
using Switch_Toolbox.Library.Rendering;
using Ryujinx.Graphics.Gal.Texture; //For ASTC
using Switch_Toolbox.Library.NodeWrappers;

namespace Switch_Toolbox.Library
{
    public enum STChannelType
    {
        Red,
        Green,
        Blue,
        Alpha,
        One,
        Zero
    }

    public class EditedBitmap
    {
        public int ArrayLevel = 0;
        public Bitmap bitmap;
    }

    public abstract class STGenericTexture : STGenericWrapper
    {
        public STGenericTexture()
        {
            RenderableTex = new RenderableTex();
            RenderableTex.GLInitialized = false;
        }

        public bool IsSwizzled { get; set; } = true;

        /// <summary>
        /// Is the texture edited or not. Used for the image editor for saving changes.
        /// </summary>
        public bool IsEdited { get; set; } = false;

        /// <summary>
        /// An array of <see cref="EditedBitmap"/> from the image editor to be saved back.
        /// </summary>
        public EditedBitmap[] EditedImages { get; set; }

        //If the texture can be edited or not. Disables some functions in image editor if false
        //If true, the editors will call "SetImageData" for setting data back to the original data.
        public abstract bool CanEdit { get; set; }

        public STChannelType RedChannel;
        public STChannelType GreenChannel;
        public STChannelType BlueChannel;
        public STChannelType AlphaChannel;

        public abstract byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0);

        public List<Surface> GetSurfaces()
        {
            var surfaces = new List<Surface>();
            for (int arrayLevel = 0; arrayLevel < ArrayCount; arrayLevel++)
            {
                List<byte[]> mips = new List<byte[]>();
                for (int mipLevel = 0; mipLevel < MipCount; mipLevel++)
                {
                    mips.Add(GetImageData(arrayLevel, mipLevel));
                }

                surfaces.Add(new Surface() { mipmaps = mips });
            }

            return surfaces;
        }

        public abstract void SetImageData(Bitmap bitmap, int ArrayLevel);

        /// <summary>
        /// The total amount of surfaces for the texture.
        /// </summary>
        public uint ArrayCount
        {
            get { return arrayCount; }
            set { arrayCount = value; }
        }
        private uint arrayCount = 1;

        /// <summary>
        /// The total amount of mipmaps for the texture.
        /// </summary>
        public uint MipCount
        {
            get { return mipCount; }
            set {
                if (value == 0)
                    mipCount = 1;
                else if (value > 14)
                    throw new Exception($"Invalid mip map count! Texture: {Text} Value: {value}");
                else
                    mipCount = value;
            }
        }
        private uint mipCount = 1;

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public uint Height { get; set; }

        /// <summary>
        /// The depth of the image in pixels. Used for 3D types.
        /// </summary>
        public uint Depth { get; set; }

        /// <summary>
        /// The <see cref="TEX_FORMAT"/> Format of the image. 
        /// </summary>
        public TEX_FORMAT Format { get; set; }

        public RenderableTex RenderableTex { get; set; }

        public abstract TEX_FORMAT[] SupportedFormats { get;}

        public static uint GetBytesPerPixel(TEX_FORMAT Format)
        {
            return FormatTable[Format].BytesPerPixel;
        }

        public static uint GetBlockHeight(TEX_FORMAT Format)
        {
            return FormatTable[Format].BlockHeight;
        }

        public static uint GetBlockWidth(TEX_FORMAT Format)
        {
            return FormatTable[Format].BlockWidth;
        }

        public static uint GetBlockDepth(TEX_FORMAT Format)
        {
            return FormatTable[Format].BlockDepth;
        }

        // Based on Ryujinx's image table 
        // https://github.com/Ryujinx/Ryujinx/blob/c86aacde76b5f8e503e2b412385c8491ecc86b3b/Ryujinx.Graphics/Graphics3d/Texture/ImageUtils.cs
        // A nice way to get bpp, block data, and buffer types for formats

        private static readonly Dictionary<TEX_FORMAT, FormatInfo> FormatTable =
                         new Dictionary<TEX_FORMAT, FormatInfo>()
        { 
            { TEX_FORMAT.R32G32B32A32_FLOAT,   new FormatInfo(16, 1,  1, 1, TargetBuffer.Color) },
            { TEX_FORMAT.R32G32B32A32_SINT,    new FormatInfo(16, 1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R32G32B32A32_UINT,    new FormatInfo(16, 1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16G16B16A16_FLOAT,   new FormatInfo(8,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16G16B16A16_SINT,    new FormatInfo(8,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16G16B16A16_SNORM,   new FormatInfo(8,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R32G32_FLOAT,         new FormatInfo(8,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R32G32_SINT,          new FormatInfo(8,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R32G32_UINT,          new FormatInfo(8,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8B8A8_SINT,        new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8B8A8_SNORM,       new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8B8A8_UINT,        new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8B8A8_UNORM,       new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8B8A8_UNORM_SRGB,  new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R32G8X24_FLOAT,       new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            
            { TEX_FORMAT.R10G10B10A2_UINT,      new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R10G10B10A2_UNORM,     new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R32_SINT,              new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R32_UINT,              new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R32_FLOAT,             new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.B4G4R4A4_UNORM,        new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16G16_FLOAT,          new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16G16_SINT,           new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16G16_SNORM,          new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16G16_UINT,           new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16G16_UNORM,          new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8_SINT,             new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8_SNORM,            new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8_UINT,             new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8G8_UNORM,            new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16_SINT,              new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16_SNORM,             new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16_UINT,              new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R16_UNORM,             new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8_SINT,               new FormatInfo(1,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8_SNORM,              new FormatInfo(1,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8_UINT,               new FormatInfo(1,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R8_UNORM,              new FormatInfo(1,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.R11G11B10_FLOAT,       new FormatInfo(4,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.B5G6R5_UNORM,          new FormatInfo(2,  1,  1, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC1_UNORM,             new FormatInfo(8,  4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC1_UNORM_SRGB,        new FormatInfo(8,  4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC2_UNORM,             new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC2_UNORM_SRGB,        new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC3_UNORM,             new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC3_UNORM_SRGB,        new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC4_UNORM,             new FormatInfo(8,  4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC4_SNORM,             new FormatInfo(8,  4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC5_UNORM,             new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC5_SNORM,             new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC6H_SF16,             new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC6H_UF16,             new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC7_UNORM,             new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.BC7_UNORM_SRGB,        new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },

            { TEX_FORMAT.ASTC_4x4_UNORM,        new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_4x4_SRGB,         new FormatInfo(16, 4,  4, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_5x5_UNORM,        new FormatInfo(16, 5,  5, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_6x6_SRGB,         new FormatInfo(16, 6,  6, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_8x8_UNORM,        new FormatInfo(16, 8,  8, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_8x8_SRGB,         new FormatInfo(16, 8,  8, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_10x10_UNORM,      new FormatInfo(16, 10, 10, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_10x10_SRGB,       new FormatInfo(16, 10, 10, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_12x12_UNORM,      new FormatInfo(16, 12, 12, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_12x12_SRGB,       new FormatInfo(16, 12, 12, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_5x4_UNORM,        new FormatInfo(16, 5,  4, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_5x4_SRGB,         new FormatInfo(16, 5,  4, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_6x5_UNORM,        new FormatInfo(16, 6,  5, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_6x5_SRGB,         new FormatInfo(16, 6,  5, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_8x6_UNORM,        new FormatInfo(16, 8,  6, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_8x6_SRGB,         new FormatInfo(16, 8,  6, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_10x8_UNORM,       new FormatInfo(16, 10, 8, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_10x8_SRGB,        new FormatInfo(16, 10, 8, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_12x10_UNORM,      new FormatInfo(16, 12, 10, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_12x10_SRGB,       new FormatInfo(16, 12, 10, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_8x5_UNORM,        new FormatInfo(16, 8,  5,  1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_8x5_SRGB,         new FormatInfo(16, 8,  5, 1,  TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_10x5_UNORM,       new FormatInfo(16, 10, 5, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_10x5_SRGB,        new FormatInfo(16, 10, 5, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_10x6_UNORM,       new FormatInfo(16, 10, 6, 1, TargetBuffer.Color) },
            { TEX_FORMAT.ASTC_10x6_SRGB,        new FormatInfo(16, 10, 6, 1, TargetBuffer.Color) },

            { TEX_FORMAT.D16_UNORM,            new FormatInfo(2, 1, 1, 1, TargetBuffer.Depth)        },
            { TEX_FORMAT.D24_UNORM_S8_UINT,    new FormatInfo(4, 1, 1, 1, TargetBuffer.Depth)        },
            { TEX_FORMAT.D32_FLOAT,            new FormatInfo(4, 1, 1, 1, TargetBuffer.Depth)        },
            { TEX_FORMAT.D32_FLOAT_S8X24_UINT, new FormatInfo(8, 1, 1, 1,TargetBuffer.DepthStencil) }
     }; 

        /// <summary>
        /// A Surface contains mip levels of compressed/uncompressed texture data
        /// </summary>
        public class Surface
        {
            public List<byte[]> mipmaps = new List<byte[]>();
        }

        public void CreateGenericTexture(uint width, uint height, List<Surface> surfaces, TEX_FORMAT format )
        {
            Width = width;
            Height = height;
            Format = format;
        }
        private enum TargetBuffer
        {
            Color = 1,
            Depth = 2,
            Stencil = 3,
            DepthStencil = 4,
        }

        public void DisposeRenderable()
        {
            if (RenderableTex != null)
            {
                RenderableTex.Dispose();
                RenderableTex = null;
            }
        }

        private class FormatInfo
        {
            public uint BytesPerPixel { get; private set; }
            public uint BlockWidth { get; private set; }
            public uint BlockHeight { get; private set; }
            public uint BlockDepth { get; private set; }

            public TargetBuffer TargetBuffer;

            public FormatInfo(uint bytesPerPixel, uint blockWidth, uint blockHeight, uint blockDepth, TargetBuffer targetBuffer)
            {
                BytesPerPixel = bytesPerPixel;
                BlockWidth = blockWidth;
                BlockHeight = blockHeight;
                BlockDepth = blockDepth;
                TargetBuffer = targetBuffer;
            }
        }

        /// <summary>
        /// Gets a <see cref="Bitmap"/> given an array and mip index.
        /// </summary>
        /// <param name="ArrayIndex">The index of the surface/array. Cubemaps will have 6</param>
        /// <param name="MipLevel">The index of the mip level.</param>
        /// <returns></returns>
        public Bitmap GetBitmap(int ArrayLevel = 0, int MipLevel = 0)
        {
            uint width = Math.Max(1, Width >> MipLevel);
            uint height = Math.Max(1, Height >> MipLevel);
            byte[] data = GetImageData(ArrayLevel, MipLevel);

            try
            {
                if (data == null)
                    throw new Exception("Data is null!");

                if (Format == TEX_FORMAT.BC5_SNORM)
                    return DDSCompressor.DecompressBC5(data, (int)width, (int)height, true);

                Bitmap bitmap = BitmapExtension.GetBitmap(DecodeBlock(data, width, height, Format),
                   (int)width, (int)height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                return bitmap;
            }
            catch (Exception ex)
            {
                Forms.STErrorDialog.Show($"Texture failed to load!", "Texture [GetBitmap({MipLevel},{ArrayLevel})]", DebugInfo() + " \n" + ex);

                try
                {
                    if (Format == TEX_FORMAT.BC1_UNORM)
                        return DDSCompressor.DecompressBC1(data, (int)Width, (int)Height, false);
                    else if (Format == TEX_FORMAT.BC1_UNORM_SRGB)
                        return DDSCompressor.DecompressBC1(data, (int)Width, (int)Height, true);
                    else if (Format == TEX_FORMAT.BC3_UNORM_SRGB)
                        return DDSCompressor.DecompressBC3(data, (int)Width, (int)Height, false);
                    else if (Format == TEX_FORMAT.BC3_UNORM)
                        return DDSCompressor.DecompressBC3(data, (int)Width, (int)Height, true);
                    else if (Format == TEX_FORMAT.BC4_UNORM)
                        return DDSCompressor.DecompressBC4(data, (int)Width, (int)Height, false);
                    else if (Format == TEX_FORMAT.BC4_SNORM)
                        return DDSCompressor.DecompressBC4(data, (int)Width, (int)Height, true);
                    else if (Format == TEX_FORMAT.BC5_UNORM)
                        return DDSCompressor.DecompressBC5(data, (int)Width, (int)Height, false);
                    else
                    {
                        Runtime.OpenTKInitialized = true;
                        LoadOpenGLTexture();
                        return RenderableTex.GLTextureToBitmap(RenderableTex);
                    }
                }
                catch
                {
                    Forms.STErrorDialog.Show($"Texture failed to load!", "Texture [GetBitmap({MipLevel},{ArrayLevel})]", DebugInfo() + " \n" + ex);
                }

                return null;
            }



            /*       try
                   {
                       Bitmap bitmap = BitmapExtension.GetBitmap(DecodeBlock(data, width, height, Format),
                           (int)width, (int)height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                       return bitmap;
                   }
                   catch (Exception ex)
                   {
                      MessageBox.Show($"Failed to texture {Text} \n{DebugInfo()}\n {ex.ToString()}");
                       return null;
                   }*/

        }
        public static Bitmap DecodeBlockGetBitmap(byte[] data, uint Width, uint Height, TEX_FORMAT Format)
        {
            Bitmap bitmap = BitmapExtension.GetBitmap(DecodeBlock(data, Width, Height, Format),
               (int)Width, (int)Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            return bitmap;
        }

        /// <summary>
        /// Decodes a byte array of image data given the source image in bytes, width, height, and DXGI format.
        /// </summary>
        /// <param name="byte[]">The byte array of the image</param>
        /// <param name="Width">The width of the image in pixels.</param>
        /// <param name="Height">The height of the image in pixels.</param>
        /// <param name=" DDS.DXGI_FORMAT">The image format.</param>
        /// <returns>Returns a byte array of decoded data. </returns>
        public static byte[] DecodeBlock(byte[] data, uint Width, uint Height, TEX_FORMAT Format)
        {
            if (data == null)     throw new Exception($"Data is null!");
            if (Format <= 0)      throw new Exception($"Invalid Format!");
            if (data.Length <= 0) throw new Exception($"Data is empty!");
            if (Width <= 0)       throw new Exception($"Invalid width size {Width}!");
            if (Height <= 0)      throw new Exception($"Invalid height size {Height}!");

            if (Format == TEX_FORMAT.R32G8X24_FLOAT)
                return ConvertBgraToRgba(DDSCompressor.DecodePixelBlock(data, (int)Width, (int)Height, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G8X24_TYPELESS));


            if (Format == TEX_FORMAT.BC5_SNORM)
                return ConvertBgraToRgba(DDSCompressor.DecompressBC5(data, (int)Width, (int)Height, true, true));

            if (IsCompressed(Format))
                return ConvertBgraToRgba(DDSCompressor.DecompressBlock(data, (int)Width, (int)Height, (DDS.DXGI_FORMAT)Format));
            else
            {
                //If blue channel becomes first, do not swap them!
                if (Format.ToString().StartsWith("B") || Format == TEX_FORMAT.B5G6R5_UNORM)
                    return DDSCompressor.DecodePixelBlock(data, (int)Width, (int)Height, (DDS.DXGI_FORMAT)Format);
                else if (IsAtscFormat(Format))
                    return ConvertBgraToRgba(ASTCDecoder.DecodeToRGBA8888(data, (int)GetBlockWidth(Format), (int)GetBlockHeight(Format), 1, (int)Width, (int)Height, 1));
                else
                    return ConvertBgraToRgba(DDSCompressor.DecodePixelBlock(data, (int)Width, (int)Height, (DDS.DXGI_FORMAT)Format));
            }
        }

        public string DebugInfo()
        {
            return $"Texture Info:\n" +
                   $"Name:               {Text}\n"  +
                   $"Format:             {Format}\n" +
                   $"Height:             {Height}\n" +
                   $"Width:              {Width}\n" +
                   $"Block Height:       {GetBlockHeight(Format)}\n" +
                   $"Block Width:        {GetBlockWidth(Format)}\n" +
                   $"Bytes Per Pixel:    {GetBytesPerPixel(Format)}\n" +
                   $"Array Count:        {ArrayCount}\n" +
                   $"Mip Map Count:      {MipCount}\n" +
                    "";
        }

        public uint GenerateMipCount(int Width, int Height)
        {
           return GenerateMipCount((uint)Width, (uint)Height);
        }

        public uint GenerateMipCount(uint Width, uint Height)
        {
            uint MipmapNum = 0;
            uint num = Math.Max(Width, Height);

            int width = (int)Width;
            int height = (int)Height;

            while (true)
            {
                num >>= 1;

                width = width / 2;
                height = height / 2;
                if (width <= 0 || height <= 0)
                    break;

                if (num > 0)
                    ++MipmapNum;
                else
                    break;
            }

            return MipmapNum;
        }

        public byte[] GenerateMipsAndCompress(Bitmap bitmap, TEX_FORMAT Format, float alphaRef = 0.5f)
        {
            byte[] DecompressedData = BitmapExtension.ImageToByte(bitmap);
            DecompressedData = ConvertBgraToRgba(DecompressedData);

            Bitmap Image = BitmapExtension.GetBitmap(DecompressedData, bitmap.Width, bitmap.Height);

            List<byte[]> mipmaps = new List<byte[]>();
            mipmaps.Add(STGenericTexture.CompressBlock(DecompressedData,
                 bitmap.Width, bitmap.Height, Format, alphaRef));

            for (int mipLevel = 0; mipLevel < MipCount; mipLevel++)
            {
                if (Image.Width / 2 > 0 && Image.Height / 2 > 0)
                {
                    Image = BitmapExtension.Resize(Image, Image.Width / 2, Image.Height / 2);
                    mipmaps.Add(STGenericTexture.CompressBlock(BitmapExtension.ImageToByte(Image),
                        Image.Width, Image.Height, Format, alphaRef));
                }
            }
            Image.Dispose();

            return Utils.CombineByteArray(mipmaps.ToArray());
        }

        public static byte[] CompressBlock(byte[] data, int width, int height, TEX_FORMAT format, float alphaRef)
        {
            if (IsCompressed(format))
                return DDSCompressor.CompressBlock(data, width, height, (DDS.DXGI_FORMAT)format, alphaRef);
            else if (IsAtscFormat(format))
                return null;
            else
                return DDSCompressor.EncodePixelBlock(data, width, height, (DDS.DXGI_FORMAT)format);
        }
        public void LoadDDS(string path)
        {
            SetNameFromPath(path);

            DDS dds = new DDS();
            LoadDDS(path);

            Width = dds.header.width;
            Height = dds.header.height;
            Format = dds.GetFormat();

            MipCount = dds.header.mipmapCount;
        }
        public void LoadTGA(string path)
        {
            SetNameFromPath(path);
            Bitmap tga = Paloma.TargaImage.LoadTargaImage(path);
        }
        public void LoadBitmap(string path)
        {
            SetNameFromPath(path);

        }
        public void LoadASTC(string path)
        {
            ASTC astc = new ASTC();
            astc.Load(new FileStream(path, FileMode.Open));
        }

        public void ExportImage()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Text;
            sfd.DefaultExt = "bftex";
            sfd.Filter = "Supported Formats|*.dds; *.png;*.tga;*.jpg;*.tiff|" +
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
            string ext = Path.GetExtension(FileName);
            ext = ext.ToLower();

            switch (ext)
            {
                case ".dds":
                    SaveDDS(FileName);
                    break;
                case ".astc":
                    SaveASTC(FileName);
                    break;
                default:
                    SaveBitMap(FileName);
                    break;
            }
        }
        public void SaveASTC(string FileName, int SurfaceLevel = 0, int MipLevel = 0)
        {
            ASTC atsc = new ASTC();
            atsc.BlockDimX = (byte)GetBlockHeight(Format);
            atsc.BlockDimY = (byte)GetBlockWidth(Format);
            atsc.BlockDimZ = (byte)1;
        }
        public void SaveTGA(string FileName, int SurfaceLevel = 0, int MipLevel = 0)
        {
           
        }
        public void SaveBitMap(string FileName, int SurfaceLevel = 0, int MipLevel = 0)
        {
            Bitmap bitMap = GetBitmap(MipLevel, SurfaceLevel);
            bitMap.Save(FileName);
            bitMap.Dispose();
        }
        public void SaveDDS(string FileName, int SurfaceLevel = 0, int MipLevel = 0)
        {
            var data = GetImageData(SurfaceLevel, MipLevel);
            var surfaces = new List<Surface>();

            DDS dds = new DDS();
            dds.header = new DDS.Header();
            dds.header.width = Width;
            dds.header.height = Height;
            dds.header.mipmapCount = (uint)MipCount;
            dds.header.pitchOrLinearSize = (uint)data.Length;
            dds.SetFlags((DDS.DXGI_FORMAT)Format);

            dds.Save(dds, FileName, GetSurfaces());
        }
        public void LoadOpenGLTexture()
        {  
            if (RenderableTex == null)
                RenderableTex = new RenderableTex();

            RenderableTex.GLInitialized = false;
            RenderableTex.LoadOpenGLTexture(this);
        }
        public static bool IsAtscFormat(TEX_FORMAT Format)
        {
            if (Format.ToString().Contains("ASTC"))
                return true;
            else
                return false;
        }

        public static bool IsCompressed(TEX_FORMAT Format)
        {
            switch (Format)
            {
                case TEX_FORMAT.BC1_UNORM:
                case TEX_FORMAT.BC1_UNORM_SRGB:
                case TEX_FORMAT.BC1_TYPELESS:
                case TEX_FORMAT.BC2_UNORM_SRGB:
                case TEX_FORMAT.BC2_UNORM:
                case TEX_FORMAT.BC2_TYPELESS:
                case TEX_FORMAT.BC3_UNORM_SRGB:
                case TEX_FORMAT.BC3_UNORM:
                case TEX_FORMAT.BC3_TYPELESS:
                case TEX_FORMAT.BC4_UNORM:
                case TEX_FORMAT.BC4_TYPELESS:
                case TEX_FORMAT.BC4_SNORM:
                case TEX_FORMAT.BC5_UNORM:
                case TEX_FORMAT.BC5_TYPELESS:
                case TEX_FORMAT.BC5_SNORM:
                case TEX_FORMAT.BC6H_UF16:
                case TEX_FORMAT.BC6H_SF16:
                case TEX_FORMAT.BC7_UNORM:
                case TEX_FORMAT.BC7_UNORM_SRGB:
                    return true;
                default:
                    return false;
            }
        }
        public static STChannelType[] SetChannelsByFormat(TEX_FORMAT Format)
        {
            STChannelType[] channels = new STChannelType[4];

            switch (Format)
            {
                case TEX_FORMAT.BC5_UNORM:
                case TEX_FORMAT.BC5_SNORM:
                    channels[0] = STChannelType.Red;
                    channels[1] = STChannelType.Green;
                    channels[2] = STChannelType.Zero;
                    channels[3] = STChannelType.One;
                    break;
                case TEX_FORMAT.BC4_UNORM:
                case TEX_FORMAT.BC4_SNORM:
                    channels[0] = STChannelType.Red;
                    channels[1] = STChannelType.Red;
                    channels[2] = STChannelType.Red;
                    channels[3] = STChannelType.Red;
                    break;
                default:
                    channels[0] = STChannelType.Red;
                    channels[1] = STChannelType.Green;
                    channels[2] = STChannelType.Blue;
                    channels[3] = STChannelType.Alpha;
                    break;
            }
            return channels;
        }

   
        private void SetNameFromPath(string path)
        {
            //Replace extensions manually. This is because using the
            //GetFileNameWithoutExtension function can remove .0, .1, texture names.
            var name = Path.GetFileName(path);
            name.Replace(".tga",  string.Empty);
            name.Replace(".png",  string.Empty);
            name.Replace(".jpg",  string.Empty);
            name.Replace(".dds",  string.Empty);
            name.Replace(".jpeg", string.Empty);
            name.Replace(".tiff", string.Empty);
            name.Replace(".gif",  string.Empty);
            name.Replace(".dds2", string.Empty);
            name.Replace(".jpe",  string.Empty);
            name.Replace(".jfif", string.Empty);
            name.Replace(".bmp",  string.Empty);
            name.Replace(".pdn",  string.Empty);
            name.Replace(".psd",  string.Empty);
            name.Replace(".hdr",  string.Empty);

            Text = name;
        }
        private static byte[] ConvertBgraToRgba(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var temp = bytes[i];
                bytes[i] = bytes[i + 2];
                bytes[i + 2] = temp;
            }
            return bytes;
        }


        public Properties GenericProperties
        {
            get
            {
                Properties prop = new Properties();
                prop.Height = Height;
                prop.Width = Width;
                prop.Format = Format;
                prop.Depth = Depth;
                prop.MipCount = MipCount;
                prop.ArrayCount = ArrayCount;
                prop.ImageSize = (uint)GetImageData().Length;

                return prop;
            }
        }

        public class Properties
        {
            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Height of the image.")]
            [Category("Image Info")]
            public uint Height { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Width of the image.")]
            [Category("Image Info")]
            public uint Width { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Format of the image.")]
            [Category("Image Info")]
            public TEX_FORMAT Format { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Depth of the image (3D type).")]
            [Category("Image Info")]
            public uint Depth { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Mip map count of the image.")]
            [Category("Image Info")]
            public uint MipCount { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Array count of the image for multiple surfaces.")]
            [Category("Image Info")]
            public uint ArrayCount { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("The image size in bytes.")]
            [Category("Image Info")]
            public uint ImageSize { get; set; }
        }
    }
}
