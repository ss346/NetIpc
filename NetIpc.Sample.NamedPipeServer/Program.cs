using System;
using NetIpc.NamedPipes;
using NetIpc.Sample.NamedPipeCommon;

namespace NetIpc.Sample.NamedPipeServer
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("NamedPipe IPC engine");

            var ipcListener = new NamedPipeListener(NamedPipeConstants.PipeName);
            var ipcEngine = new ServiceIpcEngine(ipcListener);
            ipcEngine.PingCmdReceived += OnPingCmdReceived;
            ipcEngine.ActionCmdReceived += OnActionCmdReceived;
            ipcEngine.FuncCmdReceived += OnFuncCmdReceived;
            ipcEngine.Initialize();
            ipcEngine.Start();

            Console.WriteLine("Press <ENTER> to exit...");
            Console.ReadLine();

            ipcEngine.Stop();
            ipcEngine.Dispose();
        }


        private static void OnPingCmdReceived(object sender, EventArgs e)
        {
            Console.WriteLine($"\r\n{DateTime.Now} Ping message received");
        }
        private static void OnActionCmdReceived(object sender, ActionCmdParams e)
        {
            Console.WriteLine($"\r\n{DateTime.Now} Action message received");
            Console.WriteLine($"StrParam: {e.StrParam}");
            Console.WriteLine($"IntParam: {e.IntParam}");
            Console.WriteLine($"DateParam: {e.DateParam}");
        }
        private static void OnFuncCmdReceived(object sender, FuncCmdParams e)
        {
            Console.WriteLine($"\r\n{DateTime.Now} Function message received");
            Console.WriteLine($"StrParam: {e.InParams.StrParam}");
            Console.WriteLine($"DblParam: {e.InParams.DblParam}");
            Console.WriteLine($"TimeParam: {e.InParams.TimeParam}");

            //message process simulation
            if (e.InParams.StrParam.Equals(NamedPipeConstants.ErrorStrParam))
                throw new Exception("Error occurs while processing Function message");

            e.OutParams = new FuncCmdOutParams
            {
                StrParam = $"{e.InParams.StrParam} after processing",
                DecParam = (decimal)e.InParams.DblParam + 1000,
                BoolParam = true
            };
        }
    }
}
