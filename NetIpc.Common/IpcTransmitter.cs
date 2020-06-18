using System.Threading.Tasks;
using NetIpc.Common.Interfaces;
using Newtonsoft.Json;

namespace NetIpc.Common
{
    public class IpcTransmitter : IIpcTransmitter
    {

        #region Private data
        private readonly IIpcClient _ipcClient;
        #endregion Private data

        #region Constructors
        public IpcTransmitter(IIpcClient ipcClient)
        {
            _ipcClient = ipcClient;
        }
        #endregion Constructors

        #region Public methods
        public IIpcResponse SendMessage(string command)
        {
            var message = new IpcMessage(command);
            var messageJson = JsonConvert.SerializeObject(message);
            var responseJson = _ipcClient.Send(messageJson);
            return JsonConvert.DeserializeObject<IpcResponse>(responseJson);
        }
        public async Task<IIpcResponse> SendMessageAsync(string command)
        {
            var message = new IpcMessage(command);
            var messageJson = JsonConvert.SerializeObject(message);
            var responseJson = await _ipcClient.SendAsync(messageJson);
            return JsonConvert.DeserializeObject<IpcResponse>(responseJson);
        }
        public IIpcResponse SendMessage<TIn>(string command, TIn data)
        {
            var message = new IpcMessage(command);
            message.SetData(data);
            var json = JsonConvert.SerializeObject(message);
            var responseJson = _ipcClient.Send(json);
            return JsonConvert.DeserializeObject<IpcResponse>(responseJson);
        }
        public async Task<IIpcResponse> SendMessageAsync<TIn>(string command, TIn data)
        {
            var message = new IpcMessage(command);
            message.SetData(data);
            var json = JsonConvert.SerializeObject(message);
            var responseJson = await _ipcClient.SendAsync(json);
            return JsonConvert.DeserializeObject<IpcResponse>(responseJson);
        }
        public string ProcessResponse(IIpcResponse response)
        {
            if (response.Success) return null;
            var errorMessage = response.GetData<string>();
            return errorMessage;
        }
        #endregion Public methods

    }
}
