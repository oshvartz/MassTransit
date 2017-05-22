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
namespace MassTransit.Transports.InMemory.Topology
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using MassTransit.Topology;
    using Newtonsoft.Json.Linq;
    using Util;


    public class InMemoryMessageConsumeTopology<TMessage> :
        MessageConsumeTopology<TMessage>,
        IInMemoryMessageConsumeTopologyConfigurator<TMessage>,
        IInMemoryMessageConsumeTopologyConfigurator
        where TMessage : class
    {
        readonly IList<IInMemoryConsumeTopologySpecification> _specifications;

        public InMemoryMessageConsumeTopology(IMessageEntityNameFormatter<TMessage> entityNameFormatter)
            : base(entityNameFormatter)
        {
            _specifications = new List<IInMemoryConsumeTopologySpecification>();
        }

        bool IsBindableMessageType => typeof(JToken) != typeof(TMessage);

        IInMemoryMessageConsumeTopologyConfigurator<T> IInMemoryMessageConsumeTopologyConfigurator.GetMessageTopology<T>()
        {
            var result = this as IInMemoryMessageConsumeTopologyConfigurator<T>;
            if (result == null)
                throw new ArgumentException($"The expected message type was invalid: {TypeMetadataCache<T>.ShortName}");

            return result;
        }

        public void Apply(IInMemoryConsumeTopologyBuilder builder)
        {
            foreach (var specification in _specifications)
            {
                specification.Apply(builder);
            }
        }

        public void Bind()
        {
            if (!IsBindableMessageType)
            {
                _specifications.Add(new InvalidInMemoryConsumeTopologySpecification(TypeMetadataCache<TMessage>.ShortName, "Is not a bindable message type"));
                return;
            }

            var exchangeName = EntityNameFormatter.FormatEntityName();

            var specification = new ExchangeBindingConsumeTopologySpecification(exchangeName);

            _specifications.Add(specification);
        }
    }
}