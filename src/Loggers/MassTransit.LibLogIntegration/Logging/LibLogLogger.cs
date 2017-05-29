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
    using System.Collections.Concurrent;


    public class LibLogLogger :
        MassTransit.Logging.ILogger
    {
        readonly ConcurrentDictionary<string, MassTransit.Logging.ILog> _logs; 


        public LibLogLogger() 
        {
            _logs = new ConcurrentDictionary<string, MassTransit.Logging.ILog>();
        }

        public MassTransit.Logging.ILog Get(string name)
        {
            return _logs.GetOrAdd(name, x => new LibLog(x));
        }

        public void Shutdown()
        {
        }

        public static void Use()
        {
            MassTransit.Logging.Logger.UseLogger(new LibLogLogger());
        }
    }
}