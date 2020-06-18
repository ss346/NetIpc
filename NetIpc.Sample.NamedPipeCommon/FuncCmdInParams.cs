using System;

namespace NetIpc.Sample.NamedPipeCommon
{
    public class FuncCmdInParams
    {
        public string StrParam { get; set; }
        public double DblParam { get; set; }
        public TimeSpan TimeParam { get; set; }
    }
}
