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
namespace MassTransit.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting.Messaging;
    using Context;


    /// <summary>
    /// A static consume context from the Binary serializer
    /// </summary>
    public class StaticConsumeContext :
        BaseConsumeContext
    {
        readonly Header[] _binaryHeaders;
        readonly object _message;
        readonly IDictionary<Type, object> _messageTypes;
        readonly string[] _supportedTypes;
        Guid? _conversationId;
        Guid? _correlationId;
        Uri _destinationAddress;
        Uri _faultAddress;
        Headers _headers;
        HostInfo _host;
        Guid? _initiatorId;
        Guid? _messageId;
        Guid? _requestId;
        Uri _responseAddress;
        Uri _sourceAddress;
        DateTime? _sentTime;

        public StaticConsumeContext(ReceiveContext receiveContext, object message, Header[] headers)
            : base(receiveContext)
        {
            _messageTypes = new Dictionary<Type, object>();
            _message = message;
            _binaryHeaders = headers;
            _supportedTypes = GetSupportedMessageTypes().ToArray();
        }

        public override Guid? MessageId => _messageId ?? (_messageId = GetHeaderGuid(BinaryMessageSerializer.MessageIdKey));
        public override Guid? RequestId => _requestId ?? (_requestId = GetHeaderGuid(BinaryMessageSerializer.RequestIdKey));
        public override Guid? CorrelationId => _correlationId ?? (_correlationId = GetHeaderGuid(BinaryMessageSerializer.CorrelationIdKey));
        public override Guid? ConversationId => _conversationId ?? (_conversationId = GetHeaderGuid(BinaryMessageSerializer.ConversationIdKey));
        public override Guid? InitiatorId => _initiatorId ?? (_initiatorId = GetHeaderGuid(BinaryMessageSerializer.InitiatorIdKey));
        public override DateTime? ExpirationTime => GetHeaderDateTime(BinaryMessageSerializer.ExpirationTimeKey);
        public override Uri SourceAddress => _sourceAddress ?? (_sourceAddress = GetHeaderUri(BinaryMessageSerializer.SourceAddressKey));
        public override Uri DestinationAddress => _destinationAddress ?? (_destinationAddress = GetHeaderUri(BinaryMessageSerializer.DestinationAddressKey));
        public override Uri ResponseAddress => _responseAddress ?? (_responseAddress = GetHeaderUri(BinaryMessageSerializer.ResponseAddressKey));
        public override Uri FaultAddress => _faultAddress ?? (_faultAddress = GetHeaderUri(BinaryMessageSerializer.FaultAddressKey));
        public override DateTime? SentTime => _sentTime ?? (_sentTime = GetHeaderDateTime(BinaryMessageSerializer.SentTimeKey));
        public override Headers Headers => _headers ?? (_headers = new StaticHeaders(_binaryHeaders));
        public override HostInfo Host => _host ?? (_host = GetHeaderObject<HostInfo>(BinaryMessageSerializer.HostInfoKey));
        public override IEnumerable<string> SupportedMessageTypes => _supportedTypes;

        IEnumerable<string> GetSupportedMessageTypes()
        {
            yield return GetHeaderString(BinaryMessageSerializer.MessageTypeKey);
            var header = GetHeaderString(BinaryMessageSerializer.PolymorphicMessageTypesKey);
            if (header != null)
            {
                string[] additionalMessageUrns = header.Split(';');
                foreach (var additionalMessageUrn in additionalMessageUrns)
                {
                    yield return additionalMessageUrn;
                }
            }
        }

        public override bool HasMessageType(Type messageType)
        {
            lock (_messageTypes)
            {
                object existing;
                if (_messageTypes.TryGetValue(messageType, out existing))
                    return existing != null;
            }

            var typeUrn = new MessageUrn(messageType).ToString();

            return _supportedTypes.Any(x => typeUrn.Equals(x, StringComparison.OrdinalIgnoreCase));
        }

        public override bool TryGetMessage<T>(out ConsumeContext<T> message)
        {
            lock (_messageTypes)
            {
                object existing;
                if (_messageTypes.TryGetValue(typeof(T), out existing))
                {
                    message = existing as ConsumeContext<T>;
                    return message != null;
                }

                var typeUrn = new MessageUrn(typeof(T)).ToString();

                if (_supportedTypes.Any(typeUrn.Equals))
                {
                    if (_message is T)
                    {
                        _messageTypes[typeof(T)] = message = new MessageConsumeContext<T>(this, (T)_message);
                        return true;
                    }

                    message = null;
                    return false;
                }

                _messageTypes[typeof(T)] = message = null;
                return false;
            }
        }

        string GetHeaderString(string headerName)
        {
            var header = GetHeader(headerName);
            if (header == null)
                return null;

            var s = header as string;
            if (s != null)
                return s;

            var uri = header as Uri;
            if (uri != null)
                return uri.ToString();

            return header.ToString();
        }

        Uri GetHeaderUri(string headerName)
        {
            try
            {
                var header = GetHeader(headerName);
                if (header == null)
                    return null;

                var uri = header as Uri;
                if (uri != null)
                    return uri;

                var s = header as string;
                if (s != null)
                    return new Uri(s);
            }
            catch (UriFormatException)
            {
            }

            return null;
        }

        T GetHeaderObject<T>(string headerName)
            where T : class
        {
            var header = GetHeader(headerName);

            var obj = header as T;

            return obj;
        }

        Guid? GetHeaderGuid(string headerName)
        {
            try
            {
                var header = GetHeader(headerName);
                if (header == null)
                    return default(Guid?);

                if (header is Guid)
                    return (Guid)header;

                var s = header as string;
                if (s != null)
                    return new Guid(s);
            }
            catch (FormatException)
            {
            }

            return default(Guid?);
        }

        DateTime? GetHeaderDateTime(string headerName)
        {
            try
            {
                var header = GetHeader(headerName);
                if (header == null)
                    return default(DateTime?);

                if (header is DateTime)
                    return (DateTime)header;

                var s = header as string;
                if (s != null)
                    return DateTime.Parse(s);
            }
            catch (FormatException)
            {
            }

            return default(DateTime?);
        }

        object GetHeader(string headerName)
        {
            return _binaryHeaders.Where(x => x.Name == headerName).Select(x => x.Value).FirstOrDefault();
        }
    }
}