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
namespace MassTransit.Util
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Internals.Extensions;
    using Newtonsoft.Json.Linq;
    using Saga;
    using Serialization;


    public static class TypeMetadataCache
    {
        static CachedType GetOrAdd(Type type)
        {
            return Cached.Instance.GetOrAdd(type, _ =>
                (CachedType)Activator.CreateInstance(typeof(CachedType<>).MakeGenericType(type)));
        }

        public static string GetShortName(Type type)
        {
            return GetOrAdd(type).ShortName;
        }

        public static bool IsValidMessageType(Type type)
        {
            return GetOrAdd(type).IsValidMessageType;
        }

        public static Type[] GetMessageTypes(Type type)
        {
            return GetOrAdd(type).MessageTypes;
        }


        static class Cached
        {
            internal static readonly ConcurrentDictionary<Type, CachedType> Instance = new ConcurrentDictionary<Type, CachedType>();
        }


        interface CachedType
        {
            bool IsValidMessageType { get; }
            string ShortName { get; }
            Type[] MessageTypes { get; }
        }


        class CachedType<T> :
            CachedType
        {
            public bool IsValidMessageType => TypeMetadataCache<T>.IsValidMessageType;

            public string ShortName => TypeMetadataCache<T>.ShortName;

            public Type[] MessageTypes => TypeMetadataCache<T>.MessageTypes;
        }
    }


    public class TypeMetadataCache<T> :
        ITypeMetadataCache<T>
    {
        readonly Lazy<bool> _hasSagaInterfaces;
        readonly Lazy<bool> _isTemporaryMessageType;
        readonly Lazy<bool> _isValidMessageType;
        readonly Lazy<string[]> _messageTypeNames;
        readonly Lazy<Type[]> _messageTypes;
        readonly Lazy<List<PropertyInfo>> _properties;
        readonly string _shortName;

        TypeMetadataCache()
        {
            _shortName = typeof(T).GetTypeName();

            _hasSagaInterfaces = new Lazy<bool>(ScanForSagaInterfaces, LazyThreadSafetyMode.PublicationOnly);

            _properties = new Lazy<List<PropertyInfo>>(() => typeof(T).GetAllProperties().ToList());

            _isValidMessageType = new Lazy<bool>(CheckIfValidMessageType);
            _isTemporaryMessageType = new Lazy<bool>(() => CheckIfTemporaryMessageType(typeof(T)));
            _messageTypes = new Lazy<Type[]>(() => GetMessageTypes().ToArray());
            _messageTypeNames = new Lazy<string[]>(() => GetMessageTypeNames().ToArray());
        }

        public static string ShortName => Cached.Metadata.Value.ShortName;
        public static bool HasSagaInterfaces => Cached.Metadata.Value.HasSagaInterfaces;
        public static IEnumerable<PropertyInfo> Properties => Cached.Metadata.Value.Properties;
        public static bool IsValidMessageType => Cached.Metadata.Value.IsValidMessageType;
        public static bool IsTemporaryMessageType => Cached.Metadata.Value.IsTemporaryMessageType;
        public static Type[] MessageTypes => Cached.Metadata.Value.MessageTypes;
        public static string[] MessageTypeNames => Cached.Metadata.Value.MessageTypeNames;
        bool ITypeMetadataCache<T>.IsTemporaryMessageType => _isTemporaryMessageType.Value;
        string[] ITypeMetadataCache<T>.MessageTypeNames => _messageTypeNames.Value;
        IEnumerable<PropertyInfo> ITypeMetadataCache<T>.Properties => _properties.Value;
        bool ITypeMetadataCache<T>.IsValidMessageType => _isValidMessageType.Value;
        Type[] ITypeMetadataCache<T>.MessageTypes => _messageTypes.Value;

        T ITypeMetadataCache<T>.InitializeFromObject(object values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var objValues = JObject.FromObject(values, JsonMessageSerializer.Serializer);

            return objValues.ToObject<T>(JsonMessageSerializer.Deserializer);
        }

        bool ITypeMetadataCache<T>.HasSagaInterfaces => _hasSagaInterfaces.Value;
        string ITypeMetadataCache<T>.ShortName => _shortName;

        /// <summary>
        /// Returns true if the specified type is an allowed message type, i.e.
        /// that it doesn't come from the .Net core assemblies or is without a namespace,
        /// amongst others.
        /// </summary>
        /// <returns>True if the message can be sent, otherwise false</returns>
        bool CheckIfValidMessageType()
        {
            if (typeof(T).Namespace == null)
                return false;

            if (typeof(T).Assembly == typeof(object).Assembly)
                return false;

            if (typeof(T).Namespace == "System")
                return false;

            var ns = typeof(T).Namespace;
            if (ns != null && ns.StartsWith("System."))
                return false;

            if (typeof(T).IsGenericType)
            {
                var typeDefinition = typeof(T).GetGenericTypeDefinition();
                if (typeDefinition == typeof(CorrelatedBy<>))
                    return false;
                if (typeDefinition == typeof(Orchestrates<>))
                    return false;
                if (typeDefinition == typeof(InitiatedBy<>))
                    return false;
                if (typeDefinition == typeof(Observes<,>))
                    return false;

                if (typeof(T).IsOpenGeneric())
                    return false;
            }

            if (typeof(T).IsAnonymousType())
                return false;

            return true;
        }

        bool CheckIfTemporaryMessageType(Type messageType)
        {
            return (!messageType.IsVisible && messageType.IsClass)
                || (messageType.IsGenericType && messageType.GetGenericArguments().Any(CheckIfTemporaryMessageType));
        }

        /// <summary>
        /// Returns all the message types that are available for the specified type. This will
        /// return any base classes or interfaces implemented by the type that are allowed
        /// message types.
        /// </summary>
        /// <returns>An enumeration of valid message types implemented by the specified type</returns>
        static IEnumerable<Type> GetMessageTypes()
        {
            if (IsValidMessageType)
                yield return typeof(T);

            var baseType = typeof(T).BaseType;
            while ((baseType != null) && TypeMetadataCache.IsValidMessageType(baseType))
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }

            IEnumerable<Type> interfaces = typeof(T)
                .GetInterfaces()
                .Where(TypeMetadataCache.IsValidMessageType);

            foreach (var interfaceType in interfaces)
                yield return interfaceType;
        }

        static IEnumerable<string> GetMessageTypeNames()
        {
            return MessageTypes.Select(x => new MessageUrn(x).ToString());
        }

        public static T InitializeFromObject(object values)
        {
            return Cached.Metadata.Value.InitializeFromObject(values);
        }

        static bool ScanForSagaInterfaces()
        {
            Type[] interfaces = typeof(T).GetInterfaces();

            if (interfaces.Contains(typeof(ISaga)))
                return true;

            return interfaces.Any(x => x.HasInterface(typeof(InitiatedBy<>))
                || x.HasInterface(typeof(Orchestrates<>))
                || x.HasInterface(typeof(Observes<,>)));
        }


        static class Cached
        {
            internal static readonly Lazy<ITypeMetadataCache<T>> Metadata = new Lazy<ITypeMetadataCache<T>>(
                () => new TypeMetadataCache<T>(), LazyThreadSafetyMode.PublicationOnly);
        }
    }
}