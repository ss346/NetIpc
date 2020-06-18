namespace NetIpc.Common
{
    public class BundleHeaderParams
    {
        public BundleHeaderParams()
        {
            Success = false;
            DataLength = 0;
            ChunkCount = 0;
        }
        public BundleHeaderParams(int dataLength, int chunkCount)
        {
            Success = true;
            DataLength = dataLength;
            ChunkCount = chunkCount;
        }

        public int DataLength { get; }
        public int ChunkCount { get; }
        public bool Success { get; }
    }
}
