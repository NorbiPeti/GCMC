using System;
using System.Diagnostics;
using Newtonsoft.Json.Serialization;
using uREPL;

namespace GCMC
{
    public class JsonTraceWriter : ITraceWriter
    {
        private long lastpercent;
        public void Trace(TraceLevel level, string message, Exception ex)
        {
            //Log.Output(message + "\n" + ex, lvl, new StackFrame(1, true));
            int i = message.IndexOf("position ", StringComparison.Ordinal);
            int j = message.IndexOf(".", i + 1, StringComparison.Ordinal);
            if (i > -1 && j > -1)
            {
                i += "position ".Length;
                string str = message.Substring(i, j - i);
                long pos = long.Parse(str);
                byte percent = (byte) (pos / (double) FileLength * 100);
                if (lastpercent / 10 == percent / 10) return;
                Log.Output("Deserialization " + percent + "% complete");
                lastpercent = percent;
            }

            //Console.WriteLine(message + "\n" + ex);
        }

        public TraceLevel LevelFilter { get; } = TraceLevel.Info;
        
        public long FileLength { get; set; }
    }
}