using System;
using NetIpc.Common;
using NetIpc.Common.Interfaces;
using NetIpc.Sample.NamedPipeCommon;

namespace NetIpc.Sample.NamedPipeServer
{
    public class ServiceIpcEngine : IpcEngineBase
    {

        #region Constructors
        public ServiceIpcEngine(IIpcListener ipcListener) : base(ipcListener, "Service") { }
        #endregion Constructors

        #region Events
        public event EventHandler PingCmdReceived;
        public event EventHandler<ActionCmdParams> ActionCmdReceived;
        public event EventHandler<FuncCmdParams> FuncCmdReceived;
        #endregion Events

        #region Private methods
        protected override void RegisterMessageHandlers()
        {
            IpcProcessor.RegisterMessageHandler(NamedPipeConstants.PingCmdName, PingCmdHandler);
            IpcProcessor.RegisterMessageHandler(NamedPipeConstants.ActionCmdName, ActionCmdHandler);
            IpcProcessor.RegisterMessageHandler(NamedPipeConstants.FuncCmdName, FuncCmdHandler);
        }

        private IpcResponse PingCmdHandler(IpcMessage _)
        {
            try
            {
                PingCmdReceived?.Invoke(this, EventArgs.Empty);
                return GetOkResponse();
            }
            catch (Exception ex)
            {
                return GetErrorResponse(ex, nameof(PingCmdHandler));
            }
        }
        private IpcResponse ActionCmdHandler(IpcMessage msg)
        {
            try
            {
                var inParams = msg.GetData<ActionCmdParams>();
                ActionCmdReceived?.Invoke(this, inParams);
                return GetOkResponse();
            }
            catch (Exception ex)
            {
                return GetErrorResponse(ex, nameof(ActionCmdHandler));
            }
        }
        private IpcResponse FuncCmdHandler(IpcMessage msg)
        {
            try
            {
                var inParams = msg.GetData<FuncCmdInParams>();
                var e = new FuncCmdParams {InParams = inParams};
                FuncCmdReceived?.Invoke(this, e);
                var response = GetOkResponse();
                response.SetData(e.OutParams);
                return response;
            }
            catch (Exception ex)
            {
                return GetErrorResponse(ex, nameof(FuncCmdHandler));
            }
        }
        #endregion Private methods
    }
}
