// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.ExtensionsDependencyInjectionIntegration
{
    using System;
    using Context;
    using GreenPipes;
    using Microsoft.Extensions.DependencyInjection;
    using Scoping;
    using Scoping.ConsumerContexts;
    using Util;


    public class DependencyInjectionConsumerScopeProvider :
        IConsumerScopeProvider
    {
        readonly IServiceProvider _serviceProvider;

        public DependencyInjectionConsumerScopeProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.Add("provider", "dependencyInjection");
        }

        IConsumerScopeContext IConsumerScopeProvider.GetScope(ConsumeContext context)
        {
            if (context.TryGetPayload<IServiceScope>(out var existingServiceScope))
                return new ExistingConsumerScopeContext(context);

            var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var serviceScope = scopeFactory.CreateScope();
            try
            {
                var proxy = new ConsumeContextProxyScope(context);

                var scope = serviceScope;
                proxy.GetOrAddPayload(() => scope);
                proxy.GetOrAddPayload(() => scope.ServiceProvider);

                return new CreatedConsumerScopeContext<IServiceScope>(scope, proxy);
            }
            catch
            {
                serviceScope.Dispose();

                throw;
            }
        }

        IConsumerScopeContext<TConsumer, T> IConsumerScopeProvider.GetScope<TConsumer, T>(ConsumeContext<T> context)
        {
            if (context.TryGetPayload<IServiceScope>(out var existingServiceScope))
            {
                var consumer = existingServiceScope.ServiceProvider.GetService<TConsumer>();
                if (consumer == null)
                    throw new ConsumerException($"Unable to resolve consumer type '{TypeMetadataCache<TConsumer>.ShortName}'.");

                ConsumerConsumeContext<TConsumer, T> consumerContext = context.PushConsumer(consumer);

                return new ExistingConsumerScopeContext<TConsumer, T>(consumerContext);
            }

            var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var serviceScope = scopeFactory.CreateScope();
            try
            {
                var scope = serviceScope;
                
                var consumer = scope.ServiceProvider.GetService<TConsumer>();
                if (consumer == null)
                    throw new ConsumerException($"Unable to resolve consumer type '{TypeMetadataCache<TConsumer>.ShortName}'.");

                ConsumerConsumeContext<TConsumer, T> consumerContext = context.PushConsumerScope(consumer, scope);
                consumerContext.GetOrAddPayload(() => scope.ServiceProvider);

                return new CreatedConsumerScopeContext<IServiceScope, TConsumer, T>(scope, consumerContext);
            }
            catch
            {
                serviceScope.Dispose();

                throw;
            }
        }
    }
}