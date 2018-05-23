﻿// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.HttpTransport
{
    using System;
    using Configuration;
    using Specifications;


    public static class HttpBusFactory
    {
        /// <summary>
        /// Configure and create a bus for RabbitMQ
        /// </summary>
        /// <param name="configure">The configuration callback to configure the bus</param>
        /// <returns></returns>
        public static IBusControl Create(Action<IHttpBusFactoryConfigurator> configure)
        {
            var topologyConfiguration = new HttpTopologyConfiguration(InMemoryBus.MessageTopology);
            var busConfiguration = new HttpBusConfiguration(topologyConfiguration);
            var endpointConfiguration = busConfiguration.CreateEndpointConfiguration();

            var configurator = new HttpBusFactoryConfigurator(busConfiguration, endpointConfiguration);

            configure(configurator);

            return configurator.Build();
        }
    }
}