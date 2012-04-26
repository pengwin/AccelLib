using System;
using System.Diagnostics;
using AccelLib;

namespace AccelLibTest.Stubs
{
    /// <summary>
    /// Заглушка для логгера
    /// Пишет сообщения в отладочную консоль
    /// </summary>
    class LoggerStub : ILogger
    {
       
        public void Log(string message)
        {
            Log(message,LogLevel.Info);
        }

        public void Log(string message, LogLevel level)
        {
            
            Debug.WriteLine(string.Format("{0}:{1}: {2}",DateTime.Now,level,message));
        }
    }
}
