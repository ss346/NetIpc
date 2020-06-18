using NetIpc.Common.Interfaces;

namespace NetIpc.Common
{
    public class IpcMessage : IpcDataPack, IIpcMessage
    {

        public IpcMessage(string command)
        {
            Command = command;
        }

        public string Command { get; }

    }
}
