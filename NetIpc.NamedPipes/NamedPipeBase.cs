using System;
using System.IO;
using System.Linq;
using System.Text;
using NetIpc.Common;

namespace NetIpc.NamedPipes
{
    public class NamedPipeBase
    {

        #region Constants
        protected const int MaxChunkDataSize = 65530;
        protected static readonly byte[] BundleHeaderSignature = { 1, 2, 3, 4 };
        protected static readonly Encoding StreamEncoding = Encoding.UTF8;
        #endregion Constants

        #region Private data
        protected string PipeName;
        #endregion Private data

        #region Private methods
        protected static void WriteString(string outString, Stream stream)
        {
            var data = StreamEncoding.GetBytes(outString);
            var dataLength = data.Length;
            var chunkCount = dataLength / MaxChunkDataSize + (dataLength % MaxChunkDataSize > 0 ? 1 : 0);
            WriteBundleHeader(stream, dataLength, chunkCount);
            var chunkOffset = 0;
            while (chunkOffset < dataLength)
            {
                chunkOffset = WriteChunk(stream, data, dataLength, chunkOffset);
            }
        }
        protected static int WriteChunk(Stream stream, byte[] data, int dataLength, int chunkOffset)
        {
            var chunkLength = Math.Min(dataLength - chunkOffset, MaxChunkDataSize);
            WriteInt16(stream, chunkLength);
            stream.Write(data, chunkOffset, chunkLength);
            stream.Flush();
            return chunkOffset + chunkLength;
        }
        protected static string ReadString(Stream stream)
        {
            var bundleParams = ReadBundleHeader(stream);
            if (!bundleParams.Success) throw new Exception("Invalid header signature");
            var buffer = new byte[bundleParams.DataLength];
            var chunkOffset = 0;
            for (var i = 0; i < bundleParams.ChunkCount; i++)
            {
                chunkOffset = ReadChunk(stream, buffer, chunkOffset);
            }
            return StreamEncoding.GetString(buffer);
        }
        protected static void WriteInt16(Stream stream, int value)
        {
            if (value < 0) throw new ArgumentException("Argument must be positive");
            stream.WriteByte((byte)(value / 256));
            stream.WriteByte((byte)(value & 255));
        }
        protected static void WriteInt32(Stream stream, int value)
        {
            if (value < 0) throw new ArgumentException("Argument must be positive");
            stream.WriteByte((byte)(value / 16777216));
            stream.WriteByte((byte)(value / 65536));
            stream.WriteByte((byte)(value / 256));
            stream.WriteByte((byte)(value & 255));
        }
        protected static int ReadInt16(Stream stream)
        {
            var value = stream.ReadByte() * 256;
            value += stream.ReadByte();
            return value;
        }
        protected static int ReadInt32(Stream stream)
        {
            var value = (uint)stream.ReadByte() * 16777216;
            value += (uint)stream.ReadByte() * 65536;
            value += (uint)stream.ReadByte() * 256;
            value += (uint)stream.ReadByte();
            return (int)value;
        }

        private static void WriteBundleHeader(Stream stream, int dataLength, int chunkCount)
        {
            var headerLength = BundleHeaderSignature.Length + 8;
            WriteInt16(stream, headerLength);                                           //size of header
            stream.Write(BundleHeaderSignature, 0, BundleHeaderSignature.Length);       //signature
            WriteInt32(stream, dataLength);                                             //data size
            WriteInt32(stream, chunkCount);                                             //chunk count
            stream.Flush();
        }
        private static BundleHeaderParams ReadBundleHeader(Stream stream)
        {
            var size = ReadInt16(stream);
            var signature = new byte[BundleHeaderSignature.Length];
            stream.Read(signature, 0, BundleHeaderSignature.Length);
            if (BundleHeaderSignature.Where((v, i) => signature[i] != v).Any())
            {
                var restSize = size - BundleHeaderSignature.Length;
                var buffer = new byte[restSize];
                stream.Read(buffer, 0, restSize);
                return new BundleHeaderParams();
            }
            var dataLength = ReadInt32(stream);
            var chunkCount = ReadInt32(stream);
            return new BundleHeaderParams(dataLength, chunkCount);
        }
        private static int ReadChunk(Stream stream, byte[] buffer, int chunkOffset)
        {
            var size = ReadInt16(stream);
            stream.Read(buffer, chunkOffset, size);
            return chunkOffset + size;
        }
        #endregion Private methods

    }
}