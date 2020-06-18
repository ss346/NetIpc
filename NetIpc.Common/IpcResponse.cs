using NetIpc.Common.Interfaces;

namespace NetIpc.Common
{
    public class IpcResponse : IpcDataPack, IIpcResponse
    {

        public IpcResponse(bool success)
        {
            Success = success;
        }

        public bool Success { get; }

    }
}
