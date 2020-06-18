namespace NetIpc.Common.Interfaces
{
    public interface IIpcResponse :  IIpcDataPack
    {
        bool Success { get; }
    }
}