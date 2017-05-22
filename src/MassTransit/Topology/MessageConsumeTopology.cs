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
namespace MassTransit.Topology
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using Util;


    public class MessageConsumeTopology<TMessage> :
        IMessageConsumeTopologyConfigurator<TMessage>,
        IMessageConsumeTopologyConfigurator
        where TMessage : class
    {
        readonly IList<IMessageConsumeTopologyConvention<TMessage>> _conventions;
        readonly IList<IMessageConsumeTopology<TMessage>> _delegateConfigurations;
        readonly IList<IMessageConsumeTopology<TMessage>> _topologies;

        public MessageConsumeTopology(IMessageEntityNameFormatter<TMessage> entityNameFormatter)
        {
            if (entityNameFormatter == null)
                throw new ArgumentNullException(nameof(entityNameFormatter));

            EntityNameFormatter = entityNameFormatter;
            _conventions = new List<IMessageConsumeTopologyConvention<TMessage>>();
            _topologies = new List<IMessageConsumeTopology<TMessage>>();
            _delegateConfigurations = new List<IMessageConsumeTopology<TMessage>>();
        }

        IMessageConsumeTopologyConfigurator<T> IMessageConsumeTopologyConfigurator.GetMessageTopology<T>()
        {
            var result = this as IMessageConsumeTopologyConfigurator<T>;
            if (result == null)
                throw new ArgumentException($"The expected message type was invalid: {TypeMetadataCache<T>.ShortName}");

            return result;
        }

        public void Add(IMessageConsumeTopology<TMessage> ConsumeTopology)
        {
            _topologies.Add(ConsumeTopology);
        }

        public void AddDelegate(IMessageConsumeTopology<TMessage> configuration)
        {
            _delegateConfigurations.Add(configuration);
        }

        public void Apply(ITopologyPipeBuilder<ConsumeContext<TMessage>> builder)
        {
            ITopologyPipeBuilder<ConsumeContext<TMessage>> delegatedBuilder = builder.CreateDelegatedBuilder();

            foreach (IMessageConsumeTopology<TMessage> topology in _delegateConfigurations)
            {
                topology.Apply(delegatedBuilder);
            }

            foreach (IMessageConsumeTopologyConvention<TMessage> convention in _conventions)
            {
                IMessageConsumeTopology<TMessage> topology;
                if (convention.TryGetMessageConsumeTopology(out topology))
                    topology.Apply(builder);
            }

            foreach (IMessageConsumeTopology<TMessage> topology in _topologies)
            {
                topology.Apply(builder);
            }
        }

        public IMessageEntityNameFormatter<TMessage> EntityNameFormatter { get; private set; }

        public void AddConvention(IMessageConsumeTopologyConvention<TMessage> convention)
        {
            _conventions.Add(convention);
        }

        public void UpdateConvention<TConvention>(Func<TConvention, TConvention> update)
            where TConvention : class, IMessageConsumeTopologyConvention<TMessage>
        {
            for (var i = 0; i < _conventions.Count; i++)
            {
                var convention = _conventions[i] as TConvention;
                if (convention != null)
                {
                    var updatedConvention = update(convention);
                    _conventions[i] = updatedConvention;
                    return;
                }
            }
        }

        public void AddOrUpdateConvention<TConvention>(Func<TConvention> add, Func<TConvention, TConvention> update)
            where TConvention : class, IMessageConsumeTopologyConvention<TMessage>
        {
            for (var i = 0; i < _conventions.Count; i++)
            {
                var convention = _conventions[i] as TConvention;
                if (convention != null)
                {
                    var updatedConvention = update(convention);
                    _conventions[i] = updatedConvention;
                    return;
                }
            }

            var addedConvention = add();
            if (addedConvention != null)
                _conventions.Add(addedConvention);
        }
    }
}