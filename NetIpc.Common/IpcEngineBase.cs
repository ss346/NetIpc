using System;
using NetIpc.Common.Interfaces;

namespace NetIpc.Common
{
    public abstract class IpcEngineBase : IIpcEngine
    {

        #region Private data
        protected readonly IIpcProcessor IpcProcessor;
        private bool _disposed;
        private readonly IIpcListener _ipcListener;
        private readonly string _engineName;
        #endregion Private data

        #region Events
        public event EventHandler<Exception> ExceptionThrown;
        #endregion Events

        #region Constructors
        protected IpcEngineBase(IIpcListener ipcListener, string engineName = "")
        {
            IpcProcessor = new IpcProcessor();
            _ipcListener = ipcListener;
            _ipcListener.ExceptionThrown += OnIpcListenerExceptionThrown;
            _engineName = string.IsNullOrEmpty(engineName) ? string.Empty : $"{engineName} ";
        }
        #endregion Constructors

        #region Public methods
        public void Initialize()
        {
            RegisterMessageHandlers();
            _ipcListener.SetProcessFunction(IpcProcessor.ProcessMessage);
        }
        public void Start()
        {
            _ipcListener.StartListen();
        }
        public void Stop()
        {
            _ipcListener.StopListen();
        }
        #endregion Public methods

        #region Private methods
        protected abstract void RegisterMessageHandlers();

        protected IpcResponse GetOkResponse()
        {
            return new IpcResponse(true);
        }
        protected IpcResponse GetErrorResponse(Exception ex, string methodName, string suffix = null)
        {
            var message = string.IsNullOrEmpty(suffix)
                ? $"{_engineName}{methodName}() error"
                : $"{_engineName}{methodName}() {suffix} error";
            var outerEx = new Exception(message, ex);
            ExceptionThrown?.Invoke(this, outerEx);
            var response = new IpcResponse(false);
            response.SetData(ex.Message);
            return response;
        }
        protected IpcResponse GetErrorResponse(string message, string methodName, string suffix = null)
        {
            var prefix = string.IsNullOrEmpty(suffix)
                ? $"{_engineName}{methodName}() error"
                : $"{_engineName}{methodName}() {suffix} error";
            var ex = new Exception($"{prefix}. {message}");
            ExceptionThrown?.Invoke(this, ex);
            var response = new IpcResponse(false);
            response.SetData(message);
            return response;
        }
        protected static IpcResponse GetDataResponse<T>(T data)
        {
            var response = new IpcResponse(true);
            response.SetData(data);
            return response;
        }
        #endregion Private methods

        #region Event handlers
        private void OnIpcListenerExceptionThrown(object sender, Exception ex)
        {
            var outerEx = new Exception($"{_engineName}IPC listener error", ex);
            ExceptionThrown?.Invoke(this, outerEx);
        }
        #endregion Event handlers

        #region IDisposable implementation
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                try
                {
                    if (_ipcListener != null)
                    {
                        _ipcListener.StopListen();
                        _ipcListener.ExceptionThrown -= OnIpcListenerExceptionThrown;
                        _ipcListener.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    var outerEx = new Exception($"{_engineName}IPC engine Dispose() error", ex);
                    ExceptionThrown?.Invoke(this, outerEx);
                }
            }
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable implementation

    }
}
