using System;

namespace NetIpc.Common.Interfaces
{
    public interface IIpcEngine : IDisposable
    {
        void Initialize();
        void Start();
        void Stop();

        event EventHandler<Exception> ExceptionThrown;
    }
}