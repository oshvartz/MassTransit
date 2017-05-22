﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.RabbitMqTransport
{
    /// <summary>
    /// Used to configure the binding of an exchange (to either a queue or another exchange)
    /// </summary>
    public interface IExchangeBindingConfigurator :
        IExchangeConfigurator
    {
        /// <summary>
        /// A routing key for the exchange binding
        /// </summary>
        string RoutingKey { set; }

        /// <summary>
        /// Sets the binding argument, or removes it if value is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetBindingArgument(string key, object value);
    }
}