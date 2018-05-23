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
namespace MassTransit.AzureServiceBusTransport.Topology.Configuration
{
    using GreenPipes;
    using Microsoft.ServiceBus.Messaging;


    public interface ISubscriptionConfigurator :
        IEndpointEntityConfigurator,
        ISpecification
    {
        /// <summary>
        /// The path of the subscription's topic
        /// </summary>
        string TopicPath { get; }

        /// <summary>
        /// The subscription name, unique per topic
        /// </summary>
        string SubscriptionName { get; }

        /// <summary>
        /// Sets the path where messages are forwarded to
        /// </summary>
        string ForwardTo { set; }

        /// <summary>
        /// Move messages to the dead letter queue on filter evaluation exception
        /// </summary>
        bool? EnableDeadLetteringOnFilterEvaluationExceptions { set; }

        /// <summary>
        /// Specify the filter for the subscription
        /// </summary>
        Filter Filter { set; }

        /// <summary>
        /// Specify a rule for the subscription
        /// </summary>
        RuleDescription Rule { set; }

        SubscriptionDescription GetSubscriptionDescription();
    }
}