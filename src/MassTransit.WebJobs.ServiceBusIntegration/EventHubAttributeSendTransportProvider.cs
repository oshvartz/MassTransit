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
namespace MassTransit.WebJobs.ServiceBusIntegration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBusTransport.Transport;
    using Logging;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Transports;


    public class EventHubAttributeSendTransportProvider :
        ISendTransportProvider
    {
        readonly IBinder _binder;
        readonly CancellationToken _cancellationToken;
        readonly ILog _log;

        public EventHubAttributeSendTransportProvider(IBinder binder, ILog log, CancellationToken cancellationToken)
        {
            _binder = binder;
            _log = log;
            _cancellationToken = cancellationToken;
        }

        async Task<ISendTransport> ISendTransportProvider.GetSendTransport(Uri address)
        {
            var eventHubName = address.AbsolutePath.Trim('/');

            var attribute = new EventHubAttribute(eventHubName);

            IAsyncCollector<EventData> collector = await _binder.BindAsync<IAsyncCollector<EventData>>(attribute, _cancellationToken).ConfigureAwait(false);

            var client = new CollectorEventDataSendEndpointContext(eventHubName, _log, collector, _cancellationToken);

            var source = new CollectorEventDataSendEndpointContextSource(client);

            var transport = new EventHubSendTransport(source, address);

            return transport;
        }
    }
}