using System;
using System.Collections.Generic;
using NetIpc.Common.Interfaces;
using Newtonsoft.Json;

namespace NetIpc.Common
{
    public class IpcProcessor : IIpcProcessor
    {

        #region Private data
        private readonly Dictionary<string, Func<IpcMessage, IpcResponse>> _messageHandlers;
        #endregion Private data

        #region Constructors
        public IpcProcessor()
        {
            _messageHandlers = new Dictionary<string, Func<IpcMessage, IpcResponse>>();
        }
        #endregion Constructors

        #region Public methods
        public void RegisterMessageHandler(string command, Func<IpcMessage, IpcResponse> messageHandler)
        {
            _messageHandlers.Add(command, messageHandler);
        }
        public string ProcessMessage(string messageJson)
        {
            var message = JsonConvert.DeserializeObject<IpcMessage>(messageJson);

            string responseJson;
            if (_messageHandlers.ContainsKey(message.Command))
            {
                var handler = _messageHandlers[message.Command];
                var response = handler.Invoke(message);
                responseJson = JsonConvert.SerializeObject(response);
            }
            else
                responseJson = CmdNotRegisteredResponse(message.Command);

            return responseJson;
        }
        #endregion Public methods

        #region Private methods
        private static string CmdNotRegisteredResponse(string command)
        {
            var response = new IpcResponse(false);
            response.SetData($"Command \"{command}\" is not registered");
            return JsonConvert.SerializeObject(response);
        }
        #endregion Private methods

    }
}
