using System;

namespace NetIpc.Common.Interfaces
{
    public interface IIpcListener : IDisposable
    {
        void SetProcessFunction(Func<string, string> processMessageFunc);
        void StartListen();
        void StopListen();

        event EventHandler<Exception> ExceptionThrown;
    }
}