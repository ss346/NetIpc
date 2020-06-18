namespace NetIpc.Common.Interfaces
{
    public interface IIpcMessage : IIpcDataPack
    {
        string Command { get; }
    }
}