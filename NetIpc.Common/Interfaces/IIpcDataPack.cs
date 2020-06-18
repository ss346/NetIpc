namespace NetIpc.Common.Interfaces
{
    public interface IIpcDataPack
    {
        T GetData<T>();
        void SetData<T>(T data);
    }
}