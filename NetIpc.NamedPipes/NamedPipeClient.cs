using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using NetIpc.Common;
using NetIpc.Common.Interfaces;

namespace NetIpc.NamedPipes
{
    public class NamedPipeClient : NamedPipeBase, IIpcClient
    {

        #region Constants
        private const string LocalServerName = ".";
        protected const int TimeOut = 2000;
        #endregion Constants

        #region Constructors
        public NamedPipeClient(string pipeName)
        {
            PipeName = pipeName;
        }
        #endregion Constructors

        #region Public properties
        public string Name => $"named pipe: {PipeName}";
        #endregion Public properties

        #region Public methods
        public async Task<string> SendAsync(string json)
        {
            using (var stream = new NamedPipeClientStream(LocalServerName, PipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                stream.Connect(TimeOut);
                await WriteStringAsync(json, stream);
                return await ReadStringAsync(stream);
            }
        }
        public string Send(string json)
        {
            using (var stream = new NamedPipeClientStream(LocalServerName, PipeName, PipeDirection.InOut))
            {
                stream.Connect(TimeOut);
                WriteString(json, stream);
                return ReadString(stream);
            }
        }
        #endregion Public methods

        #region Private methods
        private static async Task WriteStringAsync(string outString, Stream stream)
        {
            var data = StreamEncoding.GetBytes(outString);
            var dataLength = data.Length;
            var chunkCount = dataLength / MaxChunkDataSize + (dataLength % MaxChunkDataSize > 0 ? 1 : 0);
            await WriteBundleHeaderAsync(stream, dataLength, chunkCount);
            var chunkOffset = 0;
            while (chunkOffset < dataLength)
            {
                chunkOffset = await WriteChunkAsync(stream, data, dataLength, chunkOffset);
            }
        }
        private static async Task WriteBundleHeaderAsync(Stream stream, int dataLength, int chunkCount)
        {
            var headerLength = BundleHeaderSignature.Length + 8;
            WriteInt16(stream, headerLength);                                                   //size of header
            await stream.WriteAsync(BundleHeaderSignature, 0, BundleHeaderSignature.Length);    //signature
            WriteInt32(stream, dataLength);                                                     //data size
            WriteInt32(stream, chunkCount);                                                     //chunk count
            await stream.FlushAsync();
        }
        private static async Task<int> WriteChunkAsync(Stream stream, byte[] data, int dataLength, int chunkOffset)
        {
            var chunkLength = Math.Min(dataLength - chunkOffset, MaxChunkDataSize);
            WriteInt16(stream, chunkLength);
            await stream.WriteAsync(data, chunkOffset, chunkLength);
            await stream.FlushAsync();
            return chunkOffset + chunkLength;
        }
        protected static async Task<string> ReadStringAsync(Stream stream)
        {
            var bundleParams = await ReadBundleHeaderAsync(stream);
            if (!bundleParams.Success) throw new Exception("Invalid header signature");
            var buffer = new byte[bundleParams.DataLength];
            var chunkOffset = 0;
            for (var i = 0; i < bundleParams.ChunkCount; i++)
            {
                chunkOffset = await ReadChunkAsync(stream, buffer, chunkOffset);
            }
            return StreamEncoding.GetString(buffer);
        }
        private static async Task<BundleHeaderParams> ReadBundleHeaderAsync(Stream stream)
        {
            var size = ReadInt16(stream);
            var signature = new byte[BundleHeaderSignature.Length];
            await stream.ReadAsync(signature, 0, BundleHeaderSignature.Length);
            if (BundleHeaderSignature.Where((v, i) => signature[i] != v).Any())
            {
                var restSize = size - BundleHeaderSignature.Length;
                var buffer = new byte[restSize];
                await stream.ReadAsync(buffer, 0, restSize);
                return new BundleHeaderParams();
            }
            var dataLength = ReadInt32(stream);
            var chunkCount = ReadInt32(stream);
            return new BundleHeaderParams(dataLength, chunkCount);
        }
        private static async Task<int> ReadChunkAsync(Stream stream, byte[] buffer, int chunkOffset)
        {
            var size = ReadInt16(stream);
            await stream.ReadAsync(buffer, chunkOffset, size);
            return chunkOffset + size;
        }
        #endregion Private methods

    }
}
