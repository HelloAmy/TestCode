using System;
using System.Collections.Generic;

namespace YZ.Utility
{
    public interface ILogEmitter
    {
        void Init(Dictionary<string, string> param);

        void EmitLog(LogEntry log);
    }
}
