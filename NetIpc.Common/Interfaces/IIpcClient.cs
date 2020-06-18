using System.Threading.Tasks;

namespace NetIpc.Common.Interfaces
{
    public interface IIpcClient
    {
        Task<string> SendAsync(string json);
        string Send(string json);

        string Name { get; }
    }
}