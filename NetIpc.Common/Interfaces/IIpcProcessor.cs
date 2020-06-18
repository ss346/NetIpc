using System;

namespace NetIpc.Common.Interfaces
{
    public interface IIpcProcessor
    {
        void RegisterMessageHandler(string command, Func<IpcMessage, IpcResponse> messageHandler);
        string ProcessMessage(string messageJson);
    }
}