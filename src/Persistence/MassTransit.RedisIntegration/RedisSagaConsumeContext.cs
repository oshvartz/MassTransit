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
namespace MassTransit.RedisIntegration
{
    using System;
    using System.Threading.Tasks;
    using Context;
    using Logging;
    using Util;


    public class RedisSagaConsumeContext<TSaga, TMessage> :
        ConsumeContextProxyScope<TMessage>,
        SagaConsumeContext<TSaga, TMessage>
        where TMessage : class
        where TSaga : class, IVersionedSaga
    {
        static readonly ILog Log = Logger.Get<RedisSagaConsumeContext<TSaga, TMessage>>();

        readonly ITypedDatabase<TSaga> _sagas;

        public RedisSagaConsumeContext(ITypedDatabase<TSaga> sagas, ConsumeContext<TMessage> context, TSaga instance)
            : base(context)
        {
            _sagas = sagas;

            Saga = instance;
        }

        Guid? MessageContext.CorrelationId => Saga.CorrelationId;

        SagaConsumeContext<TSaga, T> SagaConsumeContext<TSaga>.PopContext<T>()
        {
            if (this is SagaConsumeContext<TSaga, T> context)
                return context;

            throw new ContextException($"The ConsumeContext<{TypeMetadataCache<TMessage>.ShortName}> could not be cast to {TypeMetadataCache<T>.ShortName}");
        }

        async Task SagaConsumeContext<TSaga>.SetCompleted()
        {
            await _sagas.Delete(Saga.CorrelationId).ConfigureAwait(false);

            IsCompleted = true;

            if (Log.IsDebugEnabled)
                Log.DebugFormat("SAGA:{0}:{1} Removed {2}", TypeMetadataCache<TSaga>.ShortName, TypeMetadataCache<TMessage>.ShortName, Saga.CorrelationId);
        }

        public TSaga Saga { get; }
        public bool IsCompleted { get; private set; }
    }
}