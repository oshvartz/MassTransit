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
namespace MassTransit
{
    using System;
    using System.Threading;
    using Topology;
    using Topology.EntityNameFormatters;
    using Topology.Topologies;
    using Transports.InMemory.Configuration;
    using Transports.InMemory.Configurators;


    public static class InMemoryBus
    {
        public static IMessageTopologyConfigurator MessageTopology => Cached.MessageTopologyValue.Value;

        /// <summary>
        /// Configure and create an in-memory bus
        /// </summary>
        /// <param name="configure">The configuration callback to configure the bus</param>
        /// <returns></returns>
        public static IBusControl Create(Action<IInMemoryBusFactoryConfigurator> configure)
        {
            return Create(null, configure);
        }

        /// <summary>
        /// Configure and create an in-memory bus
        /// </summary>
        /// <param name="baseAddress">Override the default base address</param>
        /// <param name="configure">The configuration callback to configure the bus</param>
        /// <returns></returns>
        public static IBusControl Create(Uri baseAddress, Action<IInMemoryBusFactoryConfigurator> configure)
        {
            var topologyConfiguration = new InMemoryTopologyConfiguration(MessageTopology);
            var busConfiguration = new InMemoryBusConfiguration(topologyConfiguration);
            var endpointConfiguration = busConfiguration.CreateEndpointConfiguration();

            var configurator = new InMemoryBusFactoryConfigurator(busConfiguration, endpointConfiguration, baseAddress);

            configure(configurator);

            return configurator.Build();
        }


        static class Cached
        {
            internal static readonly Lazy<IMessageTopologyConfigurator> MessageTopologyValue =
                new Lazy<IMessageTopologyConfigurator>(() => new MessageTopology(_entityNameFormatter), LazyThreadSafetyMode.PublicationOnly);

            static readonly IEntityNameFormatter _entityNameFormatter;

            static Cached()
            {
                _entityNameFormatter = new MessageUrnEntityNameFormatter();
            }
        }
    }
}