using System;
using NetIpc.Common;
using NetIpc.Common.Interfaces;
using NetIpc.Sample.NamedPipeCommon;

namespace NetIpc.Sample.NamedPipeClient
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("NamedPipe IPC client");
            var namedPipeClient = new NamedPipes.NamedPipeClient(NamedPipeConstants.PipeName);
            var ipcTransmitter = new IpcTransmitter(namedPipeClient);

            SendPingMessage(ipcTransmitter);
            SendActionMessage(ipcTransmitter);
            SendFuncMessage(ipcTransmitter, false); //without error
            SendFuncMessage(ipcTransmitter, true);  //with error

            Console.WriteLine("Press <ENTER> to exit...");
            Console.ReadLine();
        }

        private static void SendPingMessage(IIpcTransmitter ipcTransmitter)
        {
            Console.WriteLine("Press <ENTER> to send Ping message...");
            Console.ReadLine();
            try
            {
                var response = ipcTransmitter.SendMessage(NamedPipeConstants.PingCmdName);
                if (response.Success)
                    Console.WriteLine($"{DateTime.Now} Ping message processed successfully");
                else
                {
                    var errorMessage = response.GetData<string>();
                    Console.WriteLine($"{DateTime.Now} Ping message processed with error. {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} Ping message failed. {ex.Message}");
            }
            Console.WriteLine();
        }
        private static void SendActionMessage(IIpcTransmitter ipcTransmitter)
        {
            Console.WriteLine("Press <ENTER> to send Action message...");
            Console.ReadLine();
            try
            {
                var inParams = new ActionCmdParams
                {
                    StrParam = "Test action string",
                    IntParam = 12,
                    DateParam = DateTime.Now
                };
                Console.WriteLine($"StrParam: {inParams.StrParam}");
                Console.WriteLine($"IntParam: {inParams.IntParam}");
                Console.WriteLine($"DateParam: {inParams.DateParam}");

                var response = ipcTransmitter.SendMessage(NamedPipeConstants.ActionCmdName, inParams);
                if (response.Success)
                    Console.WriteLine($"{DateTime.Now} Action message processed successfully");
                else
                {
                    var errorMessage = response.GetData<string>();
                    Console.WriteLine($"{DateTime.Now} Action message processed with error. {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} Action message failed. {ex.Message}");
            }
            Console.WriteLine();
        }
        private static void SendFuncMessage(IIpcTransmitter ipcTransmitter, bool withError)
        {
            Console.WriteLine($"Press <ENTER> to send Function message{(withError ? " with error" : string.Empty)}...");
            Console.ReadLine();
            try
            {
                var inParams = new FuncCmdInParams
                {
                    StrParam = withError ? NamedPipeConstants.ErrorStrParam : "Test function string",
                    DblParam = 1.234,
                    TimeParam = DateTime.Now - DateTime.Today
                };
                Console.WriteLine("Input Parameters:");
                Console.WriteLine($"StrParam: {inParams.StrParam}");
                Console.WriteLine($"DblParam: {inParams.DblParam}");
                Console.WriteLine($"TimeParam: {inParams.TimeParam}");

                var response = ipcTransmitter.SendMessage(NamedPipeConstants.FuncCmdName, inParams);
                if (response.Success)
                {
                    var outParams = response.GetData<FuncCmdOutParams>();
                    Console.WriteLine($"{DateTime.Now} Function message processed successfully");
                    Console.WriteLine("Output Parameters:");
                    Console.WriteLine($"StrParam: {outParams.StrParam}");
                    Console.WriteLine($"DblParam: {outParams.DecParam}");
                    Console.WriteLine($"TimeParam: {outParams.BoolParam}");
                }
                else
                {
                    var errorMessage = response.GetData<string>();
                    Console.WriteLine($"{DateTime.Now} Function message processed with error. {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} Function message failed. {ex.Message}");
            }
            Console.WriteLine();
        }

    }
}
