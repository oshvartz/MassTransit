// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.LibLogIntegration.Logging
{
    using System;
    using MassTransit.Logging;


    
    public class LibLog :
        MassTransit.Logging.ILog
    {
        readonly ILog _log;

        /// <summary>
        /// Create a new LibLog logger instance.
        /// </summary>
        /// <param name="name">Name of type to log as.</param>
        public LibLog(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            _log = LogProvider.GetLogger(name);
        }

        public bool IsDebugEnabled => _log.IsDebugEnabled();

        public bool IsInfoEnabled => _log.IsInfoEnabled();

        public bool IsWarnEnabled => _log.IsWarnEnabled();

        public bool IsErrorEnabled => _log.IsErrorEnabled();

        public bool IsFatalEnabled => _log.IsFatalEnabled();

        public void Log(MassTransit.Logging.LogLevel level, object obj)
        {
            _log.Log(GetLibLogLevel(level), obj.ToString);
        }

        public void Log(MassTransit.Logging.LogLevel level, object obj, Exception exception)
        {
            _log.Log(GetLibLogLevel(level), () => obj?.ToString() ?? "", exception);
        }

        public void Log(MassTransit.Logging.LogLevel level, LogOutputProvider messageProvider)
        {
            _log.Log(GetLibLogLevel(level), ToGenerator(messageProvider));
        }

        public void LogFormat(MassTransit.Logging.LogLevel level, IFormatProvider formatProvider, string format,
            params object[] args)
        {
            _log.Log(GetLibLogLevel(level), () => string.Format(formatProvider, format,args));
        }

        public void LogFormat(MassTransit.Logging.LogLevel level, string format, params object[] args)
        {
            _log.Log(GetLibLogLevel(level), () => format, null, args);
        }

        public void Debug(object obj)
        {
            _log.Log(LogLevel.Debug, obj.ToString);
        }

        public void Debug(object obj, Exception exception)
        {
            _log.Log(LogLevel.Debug, () => obj?.ToString() ?? "", exception);
        }

        public void Debug(LogOutputProvider messageProvider)
        {
            _log.Debug(ToGenerator(messageProvider));
        }

        public void Info(object obj)
        {
            _log.Log(LogLevel.Info, obj.ToString);
        }

        public void Info(object obj, Exception exception)
        {
            _log.Log(LogLevel.Info, () => obj?.ToString() ?? "", exception);
        }

        public void Info(LogOutputProvider messageProvider)
        {
            _log.Info(ToGenerator(messageProvider));
        }

        public void Warn(object obj)
        {
            _log.Log(LogLevel.Warn, obj.ToString);
        }

        public void Warn(object obj, Exception exception)
        {
            _log.Log(LogLevel.Warn, () => obj?.ToString() ?? "", exception);
        }

        public void Warn(LogOutputProvider messageProvider)
        {
            _log.Warn(ToGenerator(messageProvider));
        }

        public void Error(object obj)
        {
            _log.Log(LogLevel.Error, obj.ToString);
        }

        public void Error(object obj, Exception exception)
        {
            _log.Log(LogLevel.Error, () => obj?.ToString() ?? "", exception);
        }

        public void Error(LogOutputProvider messageProvider)
        {
            _log.Error(ToGenerator(messageProvider));
        }

        public void Fatal(object obj)
        {
            _log.Log(LogLevel.Fatal, obj.ToString);
        }

        public void Fatal(object obj, Exception exception)
        {
            _log.Log(LogLevel.Fatal, () => obj?.ToString() ?? "", exception);
        }

        public void Fatal(LogOutputProvider messageProvider)
        {
            _log.Fatal(ToGenerator(messageProvider));
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Debug, () => string.Format(formatProvider, format, args));
        }

        public void DebugFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Debug, () => format, null, args);
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Info, () => string.Format(formatProvider, format, args));
        }

        public void InfoFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Info, () => format, null, args);
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Warn, () => string.Format(formatProvider, format, args));
        }

        public void WarnFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Warn, () => format, null, args);
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Error, () => string.Format(formatProvider, format, args));
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Error, () => format,null, args);
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Fatal, () => string.Format(formatProvider, format, args));
        }

        public void FatalFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Fatal, () => format, null, args);
        }

        LogLevel GetLibLogLevel(MassTransit.Logging.LogLevel level)
        {
            if (level == MassTransit.Logging.LogLevel.Fatal)
                return LogLevel.Fatal;
            if (level == MassTransit.Logging.LogLevel.Error)
                return LogLevel.Error;
            if (level == MassTransit.Logging.LogLevel.Warn)
                return LogLevel.Warn;
            if (level == MassTransit.Logging.LogLevel.Info)
                return LogLevel.Info;
            if (level == MassTransit.Logging.LogLevel.Debug)
                return LogLevel.Debug;
            if (level == MassTransit.Logging.LogLevel.All)
                return LogLevel.Trace;

            return LogLevel.Fatal;
        }

        Func<string> ToGenerator(LogOutputProvider provider)
        {
            return () =>
            {
                var obj = provider();
                return obj?.ToString() ?? "";
            };
        }
    }

}