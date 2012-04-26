using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AccelLib
{
    public enum LogLevel
    {
        Warning,Error,Info,Debug
    }

    public interface ILogger
    {
        void Log(string message);
        void Log(string message, LogLevel level);
    }
}
