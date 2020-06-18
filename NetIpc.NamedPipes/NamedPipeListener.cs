using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using NetIpc.Common.Interfaces;

namespace NetIpc.NamedPipes
{
    public class NamedPipeListener : IIpcListener
    {

        #region Constants
        private const int MaxNumberOfServerInstances = 20;
        #endregion Constants

        #region Private data
        private bool _disposed;
        private readonly string _pipeName;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly IDictionary<string, NamedPipeServerStreamWrapper> _serverStreams;
        private Func<string, string> _processMessageFunc;
        #endregion

        #region Events
        public event EventHandler<Exception> ExceptionThrown;
        #endregion Events

        #region Constructors
        public NamedPipeListener(string pipeName)
        {
            _pipeName = pipeName;
            _synchronizationContext = AsyncOperationManager.SynchronizationContext;
            _serverStreams = new ConcurrentDictionary<string, NamedPipeServerStreamWrapper>();
        }
        #endregion Constructors

        #region Public methods
        public void SetProcessFunction(Func<string, string> processMessageFunc)
        {
            _processMessageFunc = processMessageFunc;
        }
        public void StartListen()
        {
            StartNamedPipeServerStream();
        }
        public void StopListen()
        {
            foreach (var server in _serverStreams.Values)
            {
                try
                {
                    UnregisterFromServerEvents(server);
                    server.StopListen();
                }
                catch (Exception ex)
                {
                    ExceptionThrown?.Invoke(this, ex);
                }
            }

            _serverStreams.Clear();
        }
        #endregion

        #region Private methods
        private void StartNamedPipeServerStream()
        {
            var server = new NamedPipeServerStreamWrapper(_pipeName, MaxNumberOfServerInstances);
            server.SetProcessFunction(_processMessageFunc);
            server.ExceptionThrown += OnExceptionThrown;
            server.ClientConnected += OnClientConnected;
            server.ClientDisconnected += OnClientDisconnected;

            _serverStreams[server.ID] = server;

            server.StartListen();
        }
        private void StopNamedPipeServer(string id)
        {
            UnregisterFromServerEvents(_serverStreams[id]);
            _serverStreams[id].StopListen();
            _serverStreams.Remove(id);
        }
        private void UnregisterFromServerEvents(NamedPipeServerStreamWrapper server)
        {
            server.ExceptionThrown -= OnExceptionThrown;
            server.ClientConnected -= OnClientConnected;
            server.ClientDisconnected -= OnClientDisconnected;
        }
        #endregion

        #region Event handlers
        private void OnExceptionThrown(object sender, Exception ea)
        {
            _synchronizationContext.Post(e => ExceptionThrown?.Invoke(this, (Exception)e), ea);
        }
        private void OnClientConnected(object sender, string clientID)
        {
            StartNamedPipeServerStream(); // Create a additional server as a preparation for new connection
        }
        private void OnClientDisconnected(object sender, string clientID)
        {
            StopNamedPipeServer(clientID);
        }
        #endregion Event handlers

        #region IDisposable implementation
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                StopListen();
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
