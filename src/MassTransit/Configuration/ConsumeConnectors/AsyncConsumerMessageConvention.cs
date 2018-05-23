// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.ConsumeConnectors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;


    /// <summary>
    /// A default convention that looks for IConsumerOfT message types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncConsumerMessageConvention<T> :
        IConsumerMessageConvention
        where T : class
    {
        public IEnumerable<IMessageInterfaceType> GetMessageTypes()
        {
            var typeInfo = typeof(T).GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IConsumer<>))
            {
                var interfaceType = new ConsumerInterfaceType(typeof(T).GetGenericArguments()[0], typeof(T));
                if (interfaceType.MessageType.GetTypeInfo().IsValueType == false && interfaceType.MessageType != typeof(string))
                    yield return interfaceType;
            }

            IEnumerable<IMessageInterfaceType> types = typeof(T).GetInterfaces()
                .Where(x => x.GetTypeInfo().IsGenericType)
                .Where(x => x.GetGenericTypeDefinition() == typeof(IConsumer<>))
                .Select(x => new ConsumerInterfaceType(x.GetGenericArguments()[0], typeof(T)))
                .Where(x => x.MessageType.GetTypeInfo().IsValueType == false && x.MessageType != typeof(string));

            foreach (IMessageInterfaceType type in types)
                yield return type;
        }
    }
}