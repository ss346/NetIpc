using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using NetIpc.Common;
using Newtonsoft.Json;

namespace NetIpc.NamedPipes
{
    public class NamedPipeServerStreamWrapper : NamedPipeBase
    {

        #region Private data
        private readonly object _locker = new object();
        private readonly NamedPipeServerStream _serverStream;
        private bool _isStopping;
        private Func<string, string> _processMessageFunc;
        #endregion

        #region Constructors
        public NamedPipeServerStreamWrapper(string pipeName, int maxNumberOfServerInstances)
        {
            PipeName = pipeName;
            ID = Guid.NewGuid().ToString();
            var ps = GetPipeSecurity();
            _serverStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, maxNumberOfServerInstances,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1, 1, ps);
        }
        #endregion

        #region Events
        public event EventHandler<Exception> ExceptionThrown;
        public event EventHandler<string> ClientConnected;
        public event EventHandler<string> ClientDisconnected;
        #endregion Events

        #region Public properties
        public string ID { get; }
        #endregion Public properties

        #region Public methods
        public void SetProcessFunction(Func<string, string> processMessageFunc)
        {
            _processMessageFunc = processMessageFunc;
        }
        public void StartListen()
        {
            try
            {
                _serverStream.BeginWaitForConnection(WaitForConnectionCallBack, null);
            }
            catch (Exception ex)
            {
                ExceptionThrown?.Invoke(this, ex);
            }
        }
        public void StopListen()
        {
            _isStopping = true;

            try
            {
                if (_serverStream.IsConnected) _serverStream.Disconnect();
            }
            catch (Exception ex)
            {
                ExceptionThrown?.Invoke(this, ex);
            }
            finally
            {
                _serverStream.Close();
                _serverStream.Dispose();
            }
        }
        #endregion

        #region Private methods
        private PipeSecurity GetPipeSecurity()
        {
            PipeSecurity ps = null;
            try
            {
                ps = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null); //LocalSystemSid
                //ps.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.ReadWrite, AccessControlType.Allow););

                //ps.AddAccessRule(new PipeAccessRule(myPipeUsersGroup, PipeAccessRights.ReadWrite, AccessControlType.Allow));
                ps.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow));
            }
            catch (Exception ex)
            {
                ExceptionThrown?.Invoke(PipeName, ex);
            }
            return ps;
        }
        private void WaitForConnectionCallBack(IAsyncResult result)
        {
            if (_isStopping) return;
            lock (_locker)
            {
                if (_isStopping) return;
                _serverStream.EndWaitForConnection(result);
                ClientConnected?.Invoke(this, ID);
                var response = GetResponse();
                SendResponse(response);
                ClientDisconnected?.Invoke(this, ID);
            }
        }
        private string GetResponse()
        {
            string responseJson;
            try
            {
                var message = ReadString(_serverStream);
                if (string.IsNullOrEmpty(message))
                {
                    var ex = new Exception("Message is empty");
                    responseJson = GetErrorResponse(PipeName, ex);
                    ExceptionThrown?.Invoke(PipeName, ex);
                }
                else
                    responseJson = _processMessageFunc.Invoke(message);
            }
            catch (Exception ex)
            {
                ExceptionThrown?.Invoke(PipeName, ex);
                responseJson = GetErrorResponse(PipeName, ex);
            }

            return responseJson;
        }
        private void SendResponse(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                WriteString(json, _serverStream);
            }
            catch (Exception ex)
            {
                ExceptionThrown?.Invoke(PipeName, ex);
            }
        }
        private static string GetErrorResponse(string pipeName, Exception ex)
        {
            var response = new IpcResponse(false);
            response.SetData($"{pipeName} named pipe: command handling error. {ex.Message}");
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
