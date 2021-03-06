﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.IO;
using System.IO.Compression;
using OpenTK;

namespace Switch_Toolbox.Library.IO
{
    public class FileReader : BinaryDataReader
    {
        public FileReader(Stream stream, bool leaveOpen = false)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            this.Position = 0;
        }

        public FileReader(string fileName)
             : this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            this.Position = 0;
        }
        public FileReader(byte[] data)
             : this(new MemoryStream(data))
        {
            this.Position = 0;
        }

        //Checks signature (no stream advancement)
        public bool CheckSignature(int length, string Identifier, long position = 0)
        {
            if (Position + length >= BaseStream.Length || position < 0)
                return false;

            Position = position;
            string signature = ReadString(length, Encoding.ASCII);

            //Reset position
            Position = 0;

            return signature == Identifier;
        }

        /// <summary>
        /// Checks the byte order mark to determine the endianness of the reader.
        /// </summary>
        /// <param name="ByteOrderMark">The byte order value being read. 0xFFFE = Little, 0xFEFF = Big. </param>
        /// <returns></returns>
        public void CheckByteOrderMark(uint ByteOrderMark)
        {
            if (ByteOrderMark == 0xFEFF)
                ByteOrder = ByteOrder.BigEndian;
            else
                ByteOrder = ByteOrder.LittleEndian;
        }
        public string ReadSignature(int length, string ExpectedSignature)
        {
            string RealSignature = ReadString(length, Encoding.ASCII);

            if (RealSignature != ExpectedSignature)
                throw new Exception($"Invalid signature {RealSignature}! Expected {ExpectedSignature}.");

            return RealSignature;
        }
        public string LoadString(int StringReadSize = 2, DataType OffsetType = DataType.uint64, Encoding encoding = null)
        {
            long offset = 0;
            int size = 0;

            switch (OffsetType)
            {
                case DataType.int64:
                    offset = ReadInt64();
                    break;
                case DataType.int32:
                    offset = ReadInt32();
                    break;
                case DataType.uint64:
                    offset = (long)ReadUInt64();
                    break;
                case DataType.uint32:
                    offset = ReadUInt32();
                    break;
            }

            if (offset == 0) return null;

            encoding = encoding ?? Encoding;
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                if (StringReadSize == 2)
                    size = ReadInt16();
                if (StringReadSize == 4)
                    size = ReadInt32();

                return ReadString(BinaryStringFormat.ZeroTerminated, encoding);
            }
        }
        public static byte[] DeflateZLIB(byte[] i)
        {
            MemoryStream output = new MemoryStream();
            output.WriteByte(0x78);
            output.WriteByte(0x9C);
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(i, 0, i.Length);
            }
            return output.ToArray();
        }
        public byte[] getSection(int offset, int size)
        {
            Seek(offset, SeekOrigin.Begin);
            return ReadBytes(size);
        }
        public Vector4 ReadVec4()
        {
            return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }
        public Vector3 ReadVec3()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }
        public Syroot.Maths.Vector3F ReadVec3SY()
        {
            return new Syroot.Maths.Vector3F(ReadSingle(), ReadSingle(), ReadSingle());
        }
        public Vector2 ReadVec2()
        {
            return new Vector2(ReadSingle(), ReadSingle());
        }
        public Syroot.Maths.Vector2F ReadVec2SY()
        {
            return new Syroot.Maths.Vector2F(ReadSingle(), ReadSingle());
        }
        public static byte[] InflateZLIB(byte[] i)
        {
            var stream = new MemoryStream();
            var ms = new MemoryStream(i);
            ms.ReadByte();
            ms.ReadByte();
            var zlibStream = new DeflateStream(ms, CompressionMode.Decompress);
            byte[] buffer = new byte[4095];
            while (true)
            {
                int size = zlibStream.Read(buffer, 0, buffer.Length);
                if (size > 0)
                    stream.Write(buffer, 0, buffer.Length);
                else
                    break;
            }
            zlibStream.Close();
            return stream.ToArray();
        }
        public string ReadMagic(int Offset, int Length)
        {
            Seek(Offset, SeekOrigin.Begin);
            return ReadString(Length);
        }
    }
}
