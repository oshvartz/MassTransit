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
namespace MassTransit.RabbitMqTransport.Topology.Conventions.RoutingKey
{
    using MassTransit.Topology;
    using MassTransit.Topology.Conventions;
    using Topology;


    public class RoutingKeySendTopologyConvention :
        IRoutingKeySendTopologyConvention
    {
        readonly IConventionTypeCache<IMessageSendTopologyConvention> _typeCache;

        public RoutingKeySendTopologyConvention()
        {
            DefaultFormatter = new EmptyRoutingKeyFormatter();

            _typeCache = new ConventionTypeCache<IMessageSendTopologyConvention>(typeof(IRoutingKeyMessageSendTopologyConvention<>), new CacheFactory());
        }

        bool IMessageSendTopologyConvention.TryGetMessageSendTopologyConvention<T>(out IMessageSendTopologyConvention<T> convention)
        {
            return _typeCache.GetOrAdd<T, IMessageSendTopologyConvention<T>>().TryGetMessageSendTopologyConvention(out convention);
        }

        public IRoutingKeyFormatter DefaultFormatter { get; set; }


        class CacheFactory :
            IConventionTypeCacheFactory<IMessageSendTopologyConvention>
        {
            IMessageSendTopologyConvention IConventionTypeCacheFactory<IMessageSendTopologyConvention>.Create<T>()
            {
                return new RoutingKeyMessageSendTopologyConvention<T>(null);
            }
        }
    }
}