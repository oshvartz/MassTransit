// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Threading.Tasks;
    using GreenPipes;
    using GreenPipes.Util;
    using MassTransit.Pipeline;
    using MassTransit.Pipeline.Observables;
    using MassTransit.Pipeline.Pipes;
    using Transports;


    public class HttpPublishEndpointProvider :
        IPublishEndpointProvider
    {
        readonly PublishObservable _publishObservable;
        readonly IPublishPipe _publishPipe;
        readonly Uri _hostAddress;
        readonly IMessageSerializer _serializer;
        readonly ISendTransportProvider _transportProvider;

        public HttpPublishEndpointProvider(Uri hostAddress, IMessageSerializer serializer, ISendTransportProvider transportProvider, IPublishPipe publishPipe)
        {
            _hostAddress = hostAddress;
            _serializer = serializer;
            _transportProvider = transportProvider;
            _publishPipe = publishPipe;

            _publishObservable = new PublishObservable();
        }

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            return _publishObservable.Connect(observer);
        }

        public IPublishEndpoint CreatePublishEndpoint(Uri sourceAddress, ConsumeContext consumeContext)
        {
            return new PublishEndpoint(sourceAddress, this, _publishObservable, _publishPipe, consumeContext);
        }

        async Task<ISendEndpoint> IPublishEndpointProvider.GetPublishSendEndpoint<T>(T message)
        {
            // TODO: this needs some love
            var destinationAddress = new Uri("http://localhost");

            var transport = await _transportProvider.GetSendTransport(destinationAddress).ConfigureAwait(false);

            return new SendEndpoint(transport, _serializer, destinationAddress, _hostAddress, SendPipe.Empty);
        }

        public ConnectHandle ConnectSendObserver(ISendObserver observer)
        {
            return new EmptyConnectHandle();
        }
    }
}