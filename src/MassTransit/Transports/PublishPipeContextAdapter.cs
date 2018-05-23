// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Transports
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Context;
    using GreenPipes;
    using Pipeline;


    public class PublishPipeContextAdapter<T> :
        IPipe<SendContext<T>>
        where T : class
    {
        readonly T _message;
        readonly IPublishObserver _observer;
        readonly IPipe<PublishContext<T>> _pipe;
        readonly IPublishPipe _publishPipe;
        readonly Uri _sourceAddress;
        readonly ConsumeContext _consumeContext;
        PublishContext<T> _context;

        public PublishPipeContextAdapter(IPipe<PublishContext<T>> pipe, IPublishPipe publishPipe, IPublishObserver observer, Uri sourceAddress,
            ConsumeContext consumeContext, T message)
        {
            _pipe = pipe;
            _publishPipe = publishPipe;
            _observer = observer;
            _sourceAddress = sourceAddress;
            _consumeContext = consumeContext;
            _message = message;
        }

        public PublishPipeContextAdapter(IPipe<PublishContext> pipe, IPublishPipe publishPipe, IPublishObserver observer, Uri sourceAddress,
            ConsumeContext consumeContext, T message)
        {
            _pipe = pipe;
            _publishPipe = publishPipe;
            _observer = observer;
            _sourceAddress = sourceAddress;
            _consumeContext = consumeContext;
            _message = message;
        }

        public PublishPipeContextAdapter(IPublishPipe publishPipe, IPublishObserver observer, Uri sourceAddress, ConsumeContext consumeContext, T message)
        {
            _pipe = Pipe.Empty<PublishContext<T>>();
            _publishPipe = publishPipe;
            _observer = observer;
            _sourceAddress = sourceAddress;
            _consumeContext = consumeContext;
            _message = message;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _pipe.Probe(context);
        }

        public async Task Send(SendContext<T> context)
        {
            if (_consumeContext != null)
                context.TransferConsumeContextHeaders(_consumeContext);

            context.SourceAddress = _sourceAddress;

            var publishContext = new PublishContextProxy<T>(context, context.Message);
            var firstTime = Interlocked.CompareExchange(ref _context, publishContext, null) == null;

            await _publishPipe.Send(publishContext).ConfigureAwait(false);

            await _pipe.Send(publishContext).ConfigureAwait(false);

            if (firstTime)
                await _observer.PrePublish(publishContext).ConfigureAwait(false);
        }

        public Task PostPublish()
        {
            return _observer.PostPublish(_context ?? GetDefaultPublishContext());
        }

        public Task PublishFaulted(Exception exception)
        {
            return _observer.PublishFault(_context ?? GetDefaultPublishContext(), exception);
        }

        PublishContext<T> GetDefaultPublishContext()
        {
            return new FaultedPublishContext<T>(_message, CancellationToken.None)
            {
                SourceAddress = _sourceAddress,
                ConversationId = _consumeContext?.ConversationId,
                InitiatorId = _consumeContext?.CorrelationId,
                Mandatory = _context?.Mandatory ?? false
            };
        }
    }
}