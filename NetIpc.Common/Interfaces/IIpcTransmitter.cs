using System.Threading.Tasks;

namespace NetIpc.Common.Interfaces
{
    public interface IIpcTransmitter
    {
        IIpcResponse SendMessage(string command);
        Task<IIpcResponse> SendMessageAsync(string command);
        IIpcResponse SendMessage<TIn>(string command, TIn data);
        Task<IIpcResponse> SendMessageAsync<TIn>(string command, TIn data);
        string ProcessResponse(IIpcResponse response);
    }
}