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
namespace MassTransit.AzureServiceBusTransport.Configuration
{
    using MassTransit.Configuration;
    using MassTransit.Pipeline;


    public class ServiceBusEndpointConfiguration :
        EndpointConfiguration,
        IServiceBusEndpointConfiguration
    {
        readonly IServiceBusTopologyConfiguration _topologyConfiguration;

        public ServiceBusEndpointConfiguration(IServiceBusTopologyConfiguration topologyConfiguration, IConsumePipe consumePipe = null)
            : base(topologyConfiguration, consumePipe)
        {
            _topologyConfiguration = topologyConfiguration;
        }

        ServiceBusEndpointConfiguration(IServiceBusEndpointConfiguration parentConfiguration, IServiceBusTopologyConfiguration topologyConfiguration,
            IConsumePipe consumePipe = null)
            : base(parentConfiguration, topologyConfiguration, consumePipe)
        {
            _topologyConfiguration = topologyConfiguration;
        }

        public new IServiceBusTopologyConfiguration Topology => _topologyConfiguration;

        public IServiceBusEndpointConfiguration CreateEndpointConfiguration()
        {
            var topologyConfiguration = new ServiceBusTopologyConfiguration(_topologyConfiguration);

            return new ServiceBusEndpointConfiguration(this, topologyConfiguration);
        }
    }
}