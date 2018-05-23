﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.ActiveMqTransport
{
    using System;
    using Apache.NMS;


    /// <summary>
    /// Settings to configure a RabbitMQ host explicitly without requiring the fluent interface
    /// </summary>
    public interface ActiveMqHostSettings
    {
        /// <summary>
        ///     The RabbitMQ host to connect to (should be a valid hostname)
        /// </summary>
        string Host { get; }

        /// <summary>
        ///     The RabbitMQ port to connect
        /// </summary>
        int Port { get; }

        /// <summary>
        ///     The Username for connecting to the host
        /// </summary>
        string Username { get; }

        /// <summary>
        ///     The password for connection to the host
        ///     MAYBE this should be a SecureString instead of a regular string
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Returns the host address
        /// </summary>
        Uri HostAddress { get; }

        bool UseSsl { get; }

        Uri BrokerAddress { get; }

        IConnection CreateConnection();
    }
}